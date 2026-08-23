using UnityEngine;

public class GroundEnemy : MonoBehaviour
{
    [Tooltip("Velocidade do inimigo se movendo para a esquerda (em direcao ao jogador).")]
    public float speed = 4f;

    void Update()
    {
        transform.position += Vector3.left * speed * Time.deltaTime;
    }
}
