using UnityEngine;

[CreateAssetMenu(fileName = "DifficultyProfile", menuName = "SMC/DifficultyProfile")]
public class DifficultyProfile : ScriptableObject
{
    [Header("Curva de dificuldade")]
    [Tooltip("Dificuldade (0..1) em funcao da distancia percorrida.")]
    public AnimationCurve difficultyCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    [Tooltip("Distancia em que a dificuldade chega ao maximo.")]
    public float difficultyRampDistance = 1500f;

    [Header("Velocidade")]
    [Tooltip("Velocidade do jogador em funcao da dificuldade.")]
    public AnimationCurve speedCurve = AnimationCurve.Linear(0f, 8f, 1f, 16f);

    [Header("Projeteis")]
    [Tooltip("Intervalo (segundos) entre projeteis em funcao da dificuldade.")]
    public AnimationCurve projectileIntervalCurve = AnimationCurve.Linear(0f, 3f, 1f, 1.2f);
    [Tooltip("Velocidade do projetil em funcao da dificuldade.")]
    public AnimationCurve projectileSpeedCurve = AnimationCurve.Linear(0f, 9f, 1f, 14f);

    [Header("Obstaculos")]
    [Tooltip("Chance (0..1) de spawnar um obstaculo por segmento, em funcao da dificuldade.")]
    public AnimationCurve obstacleChanceCurve = AnimationCurve.Linear(0f, 0.3f, 1f, 0.85f);

    [Header("Buracos")]
    [Tooltip("Chance (0..1) de um segmento virar um buraco, em funcao da dificuldade.")]
    public AnimationCurve pitChanceCurve = AnimationCurve.Linear(0f, 0f, 1f, 0.3f);
}