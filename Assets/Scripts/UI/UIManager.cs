using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Paineis")]
    public GameObject menuPanel;
    public GameObject hudPanel;
    public GameObject gameOverPanel;
    public GameObject optionsPanel;

    [Header("HUD")]
    public Text distanceText;
    public Text scoreText;
    public Text comboText;

    [Header("Game Over")]
    public Text finalScoreText;
    public Text bestScoreText;

    [Header("Menu - botoes")]
    public Button playButton;
    public Button optionsButton;

    [Header("Game Over - botoes")]
    public Button restartButton;
    public Button menuButton;

    [Header("Opcoes")]
    public Toggle parryToggle;
    public Toggle comboToggle;
    public Toggle projectilesToggle;
    public Toggle slowMoToggle;
    public Toggle shakeToggle;
    public Toggle particlesToggle;
    public Toggle soundToggle;
    public Slider volumeSlider;
    public Button backButton;

    private GameManager _game;
    private AudioManager _audio;
    private FeedbackManager _feedback;

    void Start()
    {
        _game = GameManager.Instance;
        _audio = AudioManager.Instance;
        _feedback = FindFirstObjectByType<FeedbackManager>();

        WireButtons();
        WireToggles();

        if (_game != null)
        {
            _game.OnStateChanged += OnStateChanged;
            _game.OnScoreChanged += OnScoreChanged;
            _game.OnComboChanged += OnComboChanged;
        }

        LoadSettings();
        OnStateChanged(_game != null ? _game.State : GameState.Menu);
    }

    void OnDestroy()
    {
        if (_game != null)
        {
            _game.OnStateChanged -= OnStateChanged;
            _game.OnScoreChanged -= OnScoreChanged;
            _game.OnComboChanged -= OnComboChanged;
        }
    }

    private void WireButtons()
    {
        if (playButton != null) playButton.onClick.AddListener(StartGame);
        if (optionsButton != null) optionsButton.onClick.AddListener(() => ShowOptions(true));
        if (restartButton != null) restartButton.onClick.AddListener(Restart);
        if (menuButton != null) menuButton.onClick.AddListener(Restart);
        if (backButton != null) backButton.onClick.AddListener(() => ShowOptions(false));
    }

    private void WireToggles()
    {
        if (parryToggle != null) parryToggle.onValueChanged.AddListener(SetParry);
        if (comboToggle != null) comboToggle.onValueChanged.AddListener(SetCombo);
        if (projectilesToggle != null) projectilesToggle.onValueChanged.AddListener(SetProjectiles);
        if (slowMoToggle != null) slowMoToggle.onValueChanged.AddListener(SetSlowMo);
        if (shakeToggle != null) shakeToggle.onValueChanged.AddListener(SetShake);
        if (particlesToggle != null) particlesToggle.onValueChanged.AddListener(SetParticles);
        if (soundToggle != null) soundToggle.onValueChanged.AddListener(SetSound);
        if (volumeSlider != null) volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    private void StartGame() { if (_game != null) _game.StartGame(); }
    private void Restart() { if (_game != null) _game.Restart(); }

    private void SetParry(bool v) { if (_game != null) { _game.ParryEnabled = v; PlayerPrefs.SetInt("SMC_parry", v ? 1 : 0); } }
    private void SetCombo(bool v) { if (_game != null) { _game.ComboEnabled = v; PlayerPrefs.SetInt("SMC_combo", v ? 1 : 0); } }
    private void SetProjectiles(bool v) { if (_game != null) { _game.ProjectilesEnabled = v; PlayerPrefs.SetInt("SMC_projectiles", v ? 1 : 0); } }
    private void SetSlowMo(bool v) { if (_feedback != null) { _feedback.slowMoEnabled = v; PlayerPrefs.SetInt("SMC_slowmo", v ? 1 : 0); } }
    private void SetShake(bool v) { if (_feedback != null) { _feedback.shakeEnabled = v; PlayerPrefs.SetInt("SMC_shake", v ? 1 : 0); } }
    private void SetParticles(bool v) { if (_feedback != null) { _feedback.particlesEnabled = v; PlayerPrefs.SetInt("SMC_particles", v ? 1 : 0); } }
    private void SetSound(bool v) { if (_audio != null) { _audio.audioEnabled = v; PlayerPrefs.SetInt("SMC_audio", v ? 1 : 0); } }
    private void SetVolume(float v) { if (_audio != null) { _audio.volume = v; PlayerPrefs.SetFloat("SMC_volume", v); } }

    private void LoadSettings()
    {
        if (_game != null)
        {
            _game.ParryEnabled = PlayerPrefs.GetInt("SMC_parry", _game.ParryEnabled ? 1 : 0) == 1;
            _game.ComboEnabled = PlayerPrefs.GetInt("SMC_combo", _game.ComboEnabled ? 1 : 0) == 1;
            _game.ProjectilesEnabled = PlayerPrefs.GetInt("SMC_projectiles", _game.ProjectilesEnabled ? 1 : 0) == 1;
            SetToggle(parryToggle, _game.ParryEnabled);
            SetToggle(comboToggle, _game.ComboEnabled);
            SetToggle(projectilesToggle, _game.ProjectilesEnabled);
        }
        if (_feedback != null)
        {
            _feedback.slowMoEnabled = PlayerPrefs.GetInt("SMC_slowmo", _feedback.slowMoEnabled ? 1 : 0) == 1;
            _feedback.shakeEnabled = PlayerPrefs.GetInt("SMC_shake", _feedback.shakeEnabled ? 1 : 0) == 1;
            _feedback.particlesEnabled = PlayerPrefs.GetInt("SMC_particles", _feedback.particlesEnabled ? 1 : 0) == 1;
            SetToggle(slowMoToggle, _feedback.slowMoEnabled);
            SetToggle(shakeToggle, _feedback.shakeEnabled);
            SetToggle(particlesToggle, _feedback.particlesEnabled);
        }
        if (_audio != null)
        {
            _audio.audioEnabled = PlayerPrefs.GetInt("SMC_audio", _audio.audioEnabled ? 1 : 0) == 1;
            _audio.volume = PlayerPrefs.GetFloat("SMC_volume", _audio.volume);
            SetToggle(soundToggle, _audio.audioEnabled);
            if (volumeSlider != null) volumeSlider.SetValueWithoutNotify(_audio.volume);
        }
    }

    private static void SetToggle(Toggle t, bool on)
    {
        if (t != null) t.SetIsOnWithoutNotify(on);
    }

    public void ShowOptions(bool show)
    {
        SetPanel(optionsPanel, show);
        if (_game != null)
        {
            SetPanel(menuPanel, !show && _game.State == GameState.Menu);
            SetPanel(hudPanel, !show && _game.State == GameState.Playing);
        }
    }

    private void OnStateChanged(GameState state)
    {
        SetPanel(menuPanel, state == GameState.Menu);
        SetPanel(hudPanel, state == GameState.Playing);
        SetPanel(gameOverPanel, state == GameState.GameOver);
        SetPanel(optionsPanel, false);

        if (state == GameState.GameOver && _game != null)
        {
            if (finalScoreText != null) finalScoreText.text = "Score: " + _game.Score.ToString("F0");
            if (bestScoreText != null) bestScoreText.text = "Recorde: " + _game.BestScore.ToString("F0");
        }
    }

    private void OnScoreChanged(float score)
    {
        if (_game == null) return;
        if (distanceText != null) distanceText.text = "Distancia: " + _game.Distance.ToString("F0") + " m";
        if (scoreText != null) scoreText.text = "Score: " + score.ToString("F0");
    }

    private void OnComboChanged(int combo)
    {
        if (_game == null) return;
        if (comboText != null) comboText.text = "Combo x" + combo + "  (mult x" + _game.Multiplier.ToString("F1") + ")";
    }

    private static void SetPanel(GameObject panel, bool show)
    {
        if (panel != null) panel.SetActive(show);
    }
}