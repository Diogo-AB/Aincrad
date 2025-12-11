using UnityEngine;

public class CenterOnScreen : MonoBehaviour
{
    public void MoveToCenter()
    {
        // Pega o meio da tela
        Vector3 center = new Vector3(Screen.width / 2f, Screen.height / 2f, 1f);

        // Converte posição da tela para o mundo da câmera
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(center);

        // Move o objeto para essa posição
        transform.position = worldPos;
    }
}
