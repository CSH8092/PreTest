using DG.Tweening;
using UnityEngine;

public class HighlightEffect : MonoBehaviour
{
    [Header("Color")]
    [SerializeField] private Color color = Color.white;
    [SerializeField] private float emissionIntensity = 6f;
    [SerializeField] private float colorTweenDuration = 2f;

    [Header("Shake")]
    [SerializeField] private float shakeDuration = 0.4f;
    [SerializeField] private float shakePositionStrength = 0.15f;
    [SerializeField] private float shakeScaleStrength = 0.25f;
    [SerializeField] private int shakeVibrato = 10;

    [Header("Particle")]
    [SerializeField] private ParticleEffect prefab_effect;

    private Renderer _renderer;
    private Material _mat;
    private ParticleEffect _particleEffect;

    private Color _originalBaseColor;
    private Color _originalEmissionColor;
    private Vector3 _originalLocalPosition;
    private Vector3 _originalLocalScale;

    private float _emissionIntensity;
    private Tween _colorTween;
    private Tween _emissionTween;
    private bool _isPlaying;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _mat = _renderer.material;

        _originalBaseColor = _mat.GetColor("_BaseColor");
        _originalEmissionColor = _mat.GetColor("_EmissionColor");
        _originalLocalPosition = transform.localPosition;
        _originalLocalScale = transform.localScale;

        if (prefab_effect != null)
        {
            _particleEffect = Instantiate(prefab_effect, transform.position, Quaternion.identity, transform);
        }
    }

    public void Play()
    {
        _isPlaying = true;

        _colorTween?.Kill();
        _emissionTween?.Kill();

        _mat.EnableKeyword("_EMISSION");
        _colorTween = _mat.DOColor(color, "_BaseColor", colorTweenDuration);
        _emissionTween = DOTween.To(() => _emissionIntensity, SetEmissionIntensity, emissionIntensity, colorTweenDuration);

        TryStartShakeLoop();
        _particleEffect?.Play();
    }

    public void Stop()
    {
        _isPlaying = false;

        _colorTween?.Kill();
        _emissionTween?.Kill();

        _mat.SetColor("_BaseColor", _originalBaseColor);
        _mat.SetColor("_EmissionColor", _originalEmissionColor);
        _mat.DisableKeyword("_EMISSION");
        _emissionIntensity = 0f;

        // 현재 진행 중인 사이클 끝내고 종료
        _particleEffect?.StopLoop();
    }

    private void TryStartShakeLoop()
    {
        if (!_isPlaying)
        {
            // 원상복귀
            transform.DOKill();
            transform.localPosition = _originalLocalPosition;
            transform.localScale = _originalLocalScale;
            return;
        }

        transform.DOShakePosition(shakeDuration, shakePositionStrength, shakeVibrato).OnComplete(TryStartShakeLoop);
        transform.DOShakeScale(shakeDuration, shakeScaleStrength, shakeVibrato);
    }

    private void SetEmissionIntensity(float intensity)
    {
        _emissionIntensity = intensity;
        _mat.SetColor("_EmissionColor", color * Mathf.Pow(2f, intensity));
    }
}
