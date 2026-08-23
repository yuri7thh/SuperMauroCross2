using UnityEngine;

public class ParticleMover : MonoBehaviour, IPoolable
{
    public Vector2 velocity;
    public float life = 0.4f;

    private float _t;
    private SpriteRenderer _sr;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    public void OnSpawned()
    {
        _t = 0f;
        if (_sr != null)
        {
            var c = _sr.color;
            c.a = 1f;
            _sr.color = c;
        }
    }

    public void OnDespawned() { }

    void Update()
    {
        _t += Time.deltaTime;
        transform.position += (Vector3)velocity * Time.deltaTime;
        velocity *= 0.9f;

        if (_sr != null)
        {
            var c = _sr.color;
            c.a = Mathf.Clamp01(1f - _t / life);
            _sr.color = c;
        }

        if (_t >= life)
        {
            if (ObjectPool.Instance != null)
                ObjectPool.Instance.Despawn(gameObject);
            else
                Destroy(gameObject);
        }
    }
}
