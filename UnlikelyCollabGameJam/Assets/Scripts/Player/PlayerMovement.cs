using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    InputSystem_Actions inputActions;
    Rigidbody2D rigidbody2D;
    [SerializeField]
    float moveForce = 0f;
    [SerializeField]
    float jumpForce = 0f;
    void Awake()
    {
        inputActions = new();
        rigidbody2D = GetComponent<Rigidbody2D>();
        inputActions.Player.Jump.performed += ProcessJump;
    }

    private void ProcessJump(InputAction.CallbackContext context)
    {
        rigidbody2D.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
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
        if (dir != Vector2.zero) rigidbody2D.AddForce(new Vector2(dir.x * moveForce, 0f));
    }
}
