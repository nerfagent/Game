// Assets/Scripts/Level/Player/PlayerState.cs
using UnityEngine;
using UnityEngine.Events;

public class PlayerState : MonoBehaviour
{
    public enum State { Idle, Moving, Dashing, CastingSkill, Stunned, Dead }

    [SerializeField] private State currentState = State.Idle;
    public State CurrentState => currentState;

    public bool IsMoving => currentState == State.Moving;
    public bool IsDashing => currentState == State.Dashing;
    public bool IsCasting => currentState == State.CastingSkill;
    public bool IsStunned => currentState == State.Stunned;
    public bool IsDead => currentState == State.Dead;
    public bool IsActionLocked => IsDashing || IsCasting || IsStunned || IsDead;

    public static UnityAction<State> onPlayerStateChanged = s => { };

    public void SetState(State newState)
    {
        if (currentState == newState) return;

        currentState = newState;
        onPlayerStateChanged(newState);
    }

    public void ResetToIdle()
    {
        SetState(State.Idle);
    }
}
