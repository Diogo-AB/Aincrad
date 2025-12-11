using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class HandDataReceiver : MonoBehaviour
{
    UdpClient client;
    IPEndPoint remoteEndPoint;
    public string handState;
    public string movement;

    [Header("Referência do Objeto AR (do Vuforia)")]
    public Transform arObject;

    [Header("Ajustes de movimento")]
    public float rotationSpeed = 80f;
    public float scaleSpeed = 1f;
    public float minScale = 0.3f;
    public float maxScale = 2f;

    void Start()
    {
        client = new UdpClient(5005);
        remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
        client.BeginReceive(ReceiveData, null);
    }

    void ReceiveData(IAsyncResult ar)
    {
        byte[] received = client.EndReceive(ar, ref remoteEndPoint);
        string message = Encoding.UTF8.GetString(received);

        try
        {
            var data = JsonUtility.FromJson<HandData>(message);
            handState = data.hand_state;
            movement = data.movement;
        }
        catch (Exception e)
        {
            Debug.LogError("Erro UDP: " + e.Message);
        }

        client.BeginReceive(ReceiveData, null);
    }

    void Update()
    {
        if (arObject == null)
        {
            Debug.LogWarning("AR Object não atribuído no HandDataReceiver!");
            return;
        }

        // Debug
        Debug.Log("Movement: " + movement + " | Hand: " + handState);

        // ----------- ROTACIONAR PELO MOVIMENTO -----------
        string move = movement.ToLower();

        if (move.Contains("direita"))
            arObject.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        if (move.Contains("esquerda"))
            arObject.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime);

        if (move.Contains("cima"))
            arObject.Rotate(Vector3.right, -rotationSpeed * Time.deltaTime);

        if (move.Contains("baixo"))
            arObject.Rotate(Vector3.right, rotationSpeed * Time.deltaTime);

        // ----------- ESCALA PELA MÃO ABERTA/FECHADA -----------
        string hand = handState.ToLower();
        Vector3 scale = arObject.localScale;

        if (hand == "aberta")
            scale += Vector3.one * scaleSpeed * Time.deltaTime;

        if (hand == "fechada")
            scale -= Vector3.one * scaleSpeed * Time.deltaTime;

        // limitar escala
        scale.x = Mathf.Clamp(scale.x, minScale, maxScale);
        scale.y = Mathf.Clamp(scale.y, minScale, maxScale);
        scale.z = Mathf.Clamp(scale.z, minScale, maxScale);

        arObject.localScale = scale;
    }

    [Serializable]
    public class HandData
    {
        public string hand_state;
        public string movement;
    }
}
