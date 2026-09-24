using UnityEngine;

public class Projectile : MonoBehaviour, IPoolable, IParryable
{
    [Tooltip("Velocidade do projetil para a esquerda.")]
    public float speed = 9f;

    [Header("Visual")]
    [Tooltip("Animacao em loop do sprite (clip criado na janela Animation, animando o SpriteRenderer do filho 'Visual').")]
    public AnimationClip animation;
    [Tooltip("Velocidade de rotacao do sprite em graus por segundo. 0 desativa o giro.")]
    public float rotationSpeed = 180f;
    [Tooltip("Tint aplicada ao sprite. Use branco para manter a cor original das frames.")]
    public Color spriteTint = Color.white;

    public bool IsParried { get; private set; }

    private Transform _player;
    private Animator _anim;
    private Transform _visual;
    private SpriteRenderer _sprite;
    private bool _animReady;

    void Awake()
    {
        var pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) _player = pc.transform;

        _anim = GetComponent<Animator>();
        if (_anim == null) _anim = gameObject.AddComponent<Animator>();

        _sprite = GetComponentInChildren<SpriteRenderer>();
        if (transform.childCount > 0) _visual = transform.GetChild(0);

        SetupAnimator();
    }

    private void SetupAnimator()
    {
        if (_anim == null) return;

        var runtimeBase = Resources.Load<RuntimeAnimatorController>("Configs/ProjectileStateController");
        if (runtimeBase == null)
        {
            Debug.LogWarning("[Projectile] Base controller nao encontrado (Resources/Configs/ProjectileStateController). Verifique o GameSetup.");
            return;
        }

        var overrides = new AnimatorOverrideController(runtimeBase);
        if (animation != null && !animation.empty) overrides["ProjectileSpin"] = animation;
        _anim.runtimeAnimatorController = overrides;
        _animReady = true;
    }

    public void OnSpawned()
    {
        IsParried = false;
        if (_sprite != null) _sprite.color = spriteTint;
        if (_visual != null) _visual.localRotation = Quaternion.identity;
        if (_animReady) _anim.Play("Spin", 0, 0f);
    }

    public void OnDespawned() { }

    void Update()
    {
        transform.position += Vector3.left * speed * Time.deltaTime;

        if (_visual != null && rotationSpeed != 0f)
            _visual.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

        if (_player != null && transform.position.x < _player.position.x - 25f)
        {
            Despawn();
        }
    }

    public void Parry()
    {
        if (IsParried) return;
        IsParried = true;
    }

    public void Remove()
    {
        Despawn();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (IsParried) return;
        if (other.CompareTag("Player"))
        {
            var pc = other.GetComponentInParent<PlayerController>();
            if (pc != null && pc.IsDashing) return;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }
        }
    }

    private void Despawn()
    {
        if (ObjectPool.Instance != null)
            ObjectPool.Instance.Despawn(gameObject);
        else
            Destroy(gameObject);
    }
}