using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Tooltip("Velocidade base de rolagem para frente usada se nao houver DifficultyDriver.")]
    public float scrollSpeed = 8f;
    [Tooltip("Driver de dificuldade (define a velocidade de rolagem). Se vazio, procura na cena.")]
    public DifficultyDriver difficulty;
    [Tooltip("Altura fixa da camera.")]
    public float fixedY = 2f;

    private float _shakeTime;
    private float _shakeDuration = 0.25f;
    private float _shakeIntensity = 0.4f;

    public void Shake(float duration, float intensity)
    {
        _shakeDuration = Mathf.Max(0.001f, duration);
        _shakeIntensity = intensity;
        _shakeTime = _shakeDuration;
    }

    void Start()
    {
        if (difficulty == null)
            difficulty = FindFirstObjectByType<DifficultyDriver>();
    }

    void LateUpdate()
    {
        var gm = GameManager.Instance;
        bool playing = gm != null && gm.State == GameState.Playing;
        bool menu = gm != null && gm.State == GameState.Menu;

        // A camera rola sozinha para frente, sem acompanhar o jogador.
        // O jogador pode ficar para tras (encosta na borda esquerda) ou
        // ir para frente alem da camera, sem limite.
        float camX = transform.position.x;
        if (playing || menu)
        {
            float speed = difficulty != null ? difficulty.Speed : scrollSpeed;
            camX += speed * Time.deltaTime;
        }

        Vector3 pos = new Vector3(camX, fixedY, -10f);

        if (_shakeTime > 0f)
        {
            _shakeTime -= Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(_shakeTime / _shakeDuration);
            pos += (Vector3)(Random.insideUnitCircle * (_shakeIntensity * k));
        }

        transform.position = pos;
    }
}