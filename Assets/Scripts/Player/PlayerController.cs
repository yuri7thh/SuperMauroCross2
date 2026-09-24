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
    public bool IsDashing => _dashTimer > 0f;

    public event System.Action JumpStarted;

    private Rigidbody2D _rb;
    private Collider2D _col;
    private InputReader _input;
    private FeedbackManager _feedback;
    private float _coyoteTimer;
    private float _jumpBufferTimer;
    private float _dashTimer;
    private float _dashSpeed;
    private Vector2 _dashDir = Vector2.right;
    private bool _hasParried;
    private bool _canAirJump;
    private bool _pendingParryJump;
    private Vector2 _parryJumpTarget;
    private float _parryDecelTimer;
    private ParryIndicator _indicator;
    private PlayerAnimator _playerAnimator;
    private Camera _cam;

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
        _cam = Camera.main;

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
            if (IsAirborne && _canAirJump)
            {
                Jump();
                _canAirJump = false;
                _jumpBufferTimer = 0f;
                _coyoteTimer = 0f;
            }
            else
            {
                _jumpBufferTimer = Config.jumpBufferTime;
            }
        }

        if (_input != null && _input.JumpReleased && Config.jumpCutEnabled && _rb != null && _rb.linearVelocity.y > 0f)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _rb.linearVelocity.y * Config.jumpCutMultiplier);
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

        if (_input != null && _input.ParryJumpPressed)
        {
            Debug.Log("[ParryJump] L pressionado | noAr=" + IsAirborne + " | jaUsouNestePulo=" + _hasParried);
            if (gm.ParryEnabled && IsAirborne && !_hasParried)
            {
                _hasParried = true;
                AudioManager.Instance?.PlayParryInput();
                if (_indicator != null) _indicator.Flash();
                TryParryJump();
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

        IsGrounded = CheckGround();

        float topLimit = GetCameraTopLimit();
        bool hitTop = Config.clampToCameraTop && topLimit != float.MaxValue && transform.position.y >= topLimit;

        if (_dashTimer > 0f && (IsGrounded || hitTop))
        {
            _dashTimer = 0f;
            _pendingParryJump = false;
            if (hitTop && _rb.linearVelocity.y > 0f)
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0f);
        }

        if (_dashTimer > 0f)
        {
            _dashTimer -= Time.fixedDeltaTime;

            bool jumpNow = false;
            if (_pendingParryJump)
            {
                Vector2 toTarget = _parryJumpTarget - (Vector2)transform.position;
                jumpNow = Vector2.Dot(toTarget, _dashDir) <= 0f;
            }

            if (jumpNow || _dashTimer <= 0f)
            {
                _dashTimer = 0f;
                if (_pendingParryJump)
                {
                    _pendingParryJump = false;
                    _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, Config.parryJumpVelocity);
                    _parryDecelTimer = Config.parryJumpDecelerationTime;
                    AudioManager.Instance?.PlayJump();
                    JumpStarted?.Invoke();
                }
            }
        }

        if (Config.rotateSpriteWithDash && _dashTimer <= 0f && Quaternion.Angle(transform.rotation, Quaternion.identity) > 0.1f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.identity, Config.dashRotateResetSpeed * Time.fixedDeltaTime);
        }

        if (playing)
        {
            if (_dashTimer > 0f)
            {
                _rb.gravityScale = 0f;
                _rb.linearVelocity = _dashDir * _dashSpeed;
            }
            else
            {
                _rb.gravityScale = Config.gravityScale;
                float speed = difficulty != null ? difficulty.Speed : Config.baseSpeed;
                if (Config.slowInAir && IsAirborne)
                    speed *= Config.airSpeedMultiplier;

                if (_parryDecelTimer > 0f)
                {
                    _parryDecelTimer -= Time.fixedDeltaTime;
                    speed *= (1f - Config.parryJumpVerticality);
                    speed *= Config.parryJumpDeceleration;
                }

                _rb.linearVelocity = new Vector2(speed, _rb.linearVelocity.y);
            }
        }
        else
        {
            _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
        }

        if (Config.clampToCameraTop && topLimit != float.MaxValue && _rb.position.y > topLimit)
        {
            Vector2 p = _rb.position;
            p.y = topLimit;
            _rb.position = p;
            if (_rb.linearVelocity.y > 0f)
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0f);
        }

        if (playing)
        {
            if (IsGrounded)
            {
                _coyoteTimer = Config.coyoteTime;
                _hasParried = false;
                _canAirJump = false;
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

    private float GetCameraTopLimit()
    {
        if (_cam == null || !_cam.orthographic) return float.MaxValue;
        return _cam.transform.position.y + _cam.orthographicSize - Config.cameraTopMargin;
    }

    private void Jump()
    {
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, Config.jumpVelocity);
        AudioManager.Instance?.PlayJump();
        JumpStarted?.Invoke();
    }

    private void Dash(Vector2 targetPos)
    {
        Vector2 toTarget = targetPos - (Vector2)transform.position;
        float dist = toTarget.magnitude;
        float travel = Mathf.Max(dist, Config.dashDistance);

        _dashTimer = Config.dashDuration;
        _dashSpeed = travel / Mathf.Max(0.01f, Config.dashDuration);
        _dashDir = dist > 0.0001f ? toTarget / dist : Vector2.right;

        if (Config.rotateSpriteWithDash)
        {
            float angle = Mathf.Atan2(_dashDir.y, _dashDir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
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

        var target = FindNearestParryTarget();
        if (target != null)
        {
            var p = target.GetComponentInParent<IParryable>();
            if (p != null)
            {
                ParryTarget(p);
                Debug.Log("[Parry] SUCESSO! combo=" + (GameManager.Instance != null ? GameManager.Instance.Combo.ToString() : "?"));
            }
        }
        else
        {
            Debug.Log("[Parry] sem alvo no alcance | raio=" + Config.parryRange);
        }
    }

    private void TryParryJump()
    {
        if (!IsAirborne)
        {
            Debug.Log("[ParryJump] tentativa ignorada: jogador no chao");
            return;
        }

        var target = FindNearestParryTarget();
        if (target != null)
        {
            var p = target.GetComponentInParent<IParryable>();
            if (p != null)
            {
                ParryTargetJump(p);
                Debug.Log("[ParryJump] SUCESSO! combo=" + (GameManager.Instance != null ? GameManager.Instance.Combo.ToString() : "?"));
            }
        }
        else
        {
            Debug.Log("[ParryJump] sem alvo no alcance | raio=" + Config.parryRange);
        }
    }

    private void ApplyParryEffects(IParryable target)
    {
        target.Parry();
        _hasParried = false;
        _canAirJump = true;
        if (GameManager.Instance != null) GameManager.Instance.RegisterParry();
        if (_feedback != null) _feedback.TriggerParryFeedback(transform.position);
        AudioManager.Instance?.PlayParry();
        if (_playerAnimator != null) _playerAnimator.TriggerParry();
    }

    private void ParryTarget(IParryable target)
    {
        var targetTransform = (target as Component).transform;
        Vector2 targetPos = targetTransform.position;

        ApplyParryEffects(target);

        Dash(targetPos);
    }

    private void ParryTargetJump(IParryable target)
    {
        var targetTransform = (target as Component).transform;
        Vector2 targetPos = targetTransform.position;

        ApplyParryEffects(target);

        Dash(targetPos);
        _pendingParryJump = true;
        _parryJumpTarget = targetPos;
    }

    // Enquanto escorrega (dash) depois de um parry, faz parry automatico em qualquer
    // coisa parryable que estiver no trajeto, mantendo a corrente.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsDashing) return;

        var p = other.GetComponentInParent<IParryable>();
        if (p == null || p.IsParried) return;
        if (other.transform.position.x < transform.position.x - 0.5f) return;

        ParryTarget(p);
        Debug.Log("[Parry] CADEIA no dash! combo=" + (GameManager.Instance != null ? GameManager.Instance.Combo.ToString() : "?"));
    }

    /// <summary>
    /// Encontra o alvo de parry mais proximo na frente do jogador (ignora o que esta atras),
    /// dentro do raio de parry configurado. Usado tanto pelo parry em si quanto pelo indicador.
    /// </summary>
    public Collider2D FindNearestParryTarget()
    {
        var cols = Physics2D.OverlapCircleAll(transform.position, Config.parryRange);
        Collider2D best = null;
        float bestDist = float.MaxValue;
        foreach (var c in cols)
        {
            var p = c.GetComponentInParent<IParryable>();
            if (p == null || p.IsParried) continue;
            if (c.transform.position.x < transform.position.x - 0.5f) continue;

            float d = (c.transform.position - transform.position).sqrMagnitude;
            if (d < bestDist) { bestDist = d; best = c; }
        }
        return best;
    }
}