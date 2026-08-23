using UnityEngine;

[CreateAssetMenu(fileName = "PlayerConfig", menuName = "SMC/PlayerConfig")]
public class PlayerConfig : ScriptableObject
{
    [Header("Movimento")]
    [Tooltip("Velocidade usada se nao houver DifficultyDriver.")]
    public float baseSpeed = 8f;

    [Header("Pulo")]
    public float jumpVelocity = 13f;
    [Range(0.1f, 10f)] public float gravityScale = 3.5f;
    [Tooltip("Distancia extra do raycast de chao.")]
    public float groundCheckDistance = 0.15f;
    [Tooltip("Tempo extra (s) apos sair do chao em que ainda pode pular (coyote time).")]
    public float coyoteTime = 0.1f;
    [Tooltip("Tempo (s) que um pulo apertado antes de aterrissar fica guardado (jump buffer).")]
    public float jumpBufferTime = 0.12f;

    [Header("Parry")]
    [Tooltip("Raio generoso da bolha de parry ao redor do jogador.")]
    public float parryRange = 3f;
    [Tooltip("Mostra o indicador visual do alcance do parry (aparece no ar).")]
    public bool showParryIndicator = true;

    [Header("Dash (no parry)")]
    [Tooltip("Velocidade horizontal durante o dash apos um parry.")]
    public float dashSpeed = 24f;
    [Tooltip("Duracao do dash em segundos.")]
    public float dashDuration = 0.18f;

    [Header("Morte por queda")]
    [Tooltip("Se o jogador cair abaixo deste Y, morre (buraco).")]
    public float killY = -8f;
}