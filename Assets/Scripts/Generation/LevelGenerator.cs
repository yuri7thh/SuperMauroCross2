using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    public DifficultyDriver difficulty;
    [Tooltip("Biblioteca de chunks. Se vazio, carrega de Resources/Configs/ChunkLibrary.")]
    public ChunkLibrary chunkLibrary;
    [Tooltip("Config visual dos obstaculos (sprite/hitbox/cor). Se vazio, carrega de Resources/Configs/ObstacleConfig.")]
    public ObstacleConfig obstacleConfig;

    [Header("Chao")]
    public Color groundColor = new Color(0.3f, 0.3f, 0.35f);
    [Tooltip("Sprite do chao (opcional). Se definido, e desenhado em modo 'tiled'. Se vazio, usa uma cor solida.")]
    public Sprite groundSprite;
    [Tooltip("Altura (Y) do topo do chao.")]
    public float groundY = 0f;
    [Tooltip("Espessura do chao.")]
    public float groundHeight = 3f;
    [Tooltip("Comprimento de chao usado se nenhum chunk estiver disponivel.")]
    public float fallbackLength = 12f;

    [Header("Grama (decoracao no topo do chao)")]
    [Tooltip("Sprites da grama. 1 sprite = faixa continua (tiled) em qualquer tamanho; varios sprites = tufos sorteados ao longo do topo. Se vazio, nao gera grama.")]
    public List<Sprite> grassSprites = new List<Sprite>();
    [Tooltip("Altura da faixa de grama quando ha apenas um sprite.")]
    public float grassHeight = 0.5f;
    [Tooltip("Cor/tint da grama.")]
    public Color grassColor = Color.white;
    [Tooltip("Ordem de renderizacao da grama (acima do chao, abaixo dos obstaculos).")]
    public int grassSortingOrder = -9;

    [Header("Obstaculos")]
    [Tooltip("Mapa de tipo de obstaculo -> template.")]
    public List<TemplateEntry> templates = new List<TemplateEntry>();

    [Header("Distancias de spawn/despawn")]
    public float spawnAhead = 60f;
    public float despawnBehind = 30f;

    private float _nextX;
    private readonly List<GameObject> _spawned = new List<GameObject>();
    private Dictionary<ObstacleKind, GameObject> _templateMap;
    private RuntimeAnimatorController _obstacleBaseController;

    [System.Serializable]
    public class TemplateEntry
    {
        public ObstacleKind kind;
        public GameObject template;
    }

    void Start()
    {
        if (player == null)
        {
            var pc = FindFirstObjectByType<PlayerController>();
            if (pc != null) player = pc.transform;
        }

        if (chunkLibrary == null)
        {
            chunkLibrary = Resources.Load<ChunkLibrary>("Configs/ChunkLibrary");
        }

        if (obstacleConfig == null)
        {
            obstacleConfig = Resources.Load<ObstacleConfig>("Configs/ObstacleConfig");
        }

        BuildTemplateMap();

        SpawnSafeStart();

        EnsureChunksUpTo(player != null ? player.position.x + spawnAhead : spawnAhead);
    }

    private void SpawnSafeStart()
    {
        if (player != null)
        {
            float startX = player.position.x - 30f;
            float safeEnd = player.position.x + 25f;
            SpawnGroundSegment(startX, safeEnd - startX);
            _nextX = safeEnd;
        }
        else
        {
            _nextX = 0f;
        }
    }

    public void ResetWorld()
    {
        foreach (var go in _spawned) if (go != null) DestroyImmediate(go);
        _spawned.Clear();

        SpawnSafeStart();
        EnsureChunksUpTo(player != null ? player.position.x + spawnAhead : spawnAhead);
    }

    void Update()
    {
        if (player == null) return;
        var gm = GameManager.Instance;
        if (gm != null && gm.State == GameState.Transition) return;
        EnsureChunksUpTo(player.position.x + spawnAhead);
        DespawnBehind(player.position.x - despawnBehind);
    }

    private void EnsureChunksUpTo(float targetX)
    {
        while (_nextX < targetX)
        {
            var gm = GameManager.Instance;
            if (gm != null && gm.State == GameState.Menu)
            {
                SpawnGroundSegment(_nextX, fallbackLength);
                _nextX += fallbackLength;
                continue;
            }

            var chunk = PickChunk();
            if (chunk == null)
            {
                SpawnGroundSegment(_nextX, fallbackLength);
                _nextX += fallbackLength;
                continue;
            }

            SpawnChunk(chunk, _nextX);
            _nextX += chunk.length;
        }
    }

    private ChunkData PickChunk()
    {
        if (chunkLibrary == null || chunkLibrary.chunks == null) return null;

        float diff = difficulty != null ? difficulty.Difficulty : 0f;
        float total = 0f;
        foreach (var c in chunkLibrary.chunks)
        {
            if (c != null && c.enabled && diff >= c.minDifficulty) total += Mathf.Max(0f, c.weight);
        }
        if (total <= 0f) return null;

        float roll = Random.value * total;
        foreach (var c in chunkLibrary.chunks)
        {
            if (c == null || !c.enabled || diff < c.minDifficulty) continue;
            roll -= Mathf.Max(0f, c.weight);
            if (roll <= 0f) return c;
        }
        return null;
    }

    private void SpawnChunk(ChunkData chunk, float originX)
    {
        if (chunk == null) return;

        if (chunk.ground != null)
        {
            foreach (var g in chunk.ground)
                SpawnGroundSegment(originX + g.startX, g.length);
        }

        if (chunk.obstacles != null)
        {
            foreach (var o in chunk.obstacles)
            {
                if (IsOnGround(chunk, o.localX))
                    SpawnTemplate(o.kind, originX + o.localX);
            }
        }
    }

    private bool IsOnGround(ChunkData chunk, float x)
    {
        if (chunk.ground == null) return false;
        foreach (var g in chunk.ground)
        {
            if (x >= g.startX && x <= g.startX + g.length) return true;
        }
        return false;
    }

    private void SpawnGroundSegment(float startX, float length)
    {
        float centerX = startX + length * 0.5f;
        float centerY = groundY - groundHeight * 0.5f;

        var go = new GameObject("Ground");
        go.transform.position = new Vector3(centerX, centerY, 0f);

        var sr = go.AddComponent<SpriteRenderer>();
        var col = go.AddComponent<BoxCollider2D>();

        if (groundSprite != null)
        {
            sr.sprite = groundSprite;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = new Vector2(length, groundHeight);
            sr.color = groundColor;
            col.size = new Vector2(length, groundHeight);
        }
        else
        {
            sr.sprite = PlaceholderFactory.WhiteSprite;
            sr.color = groundColor;
            go.transform.localScale = new Vector3(length, groundHeight, 1f);
            col.size = Vector2.one;
        }

        sr.sortingOrder = -10;
        _spawned.Add(go);

        if (grassSprites != null && grassSprites.Count > 0) SpawnGrass(startX, length);
    }

    private void SpawnGrass(float startX, float length)
    {
        if (grassSprites.Count == 1)
        {
            SpawnGrassStrip(startX, length, grassSprites[0]);
        }
        else
        {
            SpawnGrassTufts(startX, length);
        }
    }

    private void SpawnGrassStrip(float startX, float length, Sprite sprite)
    {
        float centerX = startX + length * 0.5f;
        var go = new GameObject("Grass");
        go.transform.position = new Vector3(centerX, groundY + grassHeight * 0.5f, 0f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = new Vector2(length, grassHeight);
        sr.color = grassColor;
        sr.sortingOrder = grassSortingOrder;

        _spawned.Add(go);
    }

    private void SpawnGrassTufts(float startX, float length)
    {
        float endX = startX + length;
        float x = startX;
        int guard = 0;
        while (x < endX - 0.01f && guard < 200)
        {
            var sprite = grassSprites[Random.Range(0, grassSprites.Count)];
            float w = sprite.bounds.size.x;
            if (w <= 0.0001f) w = 0.5f;
            float h = sprite.bounds.size.y;
            if (h <= 0.0001f) h = grassHeight;

            SpawnGrassTuft(sprite, x + w * 0.5f, groundY + h * 0.5f);

            x += w;
            guard++;
        }
    }

    private void SpawnGrassTuft(Sprite sprite, float centerX, float centerY)
    {
        var go = new GameObject("Grass");
        go.transform.position = new Vector3(centerX, centerY, 0f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = grassColor;
        sr.sortingOrder = grassSortingOrder;

        _spawned.Add(go);
    }

    private void SpawnTemplate(ObstacleKind kind, float x)
    {
        GameObject template = GetTemplate(kind);
        if (template == null) return;

        var go = Instantiate(template, new Vector3(x, groundY, 0f), Quaternion.identity);
        go.SetActive(true);
        ConfigureObstacle(go, kind);
        SnapToGround(go);
        _spawned.Add(go);
    }

    private void ConfigureObstacle(GameObject go, ObstacleKind kind)
    {
        if (obstacleConfig == null) return;
        var entry = obstacleConfig.Get(kind);
        if (entry == null) return;

        var rootSr = go.GetComponent<SpriteRenderer>();
        Sprite prefabSprite = rootSr != null ? rootSr.sprite : null;
        int sortOrder = rootSr != null ? rootSr.sortingOrder : 0;
        int sortLayer = rootSr != null ? rootSr.sortingLayerID : 0;
        if (rootSr != null)
        {
            rootSr.enabled = false;
            Destroy(rootSr);
        }

        Transform visual = go.transform.Find("Visual");
        if (visual == null)
        {
            var vgo = new GameObject("Visual");
            visual = vgo.transform;
            visual.SetParent(go.transform, false);
            visual.localPosition = Vector3.zero;
        }

        var sr = visual.GetComponent<SpriteRenderer>();
        if (sr == null) sr = visual.gameObject.AddComponent<SpriteRenderer>();
        sr.sortingOrder = sortOrder;
        sr.sortingLayerID = sortLayer;
        sr.color = entry.color;

        var anim = visual.GetComponent<Animator>();
        RuntimeAnimatorController runtime = ResolveObstacleController(entry);
        if (runtime != null)
        {
            if (anim == null) anim = visual.gameObject.AddComponent<Animator>();
            anim.runtimeAnimatorController = runtime;
            sr.enabled = true;
            sr.sprite = entry.sprite != null ? entry.sprite : prefabSprite;
        }
        else
        {
            if (anim != null) Destroy(anim);
            sr.enabled = true;
            sr.sprite = entry.sprite != null ? entry.sprite : prefabSprite;
        }

        visual.localScale = new Vector3(entry.spriteSize.x, entry.spriteSize.y, 1f);

        ConfigureCollider(go, entry);

        var gizmo = go.GetComponent<ObstacleHitboxGizmo>();
        if (gizmo == null) gizmo = go.AddComponent<ObstacleHitboxGizmo>();
        gizmo.SetShow(obstacleConfig.showHitboxes && entry.showHitbox);
    }

    private RuntimeAnimatorController ResolveObstacleController(ObstacleConfig.ObstacleEntry entry)
    {
        if (entry.animatorController != null) return entry.animatorController;

        if (entry.idle != null)
        {
            if (_obstacleBaseController == null)
                _obstacleBaseController = Resources.Load<RuntimeAnimatorController>("Configs/ObstacleStateController");

            if (_obstacleBaseController != null)
            {
                var overrides = new AnimatorOverrideController(_obstacleBaseController);
                overrides["Idle"] = entry.idle;
                return overrides;
            }
        }

        return null;
    }

    private void ConfigureCollider(GameObject go, ObstacleConfig.ObstacleEntry entry)
    {
        Collider2D existing = go.GetComponent<Collider2D>();

        if (entry.hitboxShape == HitboxShape.Box)
        {
            var box = go.GetComponent<BoxCollider2D>();
            if (box == null)
            {
                if (existing != null) { existing.enabled = false; Destroy(existing); }
                box = go.AddComponent<BoxCollider2D>();
            }
            box.isTrigger = true;
            box.size = entry.hitboxSize;
            box.offset = entry.hitboxOffset;
        }
        else
        {
            var poly = go.GetComponent<PolygonCollider2D>();
            if (poly == null)
            {
                if (existing != null) { existing.enabled = false; Destroy(existing); }
                poly = go.AddComponent<PolygonCollider2D>();
            }
            poly.isTrigger = true;
            poly.offset = entry.hitboxOffset;
            float hw = entry.hitboxSize.x * 0.5f;
            float hh = entry.hitboxSize.y * 0.5f;
            poly.points = new Vector2[]
            {
                new Vector2(-hw, -hh),
                new Vector2(hw, -hh),
                new Vector2(0f, hh)
            };
        }

        go.transform.localScale = Vector3.one;
    }

    private void BuildTemplateMap()
    {
        _templateMap = new Dictionary<ObstacleKind, GameObject>();
        foreach (var t in templates)
        {
            if (t != null && t.template != null) _templateMap[t.kind] = t.template;
        }
    }

    private GameObject GetTemplate(ObstacleKind kind)
    {
        if (_templateMap == null) BuildTemplateMap();
        return _templateMap.TryGetValue(kind, out var t) ? t : null;
    }

    private void SnapToGround(GameObject go)
    {
        var col = go.GetComponent<Collider2D>();
        if (col == null) return;
        float bottom = col.bounds.min.y;
        go.transform.position += new Vector3(0f, groundY - bottom, 0f);
    }

    private void DespawnBehind(float x)
    {
        for (int i = _spawned.Count - 1; i >= 0; i--)
        {
            var go = _spawned[i];
            if (go == null) { _spawned.RemoveAt(i); continue; }
            if (go.transform.position.x < x)
            {
                _spawned.RemoveAt(i);
                Destroy(go);
            }
        }
    }
}