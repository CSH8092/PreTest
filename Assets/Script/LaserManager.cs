using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LaserManager : MonoBehaviour
{
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

    private int _wallLayer;
    private int _mirrorLayer;
    private int _receiverLayer;
    private int _hitMask;

    private readonly List<Vector3> _points = new List<Vector3>();

    [Header("Debug")]
    [SerializeField] private ReceiverManager currentReceiver;
    [SerializeField] private int currentBounceCount;
    public int CurrentBounceCount => currentBounceCount;

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

        line_laser.positionCount = _points.Count;
        line_laser.SetPositions(_points.ToArray());
    }
}
