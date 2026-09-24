using UnityEngine;

[CreateAssetMenu(fileName = "PlayerAnimationSet", menuName = "SMC/PlayerAnimationSet")]
public class PlayerAnimationSet : ScriptableObject
{
    [Header("Acoes do jogador")]
    [Tooltip("Correndo no chao (loop).")]
    public AnimationClip run;
    [Tooltip("Subindo (velocidade vertical positiva).")]
    public AnimationClip jumpUp;
    [Tooltip("Caindo (velocidade vertical negativa).")]
    public AnimationClip jumpDown;
    [Tooltip("Aterrissagem (toca uma vez ao tocar o chao).")]
    public AnimationClip landing;
    [Tooltip("Parry (toca uma vez ao dar parry).")]
    public AnimationClip parry;
}