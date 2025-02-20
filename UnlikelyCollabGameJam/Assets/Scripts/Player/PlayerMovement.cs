using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerStateManager))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float moveForce = 0f;
    [SerializeField] float jumpForce = 0f;
    [SerializeField] float fallForce = 0f;

    InputSystem_Actions inputActions;
    Rigidbody2D rb2D;
    PlayerStateManager playerStateManager;   
    SpriteRenderer spriteRenderer;
    float originalGravScale = 1;

    void Awake()
    {
        inputActions = new();
        rb2D = GetComponent<Rigidbody2D>();
        inputActions.Player.Jump.performed += ProcessJump;
        playerStateManager = GetComponent<PlayerStateManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start() {
        originalGravScale = rb2D.gravityScale;
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
        Move();
        StateDetector();
        ProcessFastFalling();
    }

    private void Move()
    {
        Vector2 dir = inputActions.Player.Move.ReadValue<Vector2>();
        if (dir.x == 0f) return;
        rb2D.AddForce(new Vector2(dir.x * moveForce, 0f));
        spriteRenderer.flipX = dir.x < 0f;
    }

    private void ProcessJump(InputAction.CallbackContext context)
    {
        if (playerStateManager.CurrentState != PlayerStateManager.State.Grounded) return;
        rb2D.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
        playerStateManager.CurrentState = PlayerStateManager.State.Jumping;
    }

    private void ProcessFastFalling() {
        if(inputActions.Player.FastFall.WasPerformedThisFrame()) {
            Debug.Log("Falling fast");
            if (playerStateManager.CurrentState == PlayerStateManager.State.Falling || playerStateManager.CurrentState == PlayerStateManager.State.Jumping) {
                // rb2D.linearVelocity = new(rb2D.linearVelocityX, 0f);
                rb2D.gravityScale = fallForce;
            }
        } else if (inputActions.Player.FastFall.WasReleasedThisFrame() || playerStateManager.CurrentState == PlayerStateManager.State.Grounded) {
            rb2D.linearVelocity = new(rb2D.linearVelocityX, 0f);
            rb2D.gravityScale = originalGravScale;
        }
    }

    private void StateDetector() {
        if (Mathf.Approximately(rb2D.linearVelocityY, 0f)) playerStateManager.CurrentState = PlayerStateManager.State.Grounded;
        if (rb2D.linearVelocityY < 0 && !Mathf.Approximately(rb2D.linearVelocityY, 0f)) playerStateManager.CurrentState = PlayerStateManager.State.Falling;
    }
}
