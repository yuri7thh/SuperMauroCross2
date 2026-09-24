using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Geral")]
    public bool audioEnabled = true;
    [Range(0f, 1f)] public float volume = 0.7f;

    [Header("Pulo")]
    public float jumpFrequency = 500f;
    public float jumpDuration = 0.12f;

    [Header("Parry")]
    public float parryFrequency = 880f;
    public float parryDuration = 0.1f;

    [Header("Parry Input")]
    public float parryInputFrequency = 660f;
    public float parryInputDuration = 0.08f;

    [Header("Morte")]
    public float deathFrequency = 220f;
    public float deathDuration = 0.35f;

    private AudioSource _source;
    private AudioClip _jumpClip;
    private AudioClip _parryClip;
    private AudioClip _parryInputClip;
    private AudioClip _deathClip;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        _source = gameObject.AddComponent<AudioSource>();
        _source.playOnAwake = false;
        _jumpClip = GenerateClip(jumpFrequency, jumpDuration);
        _parryClip = GenerateClip(parryFrequency, parryDuration);
        _parryInputClip = GenerateClip(parryInputFrequency, parryInputDuration);
        _deathClip = GenerateClip(deathFrequency, deathDuration);
    }

    public void PlayJump() => Play(_jumpClip);
    public void PlayParry() => Play(_parryClip);
    public void PlayParryInput() => Play(_parryInputClip);
    public void PlayDeath() => Play(_deathClip);

    private void Play(AudioClip clip)
    {
        if (!audioEnabled || clip == null || _source == null) return;
        _source.PlayOneShot(clip, volume);
    }

    private AudioClip GenerateClip(float frequency, float duration)
    {
        int sampleRate = 44100;
        int samples = Mathf.CeilToInt(sampleRate * duration);
        var data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = 1f - (float)i / samples;
            data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * 0.5f;
        }

        var clip = AudioClip.Create("sfx", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
