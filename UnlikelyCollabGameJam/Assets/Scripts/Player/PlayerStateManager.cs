using UnityEngine;

public class PlayerStateManager : MonoBehaviour
{
    public enum State { Jumping, Falling, Grounded, Dashing, Attacking }
    public State CurrentState = State.Grounded;
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
