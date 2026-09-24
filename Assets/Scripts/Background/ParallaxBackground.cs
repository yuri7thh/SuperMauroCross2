using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ParallaxLayerConfig
{
    [Tooltip("Nome da camada (so para organizar).")]
    public string name = "Camada";
    [Tooltip("Sprite do padrao que sera repetido (tile).")]
    public Sprite sprite;
    [Tooltip("Cor/tint aplicada ao sprite.")]
    public Color tint = Color.white;
    [Tooltip("Escala uniforme (1 = tamanho natural do sprite, 1:1).")]
    public float scale = 1f;
    [Tooltip("Velocidade de scroll: 0 = parado, 1 = acompanha o chao. Menor = mais longe.")]
    [Range(0f, 2f)] public float scrollSpeed = 0.3f;
    [Tooltip("Ordem de renderizacao (menor = mais atras).")]
    public int sortingOrder = -20;
    [Tooltip("Posicao vertical da camada (altura acima do topo do chao quando followGround esta ligado).")]
    public float yPosition = 0f;
    [Tooltip("Se ligado, a camada acompanha o topo do chao (LevelGenerator.groundY).")]
    public bool followGround = true;
    [Tooltip("Se desligado, esta camada nao aparece.")]
    public bool enabled = true;
}

public class ParallaxBackground : MonoBehaviour
{
    [Tooltip("Lista de camadas. Adicione/remova/ordene aqui e ajuste cada uma no proprio item.")]
    public List<ParallaxLayerConfig> layers = new List<ParallaxLayerConfig>();

    [Tooltip("LevelGenerator que define o topo do chao (groundY). Se vazio, procura na cena.")]
    public LevelGenerator levelGenerator;

    private readonly List<ParallaxLayer> _spawned = new List<ParallaxLayer>();
    private LevelGenerator _levelGen;

    void Start()
    {
        _levelGen = levelGenerator != null ? levelGenerator : FindFirstObjectByType<LevelGenerator>();
        Rebuild();
    }

    void Update()
    {
        float gy = CurrentGroundY();
        foreach (var l in _spawned)
        {
            if (l != null) l.groundY = gy;
        }
    }

    private float CurrentGroundY()
    {
        return _levelGen != null ? _levelGen.groundY : 0f;
    }

    [ContextMenu("Rebuild")]
    public void Rebuild()
    {
        foreach (var l in _spawned) if (l != null) Destroy(l.gameObject);
        _spawned.Clear();

        foreach (var cfg in layers)
        {
            if (cfg == null || !cfg.enabled || cfg.sprite == null) continue;

            var go = new GameObject(string.IsNullOrEmpty(cfg.name) ? "Camada" : cfg.name);
            go.transform.SetParent(transform, false);

            var layer = go.AddComponent<ParallaxLayer>();
            layer.sprite = cfg.sprite;
            layer.tint = cfg.tint;
            layer.scale = cfg.scale;
            layer.scrollSpeed = cfg.scrollSpeed;
            layer.sortingOrder = cfg.sortingOrder;
            layer.yPosition = cfg.yPosition;
            layer.followGround = cfg.followGround;
            layer.groundY = CurrentGroundY();

            _spawned.Add(layer);
        }
    }
}