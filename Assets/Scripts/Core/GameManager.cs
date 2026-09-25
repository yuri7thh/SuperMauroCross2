using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { Menu, Transition, Playing, GameOver }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Tooltip("Config principal. Se vazio, carrega de Resources/Configs/GameConfig ou usa padroes.")]
    public GameConfig config;

    private GameConfig _loadedConfig;
    private InputReader _input;
    private FeedbackManager _feedback;

    public GameConfig Config
    {
        get
        {
            if (_loadedConfig == null)
            {
                if (config != null) _loadedConfig = config;
                else
                {
                    _loadedConfig = Resources.Load<GameConfig>("Configs/GameConfig");
                    if (_loadedConfig == null) _loadedConfig = ScriptableObject.CreateInstance<GameConfig>();
                }
            }
            return _loadedConfig;
        }
    }

    public bool ParryEnabled { get => Config.parryEnabled; set => Config.parryEnabled = value; }
    public bool ComboEnabled { get => Config.comboEnabled; set => Config.comboEnabled = value; }
    public bool ProjectilesEnabled { get => Config.projectilesEnabled; set => Config.projectilesEnabled = value; }
    public bool ReturnToStart => Config.returnToStart;
    public float ComboResetTime => Config.comboResetTime;
    public float ScorePerUnit => Config.scorePerUnit;
    public float ParryScoreBonus => Config.parryScoreBonus;

    public GameState State { get; private set; } = GameState.Menu;
    public float Distance { get; private set; }
    public int Combo { get; private set; }
    public float Score => Distance * ScorePerUnit + _parryBonusScore;
    public float Multiplier => ComboEnabled ? Combo + 1f : 1f;
    public float ComboTimerNormalized => (ComboResetTime > 0f && Combo > 0) ? Mathf.Clamp01(_comboTimer / ComboResetTime) : 0f;
    public float BestScore => PlayerPrefs.GetFloat("BestScore", 0f);

    public event Action<GameState> OnStateChanged;
    public event Action<float> OnScoreChanged;
    public event Action<int> OnComboChanged;
    public event Action<float> OnParryPoints;

    private float _parryBonusScore;
    private float _comboTimer;
    private float _distanceOffset;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        _input = FindFirstObjectByType<InputReader>();
        _feedback = FindFirstObjectByType<FeedbackManager>();
    }

    void Update()
    {
        if (State == GameState.Menu && _input != null && _input.JumpPressed)
        {
            StartGame();
        }

        if (State == GameState.Playing && ComboEnabled && Combo > 0)
        {
            _comboTimer -= Time.deltaTime;
            if (_comboTimer <= 0f) ResetCombo();
        }

        if (State == GameState.GameOver && _input != null && _input.RestartPressed)
        {
            Restart();
        }
    }

    public void SetDistance(float distance)
    {
        Distance = distance - _distanceOffset;
        OnScoreChanged?.Invoke(Score);
    }

    public void StartGame()
    {
        _distanceOffset = 0f;
        Distance = 0f;
        _parryBonusScore = 0f;
        ResetCombo();

        if (ReturnToStart)
        {
            SetState(GameState.Transition);
        }
        else
        {
            FinishTransition();
        }

        OnScoreChanged?.Invoke(Score);
    }

    public void FinishTransition()
    {
        var pc = FindFirstObjectByType<PlayerController>();
        _distanceOffset = pc != null ? pc.transform.position.x : 0f;

        var gen = FindFirstObjectByType<LevelGenerator>();
        if (gen != null) gen.ResetWorld();

        SetState(GameState.Playing);
    }

    public float RegisterParry()
    {
        if (ComboEnabled)
        {
            Combo++;
            _comboTimer = ComboResetTime;
            OnComboChanged?.Invoke(Combo);
        }
        float gained = ParryScoreBonus * Multiplier;
        _parryBonusScore += gained;
        OnScoreChanged?.Invoke(Score);
        OnParryPoints?.Invoke(gained);
        return gained;
    }

    public void GameOver()
    {
        if (State != GameState.Playing) return;
        SetState(GameState.GameOver);
        float best = BestScore;
        if (Score > best) PlayerPrefs.SetFloat("BestScore", Score);
        Time.timeScale = 1f;
        AudioManager.Instance?.PlayDeath();
        if (_feedback != null) _feedback.TriggerDeathFeedback();
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ResetCombo()
    {
        if (Combo == 0) return;
        Combo = 0;
        OnComboChanged?.Invoke(0);
    }

    private void SetState(GameState state)
    {
        State = state;
        OnStateChanged?.Invoke(state);
    }
}