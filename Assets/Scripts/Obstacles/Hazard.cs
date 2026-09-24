using UnityEngine;

public class Hazard : MonoBehaviour
{
    [Tooltip("Mata o jogador ao tocar.")]
    public bool killOnContact = true;
    [Tooltip("Se ligado, e destruido quando atingido pelo jogador durante o dash.")]
    public bool destructibleByDash = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (destructibleByDash)
        {
            var pc = other.GetComponentInParent<PlayerController>();
            if (pc != null && pc.IsDashing)
            {
                Destroy(gameObject);
                return;
            }
        }

        if (!killOnContact) return;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }
}
