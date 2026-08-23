using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour
{
    [Header("Pulo")]
    [Tooltip("Tecla de pulo.")]
    public Key jumpKey = Key.Space;

    [Header("Parry")]
    [Tooltip("Tecla de parry (só funciona no ar).")]
    public Key parryKey = Key.K;

    [Header("Reiniciar")]
    [Tooltip("Tecla para reiniciar após o game over.")]
    public Key restartKey = Key.R;

    public bool JumpPressed => Keyboard.current != null && Keyboard.current[jumpKey].wasPressedThisFrame;
    public bool ParryPressed => Keyboard.current != null && Keyboard.current[parryKey].wasPressedThisFrame;
    public bool RestartPressed => Keyboard.current != null && Keyboard.current[restartKey].wasPressedThisFrame;
}
