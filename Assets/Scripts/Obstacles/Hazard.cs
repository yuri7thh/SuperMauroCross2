using UnityEngine;

public class Hazard : MonoBehaviour
{
    [Tooltip("Mata o jogador ao tocar.")]
    public bool killOnContact = true;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!killOnContact) return;
        if (other.CompareTag("Player") && GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }
}
