using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MirrorManager : MonoSingleton<MirrorManager>
{
    public static event Action OnMirrorChanged;

    [SerializeField] private GameObject obj_mirrorPrefab;
    [SerializeField] private Camera cam_main;
    [SerializeField] private Transform tr_mirrorRoot;

    [Header("Settings")]
    [SerializeField] private int maxMirrors = 30;

    private int _wallMask;
    private int _mirrorMask;
    private int _spawnedCount;
    private MirrorController _selectedMirror;

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
        if (Keyboard.current.mKey.wasPressedThisFrame)
        {
            CreateMirror();
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            EditMirror();
        }

        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            AllClearMirror();
        }
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
        Instantiate(obj_mirrorPrefab, hit.point, rotation, tr_mirrorRoot);
        _spawnedCount++;

        Debug.Log("[Mirror] crated " + _spawnedCount);
        EventRefresh();
    }

    private void AllClearMirror()
    {
        for (int i = tr_mirrorRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(tr_mirrorRoot.GetChild(i).gameObject);
        }

        _spawnedCount = 0;
        _selectedMirror = null;

        Debug.Log("[Mirror] clear all");
        EventRefresh();
    }

    public static void EventRefresh()
    {
        Debug.Log("[Mirror] Event Refresh");
        OnMirrorChanged?.Invoke();
    }

    private void EditMirror()
    {
        Ray ray = cam_main.ScreenPointToRay(Mouse.current.position.ReadValue());
        MirrorController hitMirror = null;

        // Mirror Click
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _mirrorMask))
        {
            hitMirror = hit.collider.GetComponentInParent<MirrorController>();
        }

        // 이미 선택되어 있는 Mirror라면 pass
        if (hitMirror == _selectedMirror) return;

        // 이미 선택되어있던 Mirror의 Edit Mode Off
        if (_selectedMirror != null)
        {
            _selectedMirror.SetEditMode(false);
        }

        // 선택한 Mirror의 Edit Mode On
        hitMirror?.SetEditMode(true);
        _selectedMirror = hitMirror;
    }
}
