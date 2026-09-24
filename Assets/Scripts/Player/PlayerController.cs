using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Driver de dificuldade (controla a velocidade). Se vazio, usa Config.baseSpeed.")]
    public DifficultyDriver difficulty;

    [Tooltip("Config do jogador. Se vazio, carrega de Resources/Configs/PlayerConfig ou usa padroes.")]
    public PlayerConfig config;

    [Header("Deteccao de chao")]
    [Tooltip("Camadas consideradas chao. Vazio (Everything) detecta qualquer colisor solido.")]
    public LayerMask groundMask = ~0;

    private PlayerConfig _loadedConfig;

    public PlayerConfig Config
    {
        get
        {
            if (_loadedConfig == null)
            {
                if (config != null) _loadedConfig = config;
                else
                {
                    _loadedConfig = Resources.Load<PlayerConfig>("Configs/PlayerConfig");
                    if (_loadedConfig == null) _loadedConfig = ScriptableObject.CreateInstance<PlayerConfig>();
                }
            }
            return _loadedConfig;
        }
    }

    public bool IsGrounded { get; private set; }
    public bool IsAirborne => !IsGrounded;

    public event System.Action JumpStarted;

    private Rigidbody2D _rb;
    private Collider2D _col;
    private InputReader _input;
    private FeedbackManager _feedback;
    private float _coyoteTimer;
    private float _jumpBufferTimer;
    private float _dashTimer;
    private bool _hasParried;
    private ParryIndicator _indicator;
    private PlayerAnimator _playerAnimator;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();
    }

    void Start()
    {
        _rb.gravityScale = Config.gravityScale;
        _rb.freezeRotation = true;
        _input = FindFirstObjectByType<InputReader>();
        _feedback = FindFirstObjectByType<FeedbackManager>();

        if (GetComponent<ParryIndicator>() == null)
            gameObject.AddComponent<ParryIndicator>();
        _indicator = GetComponent<ParryIndicator>();

        if (GetComponent<PlayerAnimator>() == null)
            gameObject.AddComponent<PlayerAnimator>();
        _playerAnimator = GetComponent<PlayerAnimator>();
    }

    void Update()
    {
        var gm = GameManager.Instance;
        if (gm == null || gm.State != GameState.Playing) return;

        if (_input != null && _input.JumpPressed)
        {
            _jumpBufferTimer = Config.jumpBufferTime;
        }

        if (_input != null && _input.ParryPressed)
        {
            Debug.Log("[Parry] K pressionado | noAr=" + IsAirborne + " | jaUsouNestePulo=" + _hasParried);
            if (gm.ParryEnabled && IsAirborne && !_hasParried)
            {
                _hasParried = true;
                Debug.Log("[Parry] INPUT DE PARRY RECONHECIDO (no ar e disponivel)");
                AudioManager.Instance?.PlayParryInput();
                if (_indicator != null) _indicator.Flash();
                TryParry();
            }
        }

        if (transform.position.y < Config.killY)
        {
            gm.GameOver();
        }
    }

    void FixedUpdate()
    {
        var gm = GameManager.Instance;
        bool playing = gm != null && gm.State == GameState.Playing;

        if (_dashTimer > 0f) _dashTimer -= Time.fixedDeltaTime;
        float speed = _dashTimer > 0f
            ? Config.dashSpeed
            : (difficulty != null ? difficulty.Speed : Config.baseSpeed);

        if (playing)
            _rb.linearVelocity = new Vector2(speed, _rb.linearVelocity.y);
        else
            _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);

        IsGrounded = CheckGround();

        if (playing)
        {
            if (IsGrounded)
            {
                _coyoteTimer = Config.coyoteTime;
                _hasParried = false;
            }
            else _coyoteTimer -= Time.fixedDeltaTime;

            _jumpBufferTimer -= Time.fixedDeltaTime;
            if (_jumpBufferTimer < 0f) _jumpBufferTimer = 0f;

            if (_jumpBufferTimer > 0f && _coyoteTimer > 0f)
            {
                Jump();
                _jumpBufferTimer = 0f;
                _coyoteTimer = 0f;
            }
        }

        if (gm != null)
            gm.SetDistance(transform.position.x);
    }

    private void Jump()
    {
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, Config.jumpVelocity);
        AudioManager.Instance?.PlayJump();
        JumpStarted?.Invoke();
    }

    private void Dash()
    {
        _dashTimer = Config.dashDuration;
    }

    private bool CheckGround()
    {
        if (_col == null) return false;
        Bounds b = _col.bounds;
        Vector2 center = new Vector2(b.center.x, b.min.y - Config.groundCheckDistance * 0.5f);
        Vector2 size = new Vector2(b.size.x * Config.groundCheckWidth, Config.groundCheckDistance);
        var hits = Physics2D.OverlapBoxAll(center, size, 0f, groundMask);
        foreach (var hit in hits)
        {
            if (hit == _col) continue;        // ignora o proprio colisor do jogador
            if (hit.isTrigger) continue;      // ignora triggers (obstaculos/projeteis)
            return true;                      // achou chao solido
        }
        return false;
    }

    private void TryParry()
    {
        if (!IsAirborne)
        {
            Debug.Log("[Parry] tentativa ignorada: jogador no chao");
            return;
        }

        var cols = Physics2D.OverlapCircleAll(transform.position, Config.parryRange);
        Projectile closest = null;
        float best = float.MaxValue;
        int count = 0;
        foreach (var c in cols)
        {
            var p = c.GetComponentInParent<Projectile>();
            if (p == null || p.IsParried) continue;
            count++;
            if (p.transform.position.x < transform.position.x - 0.5f) continue;

            float d = (p.transform.position - transform.position).sqrMagnitude;
            if (d < best) { best = d; closest = p; }
        }

        if (closest != null)
        {
            closest.Parry();
            if (GameManager.Instance != null) GameManager.Instance.RegisterParry();
            if (_feedback != null) _feedback.TriggerParryFeedback(transform.position);
            AudioManager.Instance?.PlayParry();
            Dash();
            if (_playerAnimator != null) _playerAnimator.TriggerParry();
            Debug.Log("[Parry] SUCESSO! combo=" + (GameManager.Instance != null ? GameManager.Instance.Combo.ToString() : "?"));
        }
        else
        {
            Debug.Log("[Parry] sem projetil no alcance | projeteis vistos=" + count + " | raio=" + Config.parryRange);
        }
    }
}