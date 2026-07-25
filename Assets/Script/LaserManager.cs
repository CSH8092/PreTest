using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class LaserManager : MonoBehaviour
{
    public enum EState
    {
        Normal,
        MaxBounce
    }

    [SerializeField] private LineRenderer line_laser;
    [SerializeField] private Transform tr_muzzle;

    [Header("Settings")]
    [SerializeField] private int maxMirrorBounces = 10; // 최대 튕길 횟수
    [SerializeField] private float maxRayDistance = 200f; // max ray 길이
    [SerializeField] private float notHitDistance = 50f; // 미충돌 시 ray 길이
    [SerializeField] private float laserOffset = 0.05f; // laser 여유 값

    [Header("Text")]
    [SerializeField] private TextMeshPro text_current;
    [SerializeField] private Transform tr_upper;
    [SerializeField] private Camera cam_main;
    [SerializeField] private float textHeightOffset = 5f;

    [Header("MaxBounce Color")]
    [SerializeField] private Color color_maxBounce = Color.red;
    [SerializeField] private float emissionIntensity_maxBounce = 6f;
    [SerializeField] private float colorTweenDuration = 2f;

    [Header("MaxBounce Shake")]
    [SerializeField] private float shakeDuration = 0.4f;
    [SerializeField] private float shakePositionStrength = 0.15f;
    [SerializeField] private float shakeScaleStrength = 0.25f;
    [SerializeField] private int shakeVibrato = 10;

    [Header("MaxBounce Particle")]
    [SerializeField] private ReceiverParticleEffet prefab_effect;

    private int _wallLayer;
    private int _mirrorLayer;
    private int _receiverLayer;
    private int _hitMask;

    private readonly List<Vector3> _points = new List<Vector3>();

    private Material _matUpper;
    private ReceiverParticleEffet _particleEffect;

    private Color _originalBaseColor;
    private Color _originalEmissionColor;
    private Vector3 _originalLocalPosition;
    private Vector3 _originalLocalScale;

    private float _emissionIntensity;
    private Tween _colorTween;
    private Tween _emissionTween;

    [Header("Debug")]
    [SerializeField] private ReceiverManager currentReceiver;
    [SerializeField] private int currentBounceCount;
    [SerializeField] private EState currentState = EState.Normal;
    public int CurrentBounceCount => currentBounceCount;
    public EState CurrentState => currentState;

    private void Awake()
    {
        line_laser.useWorldSpace = true;

        if (cam_main == null)
        {
            cam_main = Camera.main;
        }

        // hit mask setting
        _wallLayer = LayerMask.NameToLayer(ConstData.WallLayer);
        _mirrorLayer = LayerMask.NameToLayer(ConstData.MirrorLayer);
        _receiverLayer = LayerMask.NameToLayer(ConstData.ReceiverLayer);
        _hitMask = (1 << _wallLayer) | (1 << _mirrorLayer) | (1 << _receiverLayer);

        Renderer rendererUpper = tr_upper.GetComponent<Renderer>();
        _matUpper = rendererUpper.material;
        _originalBaseColor = _matUpper.GetColor("_BaseColor");
        _originalEmissionColor = _matUpper.GetColor("_EmissionColor");
        _originalLocalPosition = tr_upper.localPosition;
        _originalLocalScale = tr_upper.localScale;

        if (prefab_effect != null)
        {
            _particleEffect = Instantiate(prefab_effect, tr_upper.position, Quaternion.identity, tr_upper);
        }
    }

    private void Start()
    {
        // 최초 1회 실행
        UpdateTextTransform();
        RecalculatePath();
    }

    private void UpdateTextTransform()
    {
        text_current.transform.position = tr_upper.position + Vector3.up * textHeightOffset;
        text_current.transform.rotation = cam_main.transform.rotation;
    }

    private void OnEnable()
    {
        MirrorManager.OnMirrorChanged += RecalculatePath;
    }

    private void OnDisable()
    {
        MirrorManager.OnMirrorChanged -= RecalculatePath;
    }

    public void RecalculatePath()
    {
        _points.Clear();

        Vector3 direction = tr_muzzle.forward.normalized;
        Vector3 origin = tr_muzzle.position + direction * laserOffset;
        _points.Add(origin);

        int mirrorBounces = 0;
        bool isMaxBounce = false;
        ReceiverManager hitReceiver = null;
        for (int i = 0; i < maxMirrorBounces + 1; i++)
        {
            // 충돌 발생
            if (Physics.Raycast(origin, direction, out RaycastHit hit, maxRayDistance, _hitMask))
            {
                _points.Add(hit.point);
                int layer = hit.collider.gameObject.layer;

                // case1. mirror 충돌
                if (layer == _mirrorLayer)
                {
                    mirrorBounces++;
                    if (mirrorBounces > maxMirrorBounces)
                    {
                        Debug.LogWarning($"[Laser] max bounces ({maxMirrorBounces})");
                        isMaxBounce = true;
                        break;
                    }

                    direction = Vector3.Reflect(direction, hit.normal);
                    origin = hit.point + direction * laserOffset;
                    continue;
                }
                // case2. reiver 충돌
                if (layer == _receiverLayer)
                {
                    hitReceiver = hit.collider.GetComponentInParent<ReceiverManager>();
                    hitReceiver?.SetState(ReceiverManager.EState.Success);
                }
                // case3. wall 충돌
                break;
            }
            // 충돌 미발생
            else
            {
                // 해당 경로로 line 쭉 그림
                _points.Add(origin + direction * notHitDistance);
                break;
            }
        }

        if (currentReceiver != null && currentReceiver != hitReceiver)
        {
            currentReceiver.SetState(ReceiverManager.EState.Idle);
        }
        currentReceiver = hitReceiver;
        currentBounceCount = mirrorBounces;

        text_current.text = isMaxBounce ? "Max Bounce Warning!" : $"{mirrorBounces}/{maxMirrorBounces}";

        SetState(isMaxBounce ? EState.MaxBounce : EState.Normal);

        line_laser.positionCount = _points.Count;
        line_laser.SetPositions(_points.ToArray());
    }

    private void SetState(EState newState)
    {
        if (newState == currentState)
        {
            return;
        }

        currentState = newState;
        Debug.Log($"[Laser] state {currentState}");

        if (currentState == EState.MaxBounce)
        {
            PlayMaxBounceEffect();
        }
        else
        {
            StopMaxBounceEffect();
        }
    }

    private void PlayMaxBounceEffect()
    {
        _colorTween?.Kill();
        _emissionTween?.Kill();

        _matUpper.EnableKeyword("_EMISSION");
        _colorTween = _matUpper.DOColor(color_maxBounce, "_BaseColor", colorTweenDuration);
        _emissionTween = DOTween.To(() => _emissionIntensity, SetEmissionIntensity, emissionIntensity_maxBounce, colorTweenDuration);

        TryStartShakeLoop();
        _particleEffect?.Play();
    }

    private void TryStartShakeLoop()
    {
        if (currentState != EState.MaxBounce)
        {
            // 원상복귀
            tr_upper.DOKill();
            tr_upper.localPosition = _originalLocalPosition;
            tr_upper.localScale = _originalLocalScale;
            return;
        }

        tr_upper.DOShakePosition(shakeDuration, shakePositionStrength, shakeVibrato).OnComplete(TryStartShakeLoop);
        tr_upper.DOShakeScale(shakeDuration, shakeScaleStrength, shakeVibrato);
    }

    private void SetEmissionIntensity(float intensity)
    {
        _emissionIntensity = intensity;
        _matUpper.SetColor("_EmissionColor", color_maxBounce * Mathf.Pow(2f, intensity));
    }

    private void StopMaxBounceEffect()
    {
        _colorTween?.Kill();
        _emissionTween?.Kill();

        _matUpper.SetColor("_BaseColor", _originalBaseColor);
        _matUpper.SetColor("_EmissionColor", _originalEmissionColor);
        _matUpper.DisableKeyword("_EMISSION");
        _emissionIntensity = 0f;

        // 현재 진행 중인 사이클 끝내고 종료
        _particleEffect?.StopLoop();
    }
}
