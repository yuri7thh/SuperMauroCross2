using System.Collections.Generic;
using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [Header("Sprite")]
    [Tooltip("Sprite do padrao que sera repetido (tile).")]
    public Sprite sprite;
    [Tooltip("Cor/tint aplicada ao sprite.")]
    public Color tint = Color.white;
    [Tooltip("Largura de um tile em unidades do mundo.")]
    public float tileWidth = 4f;
    [Tooltip("Altura de um tile em unidades do mundo.")]
    public float tileHeight = 4f;

    [Header("Parallax")]
    [Tooltip("Velocidade de scroll: 0 = parado, 1 = acompanha o mundo (chao). Menor = mais longe.")]
    [Range(0f, 2f)] public float scrollSpeed = 0.3f;
    [Tooltip("Ordem de renderizacao (menor = mais atras).")]
    public int sortingOrder = -20;
    [Tooltip("Posicao vertical da camada.")]
    public float yPosition = 0f;

    private readonly List<SpriteRenderer> _tiles = new List<SpriteRenderer>();
    private Camera _cam;

    void Start()
    {
        _cam = Camera.main;
        BuildTiles();
    }

    private void BuildTiles()
    {
        foreach (var t in _tiles) if (t != null) Destroy(t.gameObject);
        _tiles.Clear();

        float camWidth = _cam != null ? 2f * _cam.orthographicSize * _cam.aspect : 20f;
        int count = Mathf.CeilToInt((camWidth + tileWidth * 2f) / tileWidth) + 1;

        for (int i = 0; i < count; i++)
        {
            var go = new GameObject("Tile" + i);
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = tint;
            sr.sortingOrder = sortingOrder;
            go.transform.localPosition = new Vector3(i * tileWidth, 0f, 0f);
            go.transform.localScale = new Vector3(tileWidth, tileHeight, 1f);
            _tiles.Add(sr);
        }
    }

    void LateUpdate()
    {
        if (_cam == null || _tiles.Count == 0) return;

        float scroll = _cam.transform.position.x * (1f - scrollSpeed);
        float offset = -Mathf.Repeat(scroll, tileWidth);
        transform.position = new Vector3(offset, yPosition, 0f);
    }
}