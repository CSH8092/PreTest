using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoSingleton<GameManager>
{
    private static readonly List<ReceiverManager> _receivers = new List<ReceiverManager>();

    [Header("UI")]
    [SerializeField] private Button button_howto;
    [SerializeField] private GameObject panel_howto;
    [SerializeField] private TMP_Text text_current_state;

    private static readonly Color ButtonHowtoInactiveColor = Color.white; // #FFFFFF
    private static readonly Color ButtonHowtoActiveColor = new Color(0.6f, 0.6f, 0.6f); // #999999

    protected override void Awake()
    {
        base.Awake();

        panel_howto.SetActive(false);
        button_howto.image.color = ButtonHowtoInactiveColor;
        button_howto.onClick.AddListener(ToggleHowtoPanel);
    }

    private void Update()
    {
        RefreshCurrentStateText();
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
            $"Mirror: {MirrorManager.Instance.SpawnedCount}\n" +
            $"Mode: {MirrorManager.Instance.GizmoMode}";
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
    }

    private void CheckAllSuccess()
    {
        if (!IsAllReceiverSuccess())
        {
            return;
        }

        Debug.Log("all receiver successed");
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
