using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileConfig", menuName = "SMC/ProjectileConfig")]
public class ProjectileConfig : ScriptableObject
{
    [Header("Projetil")]
    [Tooltip("Velocidade minima do projetil para a esquerda.")]
    public float speedMin = 8f;
    [Tooltip("Velocidade maxima do projetil para a esquerda.")]
    public float speedMax = 14f;

    [Header("Spawn")]
    [Tooltip("Distancia a frente do jogador para spawnar.")]
    public float spawnDistance = 40f;
    [Tooltip("Altura (Y) de spawn do projetil.")]
    public float spawnHeight = 1f;
    [Tooltip("Variacao vertical aleatoria na altura.")]
    public float heightVariance = 0f;
    [Tooltip("Distancia vertical minima entre um projetil e o anterior no spawn.")]
    public float minYSeparation = 1f;
    [Tooltip("Intervalo minimo (s) entre um projetil e o proximo.")]
    public float spawnIntervalMin = 0.5f;
    [Tooltip("Intervalo maximo (s) entre um projetil e o proximo.")]
    public float spawnIntervalMax = 2.5f;
}
