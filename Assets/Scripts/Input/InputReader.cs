using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour
{
    [Header("Pulo")]
    [Tooltip("Tecla de pulo.")]
    public Key jumpKey = Key.Space;
    [Tooltip("Segunda tecla de pulo (W).")]
    public Key jumpKeyAlt = Key.W;

    [Header("Movimento (WASD)")]
    [Tooltip("Mover para tras.")]
    public Key moveLeftKey = Key.A;
    [Tooltip("Mover para frente.")]
    public Key moveRightKey = Key.D;
    [Tooltip("Cair mais rapido (segurar no ar).")]
    public Key fastFallKey = Key.S;

    [Header("Parry")]
    [Tooltip("Tecla de parry (só funciona no ar).")]
    public Key parryKey = Key.K;

    [Header("Reiniciar")]
    [Tooltip("Tecla para reiniciar após o game over.")]
    public Key restartKey = Key.R;

    public bool JumpPressed => Keyboard.current != null &&
        (Keyboard.current[jumpKey].wasPressedThisFrame || Keyboard.current[jumpKeyAlt].wasPressedThisFrame);
    public bool JumpReleased => Keyboard.current != null &&
        (Keyboard.current[jumpKey].wasReleasedThisFrame || Keyboard.current[jumpKeyAlt].wasReleasedThisFrame);
    public bool ParryPressed => Keyboard.current != null && Keyboard.current[parryKey].wasPressedThisFrame;
    public bool ParryHeld => Keyboard.current != null && Keyboard.current[parryKey].isPressed;
    public bool MoveLeft => Keyboard.current != null && Keyboard.current[moveLeftKey].isPressed;
    public bool MoveRight => Keyboard.current != null && Keyboard.current[moveRightKey].isPressed;
    public bool FastFallHeld => Keyboard.current != null && Keyboard.current[fastFallKey].isPressed;
    public bool RestartPressed => Keyboard.current != null && Keyboard.current[restartKey].wasPressedThisFrame;
}
