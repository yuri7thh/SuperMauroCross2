using System.Collections.Generic;
using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [Header("Sprite")]
    [Tooltip("Sprite do padrao que sera repetido (tile).")]
    public Sprite sprite;
    [Tooltip("Cor/tint aplicada ao sprite.")]
    public Color tint = Color.white;
    [Tooltip("Escala uniforme (1 = tamanho natural do sprite, 1:1).")]
    public float scale = 1f;

    [Header("Parallax")]
    [Tooltip("Velocidade de scroll: 0 = parado, 1 = acompanha o mundo (chao). Menor = mais longe.")]
    [Range(0f, 2f)] public float scrollSpeed = 0.3f;
    [Tooltip("Ordem de renderizacao (menor = mais atras).")]
    public int sortingOrder = -20;
    [Tooltip("Posicao vertical da camada.")]
    public float yPosition = 0f;

    private readonly List<SpriteRenderer> _tiles = new List<SpriteRenderer>();
    private Camera _cam;
    private float _tw;

    void Start()
    {
        _cam = Camera.main;
        ComputeTileSize();
        BuildTiles();
    }

    private void ComputeTileSize()
    {
        float natW = sprite != null ? sprite.bounds.size.x : 1f;
        _tw = natW * scale;
        if (_tw <= 0.0001f) _tw = 1f;
    }

    private void BuildTiles()
    {
        foreach (var t in _tiles) if (t != null) Destroy(t.gameObject);
        _tiles.Clear();

        float camWidth = _cam != null ? 2f * _cam.orthographicSize * _cam.aspect : 20f;
        int count = Mathf.CeilToInt((camWidth + _tw * 2f) / _tw) + 1;

        for (int i = 0; i < count; i++)
        {
            var go = new GameObject("Tile" + i);
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = tint;
            sr.sortingOrder = sortingOrder;
            go.transform.localPosition = new Vector3(i * _tw, 0f, 0f);
            go.transform.localScale = new Vector3(scale, scale, 1f);
            _tiles.Add(sr);
        }
    }

    void LateUpdate()
    {
        if (_cam == null || _tiles.Count == 0) return;

        float camX = _cam.transform.position.x;
        float halfW = _cam.orthographicSize * _cam.aspect;
        float offset = (1f - scrollSpeed) * camX;
        float totalW = _tiles.Count * _tw;
        float windowStart = camX - halfW - _tw;

        for (int i = 0; i < _tiles.Count; i++)
        {
            float idealX = i * _tw + offset;
            float x = windowStart + Mathf.Repeat(idealX - windowStart, totalW);
            _tiles[i].transform.position = new Vector3(x, yPosition, 0f);
        }
    }
}