using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private enum State { Run, JumpUp, JumpDown, Landing, Parry }

    [Header("Conjunto de animacoes (clips por acao).")]
    [Tooltip("Se vazio, carrega de Resources/Configs/PlayerAnimationSet.")]
    public PlayerAnimationSet animationSet;

    [Tooltip("Controller base gerado pelo editor. Se vazio, carrega de Resources/Configs/PlayerStateController.")]
    public RuntimeAnimatorController baseController;

    private PlayerAnimationSet _loaded;
    private PlayerController _player;
    private Rigidbody2D _rb;
    private Animator _anim;
    private State _state = State.Run;
    private float _parryRemaining;
    private float _landingRemaining;
    private bool _wasGrounded;
    private bool _justJumped;

    void Awake()
    {
        _player = GetComponent<PlayerController>();
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
        if (_anim == null) _anim = gameObject.AddComponent<Animator>();
    }

    void Start()
    {
        _loaded = animationSet != null ? animationSet : Resources.Load<PlayerAnimationSet>("Configs/PlayerAnimationSet");
        SetupOverrides();
        _wasGrounded = _player != null && _player.IsGrounded;
        Play(_wasGrounded ? State.Run : State.JumpDown);
        if (_player != null) _player.JumpStarted += OnJumpStarted;
    }

    void OnDestroy()
    {
        if (_player != null) _player.JumpStarted -= OnJumpStarted;
    }

    void Update()
    {
        if (_anim == null || _player == null) return;

        bool grounded = _player.IsGrounded;
        float velY = _rb != null ? _rb.linearVelocity.y : 0f;

        if (_parryRemaining > 0f)
        {
            _parryRemaining -= Time.deltaTime;
            Play(State.Parry);
        }
        else if (grounded && !_justJumped)
        {
            if (!_wasGrounded)
            {
                _landingRemaining = LengthOr(Slot(State.Landing), 0.1f);
                Play(State.Landing);
            }
            else if (_state == State.Landing && _landingRemaining > 0f)
            {
                _landingRemaining -= Time.deltaTime;
            }
            else
            {
                Play(State.Run);
            }
        }
        else
        {
            _justJumped = false;
            Play(velY < 0f ? State.JumpDown : State.JumpUp);
        }

        _wasGrounded = grounded;
    }

    private void OnJumpStarted()
    {
        _justJumped = true;
        Play(State.JumpUp, true);
    }

    public void TriggerParry()
    {
        _parryRemaining = LengthOr(Slot(State.Parry), 0.25f);
        Play(State.Parry, true);
    }

    private void SetupOverrides()
    {
        if (_anim == null || _loaded == null) return;

        var runtimeBase = baseController != null
            ? baseController
            : Resources.Load<RuntimeAnimatorController>("Configs/PlayerStateController");
        if (runtimeBase == null)
        {
            Debug.LogWarning("[PlayerAnimator] Base controller nao encontrado (Resources/Configs/PlayerStateController). Verifique o GameSetup.");
            return;
        }

        var overrides = new AnimatorOverrideController(runtimeBase);
        SetOverride(overrides, "Run", _loaded.run);
        SetOverride(overrides, "JumpUp", _loaded.jumpUp);
        SetOverride(overrides, "JumpDown", _loaded.jumpDown);
        SetOverride(overrides, "Landing", _loaded.landing);
        SetOverride(overrides, "Parry", _loaded.parry);
        _anim.runtimeAnimatorController = overrides;
    }

    private static void SetOverride(AnimatorOverrideController overrides, string name, AnimationClip clip)
    {
        if (clip != null) overrides[name] = clip;
    }

    private void Play(State s)
    {
        Play(s, false);
    }

    private void Play(State s, bool restart)
    {
        if (!restart && _state == s) return;
        if (_anim == null || _anim.runtimeAnimatorController == null) return;

        _state = s;
        _anim.Play(s.ToString(), 0, 0f);
    }

    private AnimationClip Slot(State s)
    {
        if (_loaded == null) return null;
        switch (s)
        {
            case State.Run: return _loaded.run;
            case State.JumpUp: return _loaded.jumpUp;
            case State.JumpDown: return _loaded.jumpDown;
            case State.Landing: return _loaded.landing;
            case State.Parry: return _loaded.parry;
            default: return null;
        }
    }

    private static float LengthOr(AnimationClip clip, float fallback)
    {
        return clip != null ? clip.length : fallback;
    }
}