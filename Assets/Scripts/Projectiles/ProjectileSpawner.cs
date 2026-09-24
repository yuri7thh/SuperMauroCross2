using UnityEngine;

public class ProjectileSpawner : MonoBehaviour
{
    [Header("Config")]
    [Tooltip("Modelo do projetil (prefab/template).")]
    public GameObject projectileTemplate;
    [Tooltip("Driver de dificuldade (nao usado atualmente; reservado).")]
    public DifficultyDriver difficulty;

    [Tooltip("Config do projetil. Se vazio, carrega de Resources/Configs/ProjectileConfig ou usa padroes.")]
    public ProjectileConfig config;

    private ProjectileConfig _loadedConfig;
    private Transform _player;
    private float _timer;
    private float _lastSpawnY;
    private bool _hasLastSpawnY;

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
        _timer = Random.Range(Config.spawnIntervalMin, Config.spawnIntervalMax);
    }

    void Update()
    {
        var gm = GameManager.Instance;
        if (gm == null || gm.State != GameState.Playing) return;
        if (!gm.ProjectilesEnabled) return;
        if (projectileTemplate == null || _player == null) return;

        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            Spawn();
            _timer = Random.Range(Config.spawnIntervalMin, Config.spawnIntervalMax);
        }
    }

    private void Spawn()
    {
        Vector3 pos = new Vector3(
            _player.position.x + Config.spawnDistance,
            PickSpawnY(),
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
            proj.speed = Random.Range(Config.speedMin, Config.speedMax);
    }

    private float PickSpawnY()
    {
        float min = Config.spawnHeight - Config.heightVariance;
        float max = Config.spawnHeight + Config.heightVariance;

        float y = Random.Range(min, max);

        if (_hasLastSpawnY)
        {
            float sep = Mathf.Max(0f, Config.minYSeparation);
            if (Mathf.Abs(y - _lastSpawnY) < sep)
            {
                bool canUp = _lastSpawnY + sep <= max;
                bool canDown = _lastSpawnY - sep >= min;

                if (canUp && canDown)
                    y = (Random.value >= 0.5f) ? _lastSpawnY + sep : _lastSpawnY - sep;
                else if (canUp)
                    y = _lastSpawnY + sep;
                else if (canDown)
                    y = _lastSpawnY - sep;
            }
        }

        _lastSpawnY = y;
        _hasLastSpawnY = true;
        return y;
    }
}
