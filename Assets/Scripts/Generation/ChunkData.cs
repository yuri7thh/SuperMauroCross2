using System;
using System.Collections.Generic;
using UnityEngine;

public enum ObstacleKind { Spike, Wall, GroundEnemy }

[Serializable]
public struct GroundSegment
{
    [Tooltip("Inicio do chao (relativo ao inicio do chunk).")]
    public float startX;
    [Tooltip("Comprimento do chao.")]
    public float length;
}

[Serializable]
public struct ChunkObstacle
{
    [Tooltip("Posicao horizontal relativa ao inicio do chunk.")]
    public float localX;
    public ObstacleKind kind;
}

[Serializable]
public class ChunkData
{
    [Tooltip("Nome do chunk.")]
    public string name = "Chunk";
    [Tooltip("Comprimento horizontal total do chunk.")]
    public float length = 20f;
    [Tooltip("Peso no sorteio (maior = mais comum).")]
    public float weight = 1f;
    [Tooltip("Dificuldade minima (0..1) para aparecer.")]
    [Range(0f, 1f)] public float minDifficulty = 0f;
    [Tooltip("Se desligado, nunca aparece.")]
    public bool enabled = true;
    [Tooltip("Segmentos de chao (buracos = espacos vazios entre eles).")]
    public List<GroundSegment> ground = new List<GroundSegment>();
    [Tooltip("Obstaculos do chunk.")]
    public List<ChunkObstacle> obstacles = new List<ChunkObstacle>();
}