using UnityEngine;

public class ParryIndicator : MonoBehaviour
{
    private const float LineHeight = 0.25f;

    private GameObject _ring;
    private SpriteRenderer _ringSr;
    private GameObject _lineGo;
    private SpriteRenderer _lineSr;
    private PlayerController _player;
    private float _flashTimer;
    private bool _lastShow;
    private bool _loggedShow;
    private bool _loggedTarget;

    void Awake()
    {
        _player = GetComponent<PlayerController>();

        _ring = new GameObject("ParryRing");
        _ring.transform.SetParent(transform.parent);
        _ringSr = _ring.AddComponent<SpriteRenderer>();
        _ringSr.sprite = CreateRingSprite();
        _ringSr.sortingOrder = 100;
        _ring.SetActive(false);

        _lineGo = new GameObject("ParryLine");
        _lineGo.transform.SetParent(transform.parent);
        _lineSr = _lineGo.AddComponent<SpriteRenderer>();
        _lineSr.sprite = CreateDashSprite();
        _lineSr.drawMode = SpriteDrawMode.Tiled;
        _lineSr.sortingOrder = 99;
        _lineGo.SetActive(false);

        var mat = GetUnlitMaterial();
        if (mat != null)
        {
            _ringSr.material = mat;
            _lineSr.material = mat;
        }

        Debug.Log("[ParryIndicator] criado. sprite=" + (_ringSr.sprite != null ? _ringSr.sprite.name : "NULL")
            + " | material=" + (_ringSr.material != null ? _ringSr.material.shader.name : "NULL")
            + " | sortOrder=" + _ringSr.sortingOrder);
    }

    void OnDestroy()
    {
        if (_ring != null) Destroy(_ring);
        if (_lineGo != null) Destroy(_lineGo);
    }

    public void Flash()
    {
        _flashTimer = 0.15f;
    }

    void Update()
    {
        if (_player == null) return;

        var gm = GameManager.Instance;
        bool show = gm != null && gm.State == GameState.Playing
            && _player.IsAirborne
            && _player.Config.showParryIndicator;

        if (_player.Config.parryIndicatorMode == PlayerConfig.ParryIndicatorMode.LineToTarget)
        {
            UpdateLine(show);
            if (_ring != null) _ring.SetActive(false);
            return;
        }

        UpdateCircle(show);
    }

    private void UpdateCircle(bool show)
    {
        if (_ring == null) return;

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

        bool hasTarget = _player.FindNearestParryTarget() != null;
        if (hasTarget && !_loggedTarget)
        {
            Debug.Log("[ParryIndicator] ALVO DE PARRY NO ALCANCE - aperte K agora!");
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

    private void UpdateLine(bool show)
    {
        if (_lineGo == null) return;

        if (!show)
        {
            _lineGo.SetActive(false);
            _lastShow = false;
            return;
        }

        var cfg = _player.Config;
        var target = _player.FindNearestParryTarget();
        if (target == null)
        {
            _lineGo.SetActive(false);
            if (_lastShow)
            {
                Debug.Log("[ParryIndicator] OCULTANDO linha (sem alvo)");
                _lastShow = false;
            }
            return;
        }

        if (!_lastShow)
        {
            Debug.Log("[ParryIndicator] MOSTRANDO linha -> " + target.name);
            _lastShow = true;
        }

        _flashTimer -= Time.deltaTime;

        Vector2 start = transform.position;
        Vector2 end = target.bounds.ClosestPoint(start);
        float dist = Vector2.Distance(start, end);

        float t = Mathf.InverseLerp(cfg.parryRange, 0f, dist);
        float alpha = Mathf.Clamp01(cfg.parryLineOpacityCurve.Evaluate(t));

        Color color = cfg.parryLineColor;
        if (_flashTimer > 0f) color = Color.Lerp(color, Color.white, 0.6f);
        color.a = alpha;

        if (dist < 0.05f) dist = 0.05f;
        _lineGo.SetActive(true);
        _lineGo.transform.position = start;
        _lineGo.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg);
        _lineSr.size = new Vector2(dist, LineHeight);
        _lineSr.color = color;
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

    private static Sprite _dashSprite;

    private static Sprite CreateDashSprite()
    {
        if (_dashSprite != null) return _dashSprite;

        int w = 64, h = 16;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Repeat;

        int dashEnd = 34;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                bool inDash = x < dashEnd;
                bool inside = y >= 4 && y <= 11;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, (inDash && inside) ? 1f : 0f));
            }
        }
        tex.Apply();
        _dashSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0f, 0.5f), 64f);
        return _dashSprite;
    }
}