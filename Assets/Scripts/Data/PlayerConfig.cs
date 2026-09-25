using UnityEngine;

[CreateAssetMenu(fileName = "PlayerConfig", menuName = "SMC/PlayerConfig")]
public class PlayerConfig : ScriptableObject
{
    public enum ParryIndicatorMode { Circle, LineToTarget }

    [Header("Movimento")]
    [Tooltip("Velocidade usada se nao houver DifficultyDriver.")]
    public float baseSpeed = 8f;
    [Tooltip("Se ligado, o jogador fica mais lento no ar (pulando).")]
    public bool slowInAir = true;
    [Tooltip("Multiplicador de velocidade no ar (1 = mesma velocidade, 0 = parado).")]
    [Range(0f, 1f)] public float airSpeedMultiplier = 0.75f;
    [Tooltip("Habilita o controle WASD: A/D move para tras/frente, S faz cair mais rapido (W tambem pula).")]
    public bool useWasdMovement = true;
    [Tooltip("Velocidade de marcha a frente (D) em relacao a velocidade base (para alcancar a rolagem da camera).")]
    public float forwardSpeedMultiplier = 1.2f;
    [Tooltip("Velocidade de marcha a re (A) em relacao a velocidade para frente.")]
    [Range(0f, 1f)] public float backwardSpeedMultiplier = 0.7f;
    [Tooltip("Multiplicador de gravidade quando S e segurado no ar (cair mais rapido).")]
    public float fastFallGravityMultiplier = 2f;

    [Header("Pulo")]
    public float jumpVelocity = 13f;
    [Range(0.1f, 10f)] public float gravityScale = 3.5f;
    [Tooltip("Distancia extra do raycast de chao.")]
    public float groundCheckDistance = 0.15f;
    [Tooltip("Largura do sensor de chao relativa ao collider (1 = mesma largura).")]
    [Range(0.1f, 1f)] public float groundCheckWidth = 0.85f;
    [Tooltip("Tempo extra (s) apos sair do chao em que ainda pode pular (coyote time).")]
    public float coyoteTime = 0.1f;
    [Tooltip("Tempo (s) que um pulo apertado antes de aterrissar fica guardado (jump buffer).")]
    public float jumpBufferTime = 0.12f;

    [Header("Pulo variavel (jump cut)")]
    [Tooltip("Habilita o corte de subida ao soltar o botao de pulo cedo (pulo de altura variavel).")]
    public bool jumpCutEnabled = true;
    [Tooltip("Fracao da velocidade de subida mantida ao soltar cedo (0 = corta toda a subida, 1 = sem corte).")]
    [Range(0f, 1f)] public float jumpCutMultiplier = 0.5f;

    [Header("Parry")]
    [Tooltip("Raio generoso da bolha de parry ao redor do jogador.")]
    public float parryRange = 3f;
    [Tooltip("Mostra o indicador visual do alcance do parry (aparece no ar).")]
    public bool showParryIndicator = true;
    [Tooltip("Modo do indicador: Circle = area fixa quando no ar; LineToTarget = linha tracejada so quando ha algo parryable por perto, apontando para o alvo mais proximo a frente.")]
    public ParryIndicatorMode parryIndicatorMode = ParryIndicatorMode.Circle;
    [Tooltip("Cor da linha tracejada do indicador (o alfa e controlado pela curva de opacidade).")]
    public Color parryLineColor = new Color(0.2f, 1f, 0.35f, 1f);
    [Tooltip("Curva de opacidade da linha pela distancia: 0 = borda do range externo (sumida), 1 = dentro do range de parry (visivel).")]
    public AnimationCurve parryLineOpacityCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [Tooltip("Se ligado, durante o dash apos um parry o jogador faz parry automatico em outras coisas parryaveis no caminho (cadeia).")]
    public bool chainParryOnDash = true;

    [Header("Dash (no parry)")]
    [Tooltip("Distancia minima percorrida pelo dash (percorre pelo menos ate o alvo e pelo menos esta distancia).")]
    public float dashDistance = 2f;
    [Tooltip("Duracao fixa do dash em segundos.")]
    public float dashDuration = 0.18f;
    [Tooltip("Se ligado, o sprite rotaciona para a direcao do dash e volta ao normal apos.")]
    public bool rotateSpriteWithDash = true;
    [Tooltip("Velocidade (graus/s) para voltar a rotacao normal apos o dash.")]
    public float dashRotateResetSpeed = 720f;

    [Header("Parry pulo (botao L)")]
    [Tooltip("Velocidade vertical para cima ao usar o parry-pulo.")]
    public float parryJumpVelocity = 20f;
    [Tooltip("Verticalidade do pulo (0 = mantem o movimento horizontal, 1 = 100% vertical, > 1 = empurra para tras).")]
    [Range(0f, 3f)] public float parryJumpVerticality = 0.85f;
    [Tooltip("Desaceleracao horizontal breve apos o pulo (1 = sem desacelerar, 0 = para no ar).")]
    [Range(0f, 1f)] public float parryJumpDeceleration = 0.75f;
    [Tooltip("Duracao (s) da desaceleracao breve.")]
    public float parryJumpDecelerationTime = 0.2f;

    [Header("Limites de camera")]
    [Tooltip("Se ligado, o jogador nunca passa do topo da camera.")]
    public bool clampToCameraTop = true;
    [Tooltip("Margem (unidades) abaixo do topo da camera para travar o jogador.")]
    public float cameraTopMargin = 0.5f;
    [Tooltip("Margem (unidades) a partir da borda esquerda da camera em que o jogador e travado (a camera o carrega).")]
    public float cameraLeftMargin = 0.5f;

    [Header("Morte por queda")]
    [Tooltip("Se o jogador cair abaixo deste Y, morre (buraco).")]
    public float killY = -8f;

    [Header("Auto-pilot (menu)")]
    [Tooltip("Intervalo minimo (s) entre acoes aleatorias do jogador no menu.")]
    public float autoPilotMinInterval = 0.5f;
    [Tooltip("Intervalo maximo (s) entre acoes aleatorias do jogador no menu.")]
    public float autoPilotMaxInterval = 2.2f;
}