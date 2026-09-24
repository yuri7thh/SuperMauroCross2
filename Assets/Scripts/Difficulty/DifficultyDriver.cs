using UnityEngine;

public class DifficultyDriver : MonoBehaviour
{
    [Tooltip("Perfil de dificuldade. Se vazio, carrega de Resources/Configs/DifficultyProfile ou usa padroes.")]
    public DifficultyProfile config;

    private DifficultyProfile _loadedConfig;

    public DifficultyProfile Config
    {
        get
        {
            if (_loadedConfig == null)
            {
                if (config != null) _loadedConfig = config;
                else
                {
                    _loadedConfig = Resources.Load<DifficultyProfile>("Configs/DifficultyProfile");
                    if (_loadedConfig == null) _loadedConfig = ScriptableObject.CreateInstance<DifficultyProfile>();
                }
            }
            return _loadedConfig;
        }
    }

    public float Difficulty { get; private set; }

    public float Speed => Config.speedCurve.Evaluate(Difficulty);
    public float ProjectileInterval => Config.projectileIntervalCurve.Evaluate(Difficulty);
    public float ProjectileSpeed => Config.projectileSpeedCurve.Evaluate(Difficulty);
    public float ObstacleChance => Config.obstacleChanceCurve.Evaluate(Difficulty);
    public float PitChance => Config.pitChanceCurve.Evaluate(Difficulty);

    void Update()
    {
        if (GameManager.Instance == null) return;
        float t = Mathf.Clamp01(GameManager.Instance.Distance / Mathf.Max(1f, Config.difficultyRampDistance));
        float raw = Config.difficultyCurve.Evaluate(t);
        Difficulty = Mathf.Clamp01(Config.difficultyStartOffset + (1f - Config.difficultyStartOffset) * raw);
    }
}