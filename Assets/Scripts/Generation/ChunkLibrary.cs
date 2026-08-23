using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ChunkLibrary", menuName = "SMC/ChunkLibrary")]
public class ChunkLibrary : ScriptableObject
{
    public List<ChunkData> chunks = new List<ChunkData>
    {
        new ChunkData
        {
            name = "Plano",
            length = 20f,
            weight = 1.5f,
            minDifficulty = 0f,
            ground = new List<GroundSegment> { new GroundSegment { startX = 0f, length = 20f } }
        },
        new ChunkData
        {
            name = "Plano + Espinho",
            length = 20f,
            weight = 1f,
            minDifficulty = 0f,
            ground = new List<GroundSegment> { new GroundSegment { startX = 0f, length = 20f } },
            obstacles = new List<ChunkObstacle> { new ChunkObstacle { localX = 10f, kind = ObstacleKind.Spike } }
        },
        new ChunkData
        {
            name = "Buraco",
            length = 20f,
            weight = 0.8f,
            minDifficulty = 0.1f,
            ground = new List<GroundSegment>
            {
                new GroundSegment { startX = 0f, length = 8f },
                new GroundSegment { startX = 12f, length = 8f }
            }
        },
        new ChunkData
        {
            name = "Espinho Duplo",
            length = 22f,
            weight = 1f,
            minDifficulty = 0.15f,
            ground = new List<GroundSegment> { new GroundSegment { startX = 0f, length = 22f } },
            obstacles = new List<ChunkObstacle>
            {
                new ChunkObstacle { localX = 5f, kind = ObstacleKind.Spike },
                new ChunkObstacle { localX = 12f, kind = ObstacleKind.Spike }
            }
        },
        new ChunkData
        {
            name = "Espinho + Parede",
            length = 24f,
            weight = 0.9f,
            minDifficulty = 0.25f,
            ground = new List<GroundSegment> { new GroundSegment { startX = 0f, length = 24f } },
            obstacles = new List<ChunkObstacle>
            {
                new ChunkObstacle { localX = 6f, kind = ObstacleKind.Spike },
                new ChunkObstacle { localX = 13f, kind = ObstacleKind.Wall }
            }
        },
        new ChunkData
        {
            name = "Inimigo + Buraco",
            length = 26f,
            weight = 0.8f,
            minDifficulty = 0.4f,
            ground = new List<GroundSegment>
            {
                new GroundSegment { startX = 0f, length = 12f },
                new GroundSegment { startX = 16f, length = 10f }
            },
            obstacles = new List<ChunkObstacle> { new ChunkObstacle { localX = 6f, kind = ObstacleKind.GroundEnemy } }
        },
        new ChunkData
        {
            name = "Tres Espinhos",
            length = 22f,
            weight = 0.7f,
            minDifficulty = 0.5f,
            ground = new List<GroundSegment> { new GroundSegment { startX = 0f, length = 22f } },
            obstacles = new List<ChunkObstacle>
            {
                new ChunkObstacle { localX = 4f, kind = ObstacleKind.Spike },
                new ChunkObstacle { localX = 9f, kind = ObstacleKind.Spike },
                new ChunkObstacle { localX = 14f, kind = ObstacleKind.Spike }
            }
        }
    };
}