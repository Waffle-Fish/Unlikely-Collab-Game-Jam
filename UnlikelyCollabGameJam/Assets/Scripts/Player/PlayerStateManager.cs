using UnityEngine;

public class PlayerStateManager : MonoBehaviour
{
    public enum MoveState { Jumping, Falling, Grounded, Dashing}
    public enum AttackState {Idle, Attacking, Screaming }
    public MoveState CurrentMoveState = MoveState.Grounded;
    public AttackState CurrentAttackState = AttackState.Idle;
    public InputSystem_Actions InputActions {get; private set;}

    private void Awake() {
        InputActions = new();
    }
    private void OnEnable() {
        InputActions.Enable();
    }

    private void OnDisable() {
        InputActions.Disable();
    }
}
