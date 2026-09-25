using UnityEngine;
using UnityEngine.EventSystems;

public class HoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Tooltip("Qual objeto sera escalado. Se vazio, escala o proprio GameObject.")]
    public RectTransform target;

    [Tooltip("Escala ao passar o mouse/dedo por cima.")]
    public float hoverScale = 1.2f;

    [Tooltip("Escala ao clicar/pressionar.")]
    public float pressScale = 0.95f;

    [Tooltip("Velocidade da transicao (maior = mais rapido).")]
    public float speed = 10f;

    private RectTransform _rt;
    private Vector3 _baseScale = Vector3.one;
    private bool _initialized;
    private float _currentMult = 1f;
    private float _targetMult = 1f;

    private void EnsureInit()
    {
        if (_initialized) return;
        _rt = target != null ? target : (transform as RectTransform);
        if (_rt != null) _baseScale = _rt.localScale;
        _initialized = true;
    }

    void Update()
    {
        EnsureInit();
        if (_rt == null) return;

        _currentMult = Mathf.Lerp(_currentMult, _targetMult, Time.unscaledDeltaTime * speed);
        _rt.localScale = _baseScale * _currentMult;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        EnsureInit();
        _targetMult = hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _targetMult = 1f;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _targetMult = pressScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _targetMult = hoverScale;
    }
}
