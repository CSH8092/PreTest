using UnityEngine;

public class ReceiverManager : MonoBehaviour
{
    public enum EState
    {
        Idle,
        Success
    }

    [SerializeField] private EState currentState = EState.Idle;
    public EState CurrentState => currentState;

    [SerializeField] private HighlightEffect effect;

    private void Awake()
    {
        GameManager.RegisterReceiver(this);
    }

    private void OnDestroy()
    {
        GameManager.UnregisterReceiver(this);
    }

    public void SetState(EState newState)
    {
        if (newState == currentState)
        {
            return;
        }

        currentState = newState;
        Debug.Log($"[Receiver] {name} set {currentState}");

        if (currentState == EState.Success)
        {
            effect.Play();
        }
        else
        {
            effect.Stop();
        }

        GameManager.EventRefresh();
    }
}
