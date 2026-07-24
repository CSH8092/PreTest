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

    public void SetState(EState newState)
    {
        if (newState == currentState) return;

        currentState = newState;
        Debug.Log($"[Receiver] {name} set {currentState}");

        // todo : 시각 연출
    }
}
