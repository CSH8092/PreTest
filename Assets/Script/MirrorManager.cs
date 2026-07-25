using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MirrorManager : MonoSingleton<MirrorManager>
{
    public enum EGizmoMode
    {
        XRot,
        YRot
    }

    public static event Action OnMirrorChanged;

    public EGizmoMode GizmoMode => gizmoMode;
    public int SpawnedCount => _spawnedCount;

    [SerializeField] private GameObject obj_mirrorPrefab;
    [SerializeField] private Camera cam_main;
    [SerializeField] private Transform tr_mirrorRoot;

    [Header("Settings")]
    [SerializeField] private int maxMirrors = 30;

    [Header("Gizmo")]
    [SerializeField] private EGizmoMode gizmoMode = EGizmoMode.XRot;
    [SerializeField] private float dragRotateSpeed = 0.2f;
    [SerializeField] private float wheelRotateSpeed = 1f;

    [Header("Debug")]
    [SerializeField] private MirrorController currentSelectedMirror;

    private int _wallMask;
    private int _mirrorMask;
    private int _spawnedCount;
    private bool _isCanMirrorPosition;
    private bool _isCanMirrorRotation;

    protected override void Awake()
    {
        base.Awake();

        if (cam_main == null)
        {
            cam_main = Camera.main;
        }

        _wallMask = 1 << LayerMask.NameToLayer(ConstData.WallLayer);
        _mirrorMask = 1 << LayerMask.NameToLayer(ConstData.MirrorLayer);
    }

    private void Update()
    {
        // Mirror 생성
        if (Keyboard.current.mKey.wasPressedThisFrame)
        {
            CreateMirror();
        }

        // Mirror 모두 제거
        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            AllClearMirror();
        }

        // 선택된 Mirror 제거
        if (Keyboard.current.deleteKey.wasPressedThisFrame)
        {
            DeleteMirror();
        }

        // 회전축 모드 변경
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            ChangeGizmoMode();
        }

        // 선택된 Mirror 회전 초기화
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            ResetSelectedMirrorRotation();
        }

        // 마우스 휠 zrot 이벤트
        float wheel = Mouse.current.scroll.ReadValue().y;
        if (wheel != 0f && currentSelectedMirror != null)
        {
            currentSelectedMirror.transform.Rotate(Vector3.forward, wheel * wheelRotateSpeed, Space.Self);
            EventRefresh();
        }

        // 좌 클릭 이벤트
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            MirrorController hitMirror = EditMirror();

            if (hitMirror != null)
            {
                SetIsCanMirrorPosition(true);
            }
        }

        // 좌 드래그 이벤트
        if (Mouse.current.leftButton.isPressed)
        {
            if (_isCanMirrorPosition)
            {
                SetMirrorPosition();
            }
        }

        // 좌클릭 뗌 이벤트
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            SetIsCanMirrorPosition(false);
        }

        // 우 클릭 이벤트
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (currentSelectedMirror != null)
            {
                SetIsCanMirrorRotation(true);
            }
        }

        // 우 클릭 이벤트
        if (Mouse.current.rightButton.isPressed)
        {
            if (_isCanMirrorRotation)
            {
                SetMirrorRotation();
            }
        }

        // 우 클릭 뗌 이벤트
        if (Mouse.current.rightButton.wasReleasedThisFrame)
        {
            SetIsCanMirrorRotation(false);
        }
    }

    private void SetIsCanMirrorPosition(bool isOn)
    {
        _isCanMirrorPosition = isOn;
    }

    private void SetIsCanMirrorRotation(bool isOn)
    {
        _isCanMirrorRotation = isOn;
    }

    private void ChangeGizmoMode()
    {
        gizmoMode = gizmoMode == EGizmoMode.XRot ? EGizmoMode.YRot : EGizmoMode.XRot;
        Debug.Log($"[Mirror] gizmo mode {gizmoMode}");

        currentSelectedMirror?.RefreshMaterial();

        string mode = gizmoMode == EGizmoMode.XRot ? "X" : "Y";
        string direction = gizmoMode == EGizmoMode.XRot ? "Upside down" : "Left and Right";
        string color = gizmoMode == EGizmoMode.XRot ? ConstData.GizmoModeColorX : ConstData.GizmoModeColorY;
        ToastController.Instance.Open($"Rot Mode <color={color}><b>{mode}</b></color> Changed. Please Mouse Right Drag <color={color}><b>{direction}</b></color>.", gizmoMode);
    }

    private void SetMirrorPosition()
    {
        Ray ray = cam_main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _wallMask))
        {
            return;
        }

        currentSelectedMirror.AlignToWallNormal(hit.normal);
        currentSelectedMirror.transform.position = hit.point;

        EventRefresh();
    }

    private void SetMirrorRotation()
    {
        Vector2 delta = Mouse.current.delta.ReadValue();

        Transform tr = currentSelectedMirror.transform;
        if (gizmoMode == EGizmoMode.XRot)
        {
            tr.Rotate(Vector3.right, delta.y * dragRotateSpeed, Space.Self);
        }
        else
        {
            tr.Rotate(Vector3.up, delta.x * dragRotateSpeed, Space.Self);
        }

        EventRefresh();
    }

    private void CreateMirror()
    {
        // case 1. max count
        if (_spawnedCount >= maxMirrors)
        {
            Debug.LogWarning($"[Mirror] max mirrors reached ({maxMirrors})");
            return;
        }

        Ray ray = cam_main.ScreenPointToRay(Mouse.current.position.ReadValue());
        // case 2. not wall click
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _wallMask))
        {
            return;
        }

        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
        GameObject mirror = Instantiate(obj_mirrorPrefab, hit.point, rotation, tr_mirrorRoot);
        _spawnedCount++;

        currentSelectedMirror?.SetEditMode(false);
        currentSelectedMirror = mirror.GetComponent<MirrorController>();
        currentSelectedMirror.SetEditMode(true);

        Debug.Log("[Mirror] crated " + _spawnedCount);
        EventRefresh();
    }

    private void AllClearMirror()
    {
        for (int i = tr_mirrorRoot.childCount - 1; i >= 0; i--)
        {
            GameObject mirror = tr_mirrorRoot.GetChild(i).gameObject;
            mirror.SetActive(false);
            Destroy(mirror);
        }

        _spawnedCount = 0;
        currentSelectedMirror = null;
        SetIsCanMirrorPosition(false);
        SetIsCanMirrorRotation(false);

        Debug.Log("[Mirror] clear all");
        EventRefresh();
    }

    private void ResetSelectedMirrorRotation()
    {
        if (currentSelectedMirror == null)
        {
            return;
        }

        currentSelectedMirror.ResetRotation();

        Debug.Log("[Mirror] reset rotation");
        EventRefresh();
    }

    private void DeleteMirror()
    {
        if (currentSelectedMirror == null)
        {
            return;
        }

        currentSelectedMirror.gameObject.SetActive(false);
        Destroy(currentSelectedMirror.gameObject);
        currentSelectedMirror = null;
        _spawnedCount--;
        SetIsCanMirrorPosition(false);
        SetIsCanMirrorRotation(false);

        Debug.Log("[Mirror] delete");
        EventRefresh();
    }

    public static void EventRefresh()
    {
        Debug.Log("[Mirror] Event Refresh");
        OnMirrorChanged?.Invoke();
    }

    private MirrorController EditMirror()
    {
        Ray ray = cam_main.ScreenPointToRay(Mouse.current.position.ReadValue());
        MirrorController hitMirror = null;

        // Mirror Click
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _mirrorMask))
        {
            hitMirror = hit.collider.GetComponentInParent<MirrorController>();
        }

        // 이미 선택되어 있는 Mirror라면 pass
        if (hitMirror == currentSelectedMirror)
        {
            return hitMirror;
        }

        // 이미 선택되어있던 Mirror의 Edit Mode Off
        if (currentSelectedMirror != null)
        {
            currentSelectedMirror.SetEditMode(false);
        }

        // 선택한 Mirror의 Edit Mode On
        hitMirror?.SetEditMode(true);
        currentSelectedMirror = hitMirror;

        return hitMirror;
    }
}
