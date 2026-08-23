using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    public DifficultyDriver difficulty;
    [Tooltip("Biblioteca de chunks. Se vazio, carrega de Resources/Configs/ChunkLibrary.")]
    public ChunkLibrary chunkLibrary;

    [Header("Chao")]
    public Color groundColor = new Color(0.3f, 0.3f, 0.35f);
    [Tooltip("Altura (Y) do topo do chao.")]
    public float groundY = 0f;
    [Tooltip("Espessura do chao.")]
    public float groundHeight = 3f;
    [Tooltip("Comprimento de chao usado se nenhum chunk estiver disponivel.")]
    public float fallbackLength = 12f;

    [Header("Obstaculos")]
    [Tooltip("Mapa de tipo de obstaculo -> template.")]
    public List<TemplateEntry> templates = new List<TemplateEntry>();

    [Header("Distancias de spawn/despawn")]
    public float spawnAhead = 60f;
    public float despawnBehind = 20f;

    private float _nextX;
    private readonly List<GameObject> _spawned = new List<GameObject>();
    private Dictionary<ObstacleKind, GameObject> _templateMap;

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

        BuildTemplateMap();

        if (player != null)
        {
            float startX = player.position.x - 5f;
            float safeEnd = player.position.x + 25f;
            SpawnGroundSegment(startX, safeEnd - startX);
            _nextX = safeEnd;
        }
        else
        {
            _nextX = 0f;
        }

        EnsureChunksUpTo(player != null ? player.position.x + spawnAhead : spawnAhead);
    }

    void Update()
    {
        if (player == null) return;
        EnsureChunksUpTo(player.position.x + spawnAhead);
        DespawnBehind(player.position.x - despawnBehind);
    }

    private void EnsureChunksUpTo(float targetX)
    {
        while (_nextX < targetX)
        {
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
        var go = PlaceholderFactory.CreateBox("Ground", groundColor, new Vector2(length, groundHeight), false);
        go.transform.position = new Vector3(centerX, centerY, 0f);
        _spawned.Add(go);
    }

    private void SpawnTemplate(ObstacleKind kind, float x)
    {
        GameObject template = GetTemplate(kind);
        if (template == null) return;

        var go = Instantiate(template, new Vector3(x, groundY, 0f), Quaternion.identity);
        go.SetActive(true);
        SnapToGround(go);
        _spawned.Add(go);
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