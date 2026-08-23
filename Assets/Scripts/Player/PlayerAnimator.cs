using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [Header("Nomes dos parametros (devem bater com o seu Animator Controller)")]
    [Tooltip("Bool: esta no chao.")]
    public string isGroundedParam = "IsGrounded";
    [Tooltip("Float: velocidade vertical (positiva = subindo, negativa = caindo).")]
    public string velYParam = "VelY";
    [Tooltip("Float: velocidade horizontal (para acelerar a animacao de run).")]
    public string speedParam = "Speed";
    [Tooltip("Trigger: disparado ao dar parry.")]
    public string parryParam = "Parry";

    private Animator _anim;
    private PlayerController _player;
    private Rigidbody2D _rb;

    void Awake()
    {
        _player = GetComponent<PlayerController>();
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
        if (_anim == null) _anim = gameObject.AddComponent<Animator>();
    }

    void Update()
    {
        if (_anim == null || _player == null) return;

        _anim.SetBool(isGroundedParam, _player.IsGrounded);
        if (_rb != null) _anim.SetFloat(velYParam, _rb.linearVelocity.y);
        if (_rb != null) _anim.SetFloat(speedParam, Mathf.Abs(_rb.linearVelocity.x));
    }

    public void TriggerParry()
    {
        if (_anim != null) _anim.SetTrigger(parryParam);
    }
}