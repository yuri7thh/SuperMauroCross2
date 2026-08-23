using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "SMC/GameConfig")]
public class GameConfig : ScriptableObject
{
    [Header("Toggles de recursos")]
    [Tooltip("Habilita a mecanica de parry.")]
    public bool parryEnabled = true;
    [Tooltip("Habilita o combo/multiplicador de score.")]
    public bool comboEnabled = true;
    [Tooltip("Habilita o spawn de projeteis.")]
    public bool projectilesEnabled = true;

    [Header("Combo")]
    [Tooltip("Tempo sem parry para resetar o combo.")]
    public float comboResetTime = 3f;
    [Tooltip("Quanto o multiplicador sobe por parry.")]
    public float multiplierPerCombo = 1f;
    [Tooltip("Teto do multiplicador.")]
    public float maxMultiplier = 10f;

    [Header("Score")]
    [Tooltip("Pontos por unidade de distancia.")]
    public float scorePerUnit = 1f;
    [Tooltip("Pontos base por parry (antes do multiplicador).")]
    public float parryScoreBonus = 50f;
}