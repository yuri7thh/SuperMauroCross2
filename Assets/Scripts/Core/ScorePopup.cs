using UnityEngine;

public class ScorePopup : MonoBehaviour, IPoolable
{
    public float riseSpeed = 1.5f;
    public float life = 0.8f;
    public Color color = Color.white;

    private float _t;
    private TextMesh _text;

    void Awake()
    {
        _text = GetComponent<TextMesh>();
    }

    public void OnSpawned()
    {
        _t = 0f;
        if (_text == null) _text = GetComponent<TextMesh>();
        if (_text != null) _text.color = color;
    }

    public void OnDespawned() { }

    public void SetPoints(int points)
    {
        if (_text != null) _text.text = "+" + points;
    }

    void Update()
    {
        _t += Time.deltaTime;
        transform.position += Vector3.up * (riseSpeed * Time.deltaTime);

        if (_text != null)
        {
            var c = _text.color;
            c.a = Mathf.Clamp01(1f - _t / life);
            _text.color = c;
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
