using UnityEngine;

public class HandController : MonoBehaviour
{
    public HandDataReceiver handReceiver; // script que recebe os dados do Python
    public Transform arObject;        // objeto do vuforia a ser movido

    public float rotationSpeed = 50f;
    public float scaleSpeed = 1f;
    public float minScale = 0.3f;
    public float maxScale = 2f;

    void Update()
    {
        Debug.Log("Movement: " + handReceiver.movement +
              " | Hand: " + handReceiver.handState);
        if (handReceiver == null || arObject == null)
            return;

        // Rotação pela posição
        string move = handReceiver.movement.ToLower();

        if (move.Contains("direita"))
            arObject.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        if (move.Contains("esquerda"))
            arObject.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime);

        if (move.Contains("cima"))
            arObject.Rotate(Vector3.right, -rotationSpeed * Time.deltaTime);

        if (move.Contains("baixo"))
            arObject.Rotate(Vector3.right, rotationSpeed * Time.deltaTime);

        // Escala pela mão aberta/fechada
        string hand = handReceiver.handState.ToLower();
        Vector3 scale = arObject.localScale;

        if (hand == "aberta")
            scale += Vector3.one * scaleSpeed * Time.deltaTime;

        if (hand == "fechada")
            scale -= Vector3.one * scaleSpeed * Time.deltaTime;

        // limitar
        scale = new Vector3(
            Mathf.Clamp(scale.x, minScale, maxScale),
            Mathf.Clamp(scale.y, minScale, maxScale),
            Mathf.Clamp(scale.z, minScale, maxScale)
        );

        arObject.localScale = scale;
    }
}
