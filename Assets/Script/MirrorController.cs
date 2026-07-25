using UnityEngine;

public class MirrorController : MonoBehaviour
{
    [SerializeField] private bool editMode;
    public bool EditMode => editMode;

    [SerializeField] private Material mat_origin;
    [SerializeField] private Material mat_highlight1;
    [SerializeField] private Material mat_highlight2;

    private Renderer _renderer;

    private void Awake()
    {
        _renderer = GetComponentInChildren<Renderer>();
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

        _renderer.material = MirrorManager.Instance.GizmoMode == MirrorManager.EGizmoMode.XRot
            ? mat_highlight1
            : mat_highlight2;
    }
}
