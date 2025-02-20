using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    InputSystem_Actions inputActions;
    Rigidbody2D rb2D;
    [SerializeField]
    float moveForce = 0f;
    [SerializeField]
    float jumpForce = 0f;

    
    private int jumpCount = 1;

    void Awake()
    {
        inputActions = new();
        rb2D = GetComponent<Rigidbody2D>();
        inputActions.Player.Jump.performed += ProcessJump;
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
    }

    private void ProcessJump(InputAction.CallbackContext context)
    {
        rb2D.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
        // CurrentMoveState = MoveStates.Jumping;
    }

    private void GroundDetector() {
        // if (Mathf.Approximately(rb2D.linearVelocityY, 0f)) { CurrentMoveState = MoveStates.Grounded; }
    }
}
