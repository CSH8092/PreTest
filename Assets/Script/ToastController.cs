using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToastController : MonoSingleton<ToastController>
{
    [SerializeField] private RectTransform rt_toast;
    [SerializeField] private TMP_Text text_message;
    [SerializeField] private Image image_icon;
    [SerializeField] private Sprite icon_x;
    [SerializeField] private Sprite icon_y;

    [Header("Settings")]
    [SerializeField] private float hiddenY = -117f;
    [SerializeField] private float shownY = 0f;
    [SerializeField] private float showDuration = 0.3f;
    [SerializeField] private float hideDuration = 0.3f;
    [SerializeField] private float holdDuration = 1f;

    private Sequence _sequence;

    protected override void Awake()
    {
        base.Awake();

        Vector2 pos = rt_toast.anchoredPosition;
        pos.y = hiddenY;
        rt_toast.anchoredPosition = pos;
    }

    public void Open(string message, MirrorManager.EGizmoMode mode)
    {
        _sequence?.Kill();

        text_message.text = message;
        image_icon.sprite = mode == MirrorManager.EGizmoMode.XRot ? icon_x : icon_y;

        _sequence = DOTween.Sequence()
            .Append(rt_toast.DOAnchorPosY(shownY, showDuration))
            .AppendInterval(holdDuration)
            .Append(rt_toast.DOAnchorPosY(hiddenY, hideDuration));
    }
}
