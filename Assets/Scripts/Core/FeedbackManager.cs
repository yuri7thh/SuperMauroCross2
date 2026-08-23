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

    private CameraController _camCtrl;

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
