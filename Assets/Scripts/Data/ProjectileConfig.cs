using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileConfig", menuName = "SMC/ProjectileConfig")]
public class ProjectileConfig : ScriptableObject
{
    [Header("Projetil")]
    [Tooltip("Velocidade do projetil para a esquerda.")]
    public float speed = 9f;

    [Header("Spawn")]
    [Tooltip("Distancia a frente do jogador para spawnar.")]
    public float spawnDistance = 40f;
    [Tooltip("Altura (Y) de spawn do projetil.")]
    public float spawnHeight = 1f;
    [Tooltip("Variacao vertical aleatoria na altura.")]
    public float heightVariance = 0f;
    [Tooltip("Intervalo base se nao houver DifficultyDriver.")]
    public float baseInterval = 2.5f;
}