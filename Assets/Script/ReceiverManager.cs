using DG.Tweening;
using UnityEngine;

public class ReceiverManager : MonoBehaviour
{
    public enum EState
    {
        Idle,
        Success
    }

    [SerializeField] private EState currentState = EState.Idle;
    public EState CurrentState => currentState;

    [Header("Success Color")]
    [SerializeField] private Color color_success = Color.white; // success color
    [SerializeField] private float emissionIntensity_success = 6f; // success 세기
    [SerializeField] private float colorTweenDuration = 2f; // 시간

    [Header("Success Shake")]
    [SerializeField] private float shakeDuration = 0.4f; // shake 1회 시간
    [SerializeField] private float shakePositionStrength = 0.15f; // pos shake 강도
    [SerializeField] private float shakeScaleStrength = 0.25f; // scale shake 강도
    [SerializeField] private int shakeVibrato = 10; // shake 빈도

    [Header("Success Particle")]
    [SerializeField] private ReceiverParticleEffet prefab_effect;

    private Renderer _renderer;
    private Material _mat;
    private Transform _trSphere;
    private ReceiverParticleEffet _particleEffect;

    private Color _originalBaseColor;
    private Color _originalEmissionColor;

    private float _emissionIntensity;
    private Tween _colorTween;
    private Tween _emissionTween;

    private void Awake()
    {
        _renderer = GetComponentInChildren<Renderer>();
        _mat = _renderer.material;
        _trSphere = _renderer.transform;

        _originalBaseColor = _mat.GetColor("_BaseColor");
        _originalEmissionColor = _mat.GetColor("_EmissionColor");

        if (prefab_effect != null)
        {
            _particleEffect = Instantiate(prefab_effect, _trSphere.position, Quaternion.identity, _trSphere);
        }
    }

    public void SetState(EState newState)
    {
        if (newState == currentState)
        {
            return;
        }

        currentState = newState;
        Debug.Log($"[Receiver] {name} set {currentState}");

        if (currentState == EState.Success)
        {
            PlaySuccessEffect();
        }
        else
        {
            StopSuccessEffect();
        }
    }
    
    private void PlaySuccessEffect()
    {
        _colorTween?.Kill();
        _emissionTween?.Kill();

        _mat.EnableKeyword("_EMISSION");
        _colorTween = _mat.DOColor(color_success, "_BaseColor", colorTweenDuration);
        _emissionTween = DOTween.To(() => _emissionIntensity, SetEmissionIntensity, emissionIntensity_success, colorTweenDuration);

        TryStartShakeLoop();
        _particleEffect?.Play();
    }

    private void TryStartShakeLoop()
    {
        if (currentState != EState.Success)
        {
            return;
        }

        _trSphere.DOShakePosition(shakeDuration, shakePositionStrength, shakeVibrato).OnComplete(TryStartShakeLoop);
        _trSphere.DOShakeScale(shakeDuration, shakeScaleStrength, shakeVibrato);
    }

    private void SetEmissionIntensity(float intensity)
    {
        _emissionIntensity = intensity;
        _mat.SetColor("_EmissionColor", Color.white * intensity);
    }

    private void StopSuccessEffect()
    {
        _colorTween?.Kill();
        _emissionTween?.Kill();

        _mat.SetColor("_BaseColor", _originalBaseColor);
        _mat.SetColor("_EmissionColor", _originalEmissionColor);
        _mat.DisableKeyword("_EMISSION");
        _emissionIntensity = 0f;

        // 현재 진행 중인 사이클 끝내고 종료
        _particleEffect?.StopLoop();
    }
}
