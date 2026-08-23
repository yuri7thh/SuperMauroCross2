using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Tooltip("Alvo a seguir (o jogador).")]
    public Transform target;
    [Tooltip("Velocidade de suavizacao do follow.")]
    public float followSpeed = 10f;
    [Tooltip("Deslocamento em X a frente do alvo.")]
    public float lookAhead = 3f;
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

    void LateUpdate()
    {
        Vector3 desired;
        if (target != null)
            desired = new Vector3(target.position.x + lookAhead, fixedY, -10f);
        else
            desired = transform.position;

        Vector3 follow = Vector3.Lerp(transform.position, desired, Mathf.Clamp01(Time.deltaTime * followSpeed));

        Vector3 offset = Vector3.zero;
        if (_shakeTime > 0f)
        {
            _shakeTime -= Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(_shakeTime / _shakeDuration);
            offset = Random.insideUnitCircle * (_shakeIntensity * k);
        }

        transform.position = follow + offset;
    }
}
