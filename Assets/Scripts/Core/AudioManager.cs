using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Geral")]
    [SerializeField] private bool _audioEnabled = true;
    [Range(0f, 1f)] public float volume = 0.7f;
    [Range(0f, 1f)] public float musicVolume = 0.5f;

    [Header("Musica")]
    [Tooltip("Lista de musicas de fundo. Uma toca por vez, variando entre si.")]
    public AudioClip[] musicTracks;

    [Header("Abafada na morte")]
    [Tooltip("Se ligado, a musica fica abafada (passa-baixa) ao morrer.")]
    public bool muffleOnDeath = true;
    [Tooltip("Frequencia de corte (Hz) quando abafada.")]
    public float muffledCutoff = 500f;
    [Tooltip("Frequencia de corte (Hz) normal.")]
    public float normalCutoff = 22000f;
    [Tooltip("Velocidade (Hz/s) da transicao de abafada.")]
    public float muffleSpeed = 12000f;

    [Header("Sons de Pulo")]
    [Tooltip("Lista de sons. Um e escolhido aleatoriamente a cada pulo. Se vazio, usa o som generico.")]
    public AudioClip[] jumpSounds;

    [Header("Sons de Parry (acerto)")]
    [Tooltip("Lista de sons. Um e escolhido aleatoriamente a cada parry acertado. Se vazio, usa o som generico.")]
    public AudioClip[] parrySounds;

    [Header("Sons de Parry (input)")]
    [Tooltip("Lista de sons. Se vazio, usa o som generico.")]
    public AudioClip[] parryInputSounds;

    [Header("Sons de Morte")]
    [Tooltip("Lista de sons. Um e escolhido aleatoriamente ao morrer. Se vazio, usa o som generico.")]
    public AudioClip[] deathSounds;

    [Header("Sons genericos (fallback)")]
    [Tooltip("Se ligado, quando a lista de sons de uma acao estiver vazia, toca um som generico (beep) no lugar.")]
    public bool useGenericFallback = true;
    public float jumpFreq = 500f;
    public float jumpDur = 0.12f;
    public float parryFreq = 880f;
    public float parryDur = 0.1f;
    public float parryInputFreq = 660f;
    public float parryInputDur = 0.06f;
    public float deathFreq = 220f;
    public float deathDur = 0.35f;

    public bool audioEnabled
    {
        get => _audioEnabled;
        set
        {
            _audioEnabled = value;
            if (value) RefreshMusic();
            else StopMusic();
        }
    }

    private AudioSource _sfxSource;
    private AudioSource _musicSource;
    private AudioLowPassFilter _lowPass;
    private int _lastMusicIndex = -1;
    private float _targetCutoff = 22000f;
    private GameState _lastState;
    private bool _stateInitialized;

    private AudioClip _jumpGeneric;
    private AudioClip _parryGeneric;
    private AudioClip _parryInputGeneric;
    private AudioClip _deathGeneric;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.playOnAwake = false;

        _musicSource = gameObject.AddComponent<AudioSource>();
        _musicSource.playOnAwake = false;
        _musicSource.loop = false;

        _lowPass = gameObject.AddComponent<AudioLowPassFilter>();
        _lowPass.enabled = true;
        _lowPass.cutoffFrequency = normalCutoff;
        _targetCutoff = normalCutoff;

        _jumpGeneric = GenerateClip(jumpFreq, jumpDur);
        _parryGeneric = GenerateClip(parryFreq, parryDur);
        _parryInputGeneric = GenerateClip(parryInputFreq, parryInputDur);
        _deathGeneric = GenerateClip(deathFreq, deathDur);
    }

    void Update()
    {
        if (_lowPass != null)
        {
            _lowPass.cutoffFrequency = Mathf.MoveTowards(_lowPass.cutoffFrequency, _targetCutoff, muffleSpeed * Time.deltaTime);
        }

        PlayMusic();

        var gm = GameManager.Instance;
        GameState current = gm != null ? gm.State : GameState.Menu;
        if (!_stateInitialized || current != _lastState)
        {
            _stateInitialized = true;
            _lastState = current;
            RefreshMusic();
        }
    }

    private void RefreshMusic()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        if (gm.State == GameState.GameOver && muffleOnDeath)
            SetMuffled(true);
        else
            SetMuffled(false);

        PlayMusic();
    }

    public void PlayJump() => Play(jumpSounds, _jumpGeneric);
    public void PlayParry() => Play(parrySounds, _parryGeneric);
    public void PlayParryInput() => Play(parryInputSounds, _parryInputGeneric);
    public void PlayDeath() => Play(deathSounds, _deathGeneric);

    public void PlayMusic()
    {
        if (!_audioEnabled || _musicSource == null) return;
        if (musicTracks == null || musicTracks.Length == 0) return;
        if (_musicSource.isPlaying) return;
        PlayNextTrack();
    }

    private void PlayNextTrack()
    {
        if (musicTracks == null || musicTracks.Length == 0) return;

        int index;
        if (musicTracks.Length == 1)
        {
            index = 0;
        }
        else
        {
            index = Random.Range(0, musicTracks.Length);
            if (index == _lastMusicIndex) index = (index + 1) % musicTracks.Length;
        }

        _lastMusicIndex = index;
        var clip = musicTracks[index];
        if (clip == null) return;

        _musicSource.clip = clip;
        _musicSource.volume = musicVolume;
        _musicSource.Play();
    }

    public void StopMusic()
    {
        if (_musicSource != null && _musicSource.isPlaying)
            _musicSource.Stop();
    }

    public void SetMusicVolume(float v)
    {
        musicVolume = v;
        if (_musicSource != null) _musicSource.volume = v;
    }

    private void SetMuffled(bool muffled)
    {
        _targetCutoff = muffled ? muffledCutoff : normalCutoff;
    }

    private void Play(AudioClip[] clips, AudioClip generic)
    {
        if (!_audioEnabled || _sfxSource == null) return;

        AudioClip clip = null;
        if (clips != null && clips.Length > 0)
            clip = clips[Random.Range(0, clips.Length)];

        if (clip == null && useGenericFallback)
            clip = generic;

        if (clip == null) return;
        _sfxSource.PlayOneShot(clip, volume);
    }

    private AudioClip GenerateClip(float frequency, float duration)
    {
        int sampleRate = 44100;
        int samples = Mathf.CeilToInt(sampleRate * duration);
        if (samples < 1) samples = 1;
        var data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = 1f - (float)i / samples;
            data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * 0.5f;
        }

        var clip = AudioClip.Create("generic", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}