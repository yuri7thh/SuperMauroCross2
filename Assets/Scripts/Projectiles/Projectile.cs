using UnityEngine;

public class Projectile : MonoBehaviour, IPoolable
{
    [Tooltip("Velocidade do projetil para a esquerda.")]
    public float speed = 9f;

    public bool IsParried { get; private set; }

    private Transform _player;

    void Awake()
    {
        var cfg = Resources.Load<ProjectileConfig>("Configs/ProjectileConfig");
        if (cfg != null) speed = cfg.speed;

        var pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) _player = pc.transform;
    }

    public void OnSpawned()
    {
        IsParried = false;
    }

    public void OnDespawned() { }

    void Update()
    {
        transform.position += Vector3.left * speed * Time.deltaTime;

        if (_player != null && transform.position.x < _player.position.x - 25f)
        {
            Despawn();
        }
    }

    public void Parry()
    {
        if (IsParried) return;
        IsParried = true;
        Despawn();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (IsParried) return;
        if (other.CompareTag("Player") && GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }

    private void Despawn()
    {
        if (ObjectPool.Instance != null)
            ObjectPool.Instance.Despawn(gameObject);
        else
            Destroy(gameObject);
    }
}
