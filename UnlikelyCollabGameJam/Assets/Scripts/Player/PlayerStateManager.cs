using UnityEngine;

public class PlayerStateManager : MonoBehaviour
{
    public enum MoveState { Jumping, Falling, Grounded, Dashing}
    public enum AttackState {Idle, Attacking, Screaming, Fireball }
    public MoveState CurrentMoveState = MoveState.Grounded;
    public AttackState CurrentAttackState = AttackState.Idle;
    public InputSystem_Actions InputActions {get; private set;}

    public bool IsFacingLeft { get; private set;}

    private void Awake() {
        InputActions = new();
    }
    private void OnEnable() {
        InputActions.Enable();
    }

    private void OnDisable() {
        InputActions.Disable();
    }

    public void UpdateFaceDirection(bool dir) {
        IsFacingLeft = dir;
    }
}
