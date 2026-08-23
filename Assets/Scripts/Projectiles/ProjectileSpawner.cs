using UnityEngine;

public class ProjectileSpawner : MonoBehaviour
{
    [Header("Config")]
    [Tooltip("Modelo do projetil (prefab/template).")]
    public GameObject projectileTemplate;
    [Tooltip("Driver de dificuldade (controla o intervalo).")]
    public DifficultyDriver difficulty;

    [Tooltip("Config do projetil. Se vazio, carrega de Resources/Configs/ProjectileConfig ou usa padroes.")]
    public ProjectileConfig config;

    private ProjectileConfig _loadedConfig;
    private Transform _player;
    private float _timer;

    public ProjectileConfig Config
    {
        get
        {
            if (_loadedConfig == null)
            {
                if (config != null) _loadedConfig = config;
                else
                {
                    _loadedConfig = Resources.Load<ProjectileConfig>("Configs/ProjectileConfig");
                    if (_loadedConfig == null) _loadedConfig = ScriptableObject.CreateInstance<ProjectileConfig>();
                }
            }
            return _loadedConfig;
        }
    }

    void Start()
    {
        var pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) _player = pc.transform;
        _timer = Config.baseInterval;
    }

    void Update()
    {
        var gm = GameManager.Instance;
        if (gm == null || gm.State != GameState.Playing) return;
        if (!gm.ProjectilesEnabled) return;
        if (projectileTemplate == null || _player == null) return;

        float interval = difficulty != null ? difficulty.ProjectileInterval : Config.baseInterval;

        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            Spawn();
            _timer = Mathf.Max(0.2f, interval);
        }
    }

    private void Spawn()
    {
        Vector3 pos = new Vector3(
            _player.position.x + Config.spawnDistance,
            Config.spawnHeight + Random.Range(-Config.heightVariance, Config.heightVariance),
            0f);

        GameObject go;
        if (ObjectPool.Instance != null)
            go = ObjectPool.Instance.Spawn(projectileTemplate, pos, Quaternion.identity);
        else
        {
            go = Instantiate(projectileTemplate, pos, Quaternion.identity);
            go.SetActive(true);
        }

        var proj = go.GetComponent<Projectile>();
        if (proj != null)
            proj.speed = difficulty != null ? difficulty.ProjectileSpeed : Config.speed;
    }
}