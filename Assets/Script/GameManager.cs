using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class GameManager : MonoSingleton<GameManager>
{
    private static readonly List<ReceiverManager> _receivers = new List<ReceiverManager>();

    [Header("UI")]
    [SerializeField] private Button button_howto;
    [SerializeField] private GameObject panel_howto;
    [SerializeField] private TMP_Text text_current_state;

    [Header("Vignette : 모든 Receiver가 Success상태일 때")]
    [SerializeField] private Volume volume_global;
    [SerializeField] private float vignetteIntensity_success = 0.6f;
    [SerializeField] private float vignetteIntensity_default = 0.2f;
    [SerializeField] private float vignetteTweenDuration = 0.5f;

    private static readonly Color ButtonHowtoInactiveColor = Color.white; // #FFFFFF
    private static readonly Color ButtonHowtoActiveColor = new Color(0.6f, 0.6f, 0.6f); // #999999

    private Vignette _vignette;
    private Tween _vignetteTween;

    protected override void Awake()
    {
        base.Awake();

        panel_howto.SetActive(false);
        button_howto.image.color = ButtonHowtoInactiveColor;
        button_howto.onClick.AddListener(ToggleHowtoPanel);

        if (volume_global == null)
        {
            volume_global = FindFirstObjectByType<Volume>();
        }

        if (volume_global == null)
        {
            Debug.LogWarning("[GameManager] Volume을 찾을 수 없음");
            return;
        }

        if (!volume_global.profile.TryGet(out _vignette))
        {
            Debug.LogWarning("[GameManager] Volume Profile에 Vignette가 없음");
        }
    }

    private void Start()
    {
        RefreshCurrentStateText();
    }

    private void OnEnable()
    {
        MirrorManager.OnMirrorChanged += RefreshCurrentStateText;
    }

    private void OnDisable()
    {
        MirrorManager.OnMirrorChanged -= RefreshCurrentStateText;
    }

    private void ToggleHowtoPanel()
    {
        bool isActive = !panel_howto.activeSelf;
        panel_howto.SetActive(isActive);
        button_howto.image.color = isActive ? ButtonHowtoActiveColor : ButtonHowtoInactiveColor;
    }

    private void RefreshCurrentStateText()
    {
        text_current_state.text =
            $"Current Mirror Spawn Count: {MirrorManager.Instance.SpawnedCount}/{MirrorManager.Instance.MaxMirrors}\n" +
            $"Current Mirror Rotation Mode: {MirrorManager.Instance.GizmoMode}\n" +
            $"Mirror Selected: {MirrorManager.Instance.HasSelection}\n" +
            $"Receiver Success: {GetReceiverSuccessCount()}/{_receivers.Count}\n" +
            $"Is All Receiver Success : {IsAllReceiverSuccess()}";
    }

    private int GetReceiverSuccessCount()
    {
        int count = 0;
        foreach (ReceiverManager receiver in _receivers)
        {
            if (receiver.CurrentState == ReceiverManager.EState.Success)
            {
                count++;
            }
        }

        return count;
    }

    public static void RegisterReceiver(ReceiverManager receiver)
    {
        _receivers.Add(receiver);
    }

    public static void UnregisterReceiver(ReceiverManager receiver)
    {
        _receivers.Remove(receiver);
    }

    public static void EventRefresh()
    {
        Instance?.CheckAllSuccess();
        Instance?.RefreshCurrentStateText();
    }

    private void CheckAllSuccess()
    {
        bool isAllSuccess = IsAllReceiverSuccess();

        if (isAllSuccess)
        {
            Debug.Log("all receiver successed");
        }

        SetVignetteIntensity(isAllSuccess ? vignetteIntensity_success : vignetteIntensity_default);
    }

    private void SetVignetteIntensity(float target)
    {
        if (_vignette == null)
        {
            return;
        }

        _vignetteTween?.Kill();
        _vignetteTween = DOTween.To(() => _vignette.intensity.value, value => _vignette.intensity.value = value, target, vignetteTweenDuration)  .SetEase(Ease.OutBack);
    }

    private bool IsAllReceiverSuccess()
    {
        if (_receivers.Count == 0)
        {
            return false;
        }

        foreach (ReceiverManager receiver in _receivers)
        {
            if (receiver.CurrentState != ReceiverManager.EState.Success)
            {
                return false;
            }
        }

        return true;
    }
}
