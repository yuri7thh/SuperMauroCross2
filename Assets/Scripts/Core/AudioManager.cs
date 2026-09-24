using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Geral")]
    [SerializeField] private bool _audioEnabled = true;
    [Range(0f, 1f)] public float volume = 0.7f;
    [Range(0f, 1f)] public float musicVolume = 0.5f;

    [Header("Musica")]
    [Tooltip("Musica de fundo que toca em loop enquanto o jogador esta jogando.")]
    public AudioClip music;

    [Header("Sons de Pulo")]
    [Tooltip("Lista de sons. Um e escolhido aleatoriamente a cada pulo.")]
    public AudioClip[] jumpSounds;

    [Header("Sons de Parry (acerto)")]
    [Tooltip("Lista de sons. Um e escolhido aleatoriamente a cada parry acertado.")]
    public AudioClip[] parrySounds;

    [Header("Sons de Parry (input)")]
    public AudioClip[] parryInputSounds;

    [Header("Sons de Morte")]
    [Tooltip("Lista de sons. Um e escolhido aleatoriamente ao morrer.")]
    public AudioClip[] deathSounds;

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

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.playOnAwake = false;

        _musicSource = gameObject.AddComponent<AudioSource>();
        _musicSource.playOnAwake = false;
        _musicSource.loop = true;
    }

    void Start()
    {
        var gm = GameManager.Instance;
        if (gm != null) gm.OnStateChanged += OnStateChanged;
        RefreshMusic();
    }

    void OnDestroy()
    {
        var gm = GameManager.Instance;
        if (gm != null) gm.OnStateChanged -= OnStateChanged;
    }

    private void OnStateChanged(GameState state) => RefreshMusic();

    private void RefreshMusic()
    {
        var gm = GameManager.Instance;
        bool playing = gm != null && gm.State == GameState.Playing;
        if (playing) PlayMusic();
        else StopMusic();
    }

    public void PlayJump() => PlayRandom(jumpSounds);
    public void PlayParry() => PlayRandom(parrySounds);
    public void PlayParryInput() => PlayRandom(parryInputSounds);
    public void PlayDeath() => PlayRandom(deathSounds);

    public void PlayMusic()
    {
        if (!_audioEnabled || music == null || _musicSource == null) return;
        if (_musicSource.isPlaying && _musicSource.clip == music) return;
        _musicSource.clip = music;
        _musicSource.volume = musicVolume;
        _musicSource.Play();
    }

    public void StopMusic()
    {
        if (_musicSource != null && _musicSource.isPlaying)
            _musicSource.Stop();
    }

    private void PlayRandom(AudioClip[] clips)
    {
        if (!_audioEnabled || clips == null || clips.Length == 0 || _sfxSource == null) return;
        var clip = clips[Random.Range(0, clips.Length)];
        if (clip == null) return;
        _sfxSource.PlayOneShot(clip, volume);
    }
}
