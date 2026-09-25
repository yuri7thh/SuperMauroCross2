using System.Collections;
using UnityEngine;

public class FeedbackManager : MonoBehaviour
{
    [Header("Parry - Slow motion")]
    public bool slowMoEnabled = true;
    [Range(0f, 1f)] public float slowMoTimeScale = 0.2f;
    public float slowMoDuration = 0.15f;

    [Header("Parry - Screen shake")]
    public bool shakeEnabled = true;
    public float shakeIntensity = 0.4f;
    public float shakeDuration = 0.25f;

    [Header("Parry - Particulas")]
    public bool particlesEnabled = true;
    public int particleCount = 12;
    public float particleLife = 0.4f;
    public float particleSpeed = 6f;
    public Color particleColor = Color.white;
    [Tooltip("Template de particula (criado automaticamente se vazio).")]
    public GameObject particleTemplate;

    [Header("Popup de pontos")]
    [Tooltip("Fonte do popup de pontos do parry. Se vazio, tenta carregar rawpixel-bold e depois a fonte embutida.")]
    public Font scorePopupFont;

    private CameraController _camCtrl;
    private GameObject _scorePopupTemplate;

    void Awake()
    {
        if (Camera.main != null)
            _camCtrl = Camera.main.GetComponent<CameraController>();
    }

    public void TriggerParryFeedback(Vector3 position)
    {
        if (slowMoEnabled) StartCoroutine(SlowMoRoutine());
        if (shakeEnabled && _camCtrl != null) _camCtrl.Shake(shakeDuration, shakeIntensity);
        if (particlesEnabled) SpawnParticles(position);
    }

    public void TriggerDeathFeedback()
    {
        if (shakeEnabled && _camCtrl != null) _camCtrl.Shake(0.5f, 0.7f);
    }

    public void ShowScorePopup(Vector3 position, int points)
    {
        if (_scorePopupTemplate == null) CreateScorePopupTemplate();

        GameObject obj;
        if (ObjectPool.Instance != null)
        {
            obj = ObjectPool.Instance.Spawn(_scorePopupTemplate, position, Quaternion.identity);
        }
        else
        {
            obj = Instantiate(_scorePopupTemplate, position, Quaternion.identity);
            obj.SetActive(true);
        }

        var popup = obj.GetComponent<ScorePopup>();
        if (popup != null) popup.SetPoints(points);
    }

    private void CreateScorePopupTemplate()
    {
        var t = new GameObject("ScorePopupTemplate");
        var tm = t.AddComponent<TextMesh>();
        tm.font = GetScorePopupFont();
        tm.fontSize = 48;
        tm.characterSize = 0.12f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = Color.white;

        var renderer = t.GetComponent<MeshRenderer>();
        if (renderer != null) renderer.sortingOrder = 100;

        t.AddComponent<ScorePopup>();
        t.SetActive(false);
        _scorePopupTemplate = t;
    }

    private Font GetScorePopupFont()
    {
        if (scorePopupFont != null) return scorePopupFont;

        var font = Resources.Load<Font>("Fonts/rawpixel-bold");
        if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return font;
    }

    private IEnumerator SlowMoRoutine()
    {
        Time.timeScale = slowMoTimeScale;
        float t = 0f;
        while (t < slowMoDuration)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        Time.timeScale = 1f;
    }

    private void SpawnParticles(Vector3 position)
    {
        if (particleTemplate == null) CreateParticleTemplate();

        for (int i = 0; i < particleCount; i++)
        {
            GameObject p;
            if (ObjectPool.Instance != null)
            {
                p = ObjectPool.Instance.Spawn(particleTemplate, position, Quaternion.identity);
            }
            else
            {
                p = Instantiate(particleTemplate, position, Quaternion.identity);
                p.SetActive(true);
            }

            var sr = p.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = particleColor;

            var mover = p.GetComponent<ParticleMover>();
            var dir = Random.insideUnitCircle.normalized;
            mover.velocity = dir * particleSpeed;
            mover.life = particleLife;
        }
    }

    private void CreateParticleTemplate()
    {
        var t = new GameObject("ParryParticleTemplate");
        var sr = t.AddComponent<SpriteRenderer>();
        sr.sprite = PlaceholderFactory.WhiteSprite;
        sr.color = particleColor;
        t.transform.localScale = new Vector3(0.18f, 0.18f, 1f);
        t.AddComponent<ParticleMover>();
        t.SetActive(false);
        particleTemplate = t;
    }
}
