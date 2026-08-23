using System;
using UnityEngine;

[Serializable]
public class ObstacleEntry
{
    [Tooltip("Modelo/prefab do obstaculo.")]
    public GameObject template;
    [Tooltip("Peso no sorteio (maior = mais comum).")]
    public float weight = 1f;
    [Tooltip("Dificuldade minima (0..1) para este obstaculo aparecer.")]
    [Range(0f, 1f)] public float minDifficulty = 0f;
    [Tooltip("Se desligado, este obstaculo nunca aparece.")]
    public bool enabled = true;
}
