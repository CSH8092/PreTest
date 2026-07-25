using UnityEngine;

public class MirrorController : MonoBehaviour
{
    [SerializeField] private bool editMode;
    public bool EditMode => editMode;

    [SerializeField] private Material mat_origin;
    [SerializeField] private Material mat_highlight1;
    [SerializeField] private Material mat_highlight2;

    private Renderer _renderer;
    private Vector3? _lastWallNormal;
    private Quaternion _initialRotation;

    private void Awake()
    {
        _renderer = GetComponentInChildren<Renderer>();
        _initialRotation = transform.rotation;
    }

    public void AlignToWallNormal(Vector3 normal)
    {
        if (_lastWallNormal != null && Vector3.Angle(_lastWallNormal.Value, normal) <= 0.01f)
        {
            return;
        }

        // 다른 벽으로 이동될 때, normal 재정렬
        Quaternion normalDelta = Quaternion.FromToRotation(transform.up, normal);
        transform.rotation = normalDelta * transform.rotation;
        _lastWallNormal = normal;
    }

    public void ResetRotation()
    {
        // 한 번도 안 옮겼으면 생성 시점 벽 기준, 옮긴 적 있으면 현재 붙어있는 벽 기준으로 초기화
        transform.rotation = _lastWallNormal != null ? Quaternion.FromToRotation(Vector3.up, _lastWallNormal.Value) : _initialRotation;
    }

    public void SetEditMode(bool value)
    {
        if (editMode == value)
        {
            return;
        }

        editMode = value;
        Debug.Log($"[Mirror] {name} editMode {editMode}");

        RefreshMaterial();
    }

    public void RefreshMaterial()
    {
        if (!editMode)
        {
            _renderer.material = mat_origin;
            return;
        }

        _renderer.material = MirrorManager.Instance.GizmoMode == MirrorManager.EGizmoMode.Pitch ? mat_highlight1 : mat_highlight2;
    }
}
