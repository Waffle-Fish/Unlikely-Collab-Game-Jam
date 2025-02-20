using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerStateManager))]
public class PlayerMovement : MonoBehaviour
{
    InputSystem_Actions inputActions;
    Rigidbody2D rb2D;
    [SerializeField]
    float moveForce = 0f;
    [SerializeField]
    float jumpForce = 0f;
    PlayerStateManager playerStateManager;
    
    private int jumpCount = 1;

    void Awake()
    {
        inputActions = new();
        rb2D = GetComponent<Rigidbody2D>();
        inputActions.Player.Jump.performed += ProcessJump;
        playerStateManager = GetComponent<PlayerStateManager>();
    }

    private void Start() {
        
    }

    private void OnEnable() {
        inputActions.Enable();
    }

    private void OnDisable() {
        inputActions.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 dir = inputActions.Player.Move.ReadValue<Vector2>();
        if (dir.x != 0f) rb2D.AddForce(new Vector2(dir.x * moveForce, 0f));
        GroundDetector();
        FallDetector();
    }

    private void ProcessJump(InputAction.CallbackContext context)
    {
        if (playerStateManager.CurrentState != PlayerStateManager.State.Grounded) return;
        rb2D.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
        playerStateManager.CurrentState = PlayerStateManager.State.Jumping;
    }

    private void GroundDetector() {
        if (Mathf.Approximately(rb2D.linearVelocityY, 0f)) playerStateManager.CurrentState = PlayerStateManager.State.Grounded;
    }

    private void FallDetector() {
        if (rb2D.linearVelocityY < 0 && !Mathf.Approximately(rb2D.linearVelocityY, 0f)) playerStateManager.CurrentState = PlayerStateManager.State.Falling;
    }
}
