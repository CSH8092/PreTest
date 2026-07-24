using UnityEngine;

public class MirrorController : MonoBehaviour
{
    [SerializeField] private bool editMode;
    public bool EditMode => editMode;

    public void SetEditMode(bool value)
    {
        if (editMode == value) return;

        editMode = value;
        Debug.Log($"[Mirror] {name} editMode {editMode}");
    }
}
