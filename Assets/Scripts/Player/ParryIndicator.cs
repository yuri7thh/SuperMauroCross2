using UnityEngine;

public class ParryIndicator : MonoBehaviour
{
    private GameObject _ring;
    private SpriteRenderer _ringSr;
    private PlayerController _player;
    private float _flashTimer;
    private bool _lastShow;
    private bool _loggedTarget;
    private bool _loggedShow;

    void Awake()
    {
        _player = GetComponent<PlayerController>();

        _ring = new GameObject("ParryRing");
        _ring.transform.SetParent(transform.parent);
        _ringSr = _ring.AddComponent<SpriteRenderer>();
        _ringSr.sprite = CreateRingSprite();
        _ringSr.sortingOrder = 100;

        var mat = GetUnlitMaterial();
        if (mat != null) _ringSr.material = mat;

        _ring.SetActive(false);

        Debug.Log("[ParryIndicator] criado. sprite=" + (_ringSr.sprite != null ? _ringSr.sprite.name : "NULL")
            + " | material=" + (_ringSr.material != null ? _ringSr.material.shader.name : "NULL")
            + " | sortOrder=" + _ringSr.sortingOrder);
    }

    void OnDestroy()
    {
        if (_ring != null) Destroy(_ring);
    }

    public void Flash()
    {
        _flashTimer = 0.15f;
    }

    void Update()
    {
        if (_player == null || _ring == null) return;

        var gm = GameManager.Instance;
        bool show = gm != null && gm.State == GameState.Playing
            && _player.IsAirborne
            && _player.Config.showParryIndicator;

        if (show != _lastShow)
        {
            Debug.Log("[ParryIndicator] " + (show ? "MOSTRANDO (no ar)" : "OCULTANDO") + " | noAr=" + _player.IsAirborne);
            _lastShow = show;
        }

        _ring.SetActive(show);
        if (!show) return;

        _flashTimer -= Time.deltaTime;

        _ring.transform.position = transform.position;
        float r = _player.Config.parryRange;
        float scale = _flashTimer > 0f ? r * 1.2f : r;
        _ring.transform.localScale = new Vector3(scale, scale, 1f);

        if (!_loggedShow)
        {
            _loggedShow = true;
            Debug.Log("[ParryIndicator] diagnostico: isVisible=" + _ringSr.isVisible
                + " | pos=" + _ring.transform.position
                + " | scale=" + _ring.transform.localScale
                + " | bounds=" + _ringSr.bounds);
        }

        bool hasTarget = HasProjectileInRange(r);
        if (hasTarget && !_loggedTarget)
        {
            Debug.Log("[ParryIndicator] PROJETIL NO ALCANCE - aperte K agora!");
            _loggedTarget = true;
        }
        else if (!hasTarget)
        {
            _loggedTarget = false;
        }

        _ringSr.color = hasTarget
            ? new Color(0.2f, 1f, 0.35f, 1f)
            : (_flashTimer > 0f ? new Color(1f, 0.9f, 0.3f, 1f) : new Color(0.4f, 0.85f, 1f, 1f));
    }

    private bool HasProjectileInRange(float r)
    {
        var cols = Physics2D.OverlapCircleAll(transform.position, r);
        foreach (var c in cols)
        {
            if (c.GetComponentInParent<Projectile>() != null) return true;
        }
        return false;
    }

    private static Material _unlitMaterial;

    private static Material GetUnlitMaterial()
    {
        if (_unlitMaterial != null) return _unlitMaterial;

        var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default");
        if (shader != null) _unlitMaterial = new Material(shader);
        return _unlitMaterial;
    }

    private static Sprite _ringSprite;

    private static Sprite CreateRingSprite()
    {
        if (_ringSprite != null) return _ringSprite;

        int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var center = new Vector2((size - 1) / 2f, (size - 1) / 2f);
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), center);
                float a = 0f;
                if (d <= radius - 14f) a = 0.35f;
                else if (d <= radius) a = 1f;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        tex.Apply();
        _ringSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64f);
        return _ringSprite;
    }
}