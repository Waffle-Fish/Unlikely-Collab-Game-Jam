using System;
using System.Collections;
using System.Threading;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerStateManager))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Horizontal Movement")]
    [SerializeField] float moveForce = 0f;
    [Tooltip("How much to reduce the final velocity of the player after releasing move buttons")]
    [SerializeField][Range(0f,1f)] float finalXVelocityReduction = 0f;

    [Header("Dash")]
    [Tooltip("How far player goes in a single dash")]
    [SerializeField] float DashDistance = 0f;
    [Tooltip("Time between dashes")]
    [SerializeField] float DashCooldown = 0f;
    [Tooltip("Time it takes to dash")]
    [SerializeField][Min(0.0000001f)] float DashDuration = 1f;
    float timeDashUsed = 0;

    [Header("Vertical Movement")]
    [SerializeField] float jumpForce = 0f;
    [SerializeField] float fallForce = 0f;
    float originalGravScale = 1;

    [Header("Components")]
    InputSystem_Actions inputActions;
    Rigidbody2D rb2D;
    PlayerStateManager psm;   
    SpriteRenderer spriteRenderer;
    Animator animator;

    void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
        psm = GetComponent<PlayerStateManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    private void Start() {
        inputActions = psm.InputActions;
        inputActions.Player.Jump.performed += ProcessJump;
        inputActions.Player.Dash.performed += ProcessDash;
        originalGravScale = rb2D.gravityScale;
    }

    void Update()
    {
        Move();
        DetectState();
        ProcessFastFalling();
        UpdateAnimation();
        if(!inputActions.Player.Move.WasPerformedThisFrame() && psm.CurrentMoveState == PlayerStateManager.MoveState.Grounded) rb2D.linearVelocityX *= finalXVelocityReduction;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.rigidbody) {
            collision.rigidbody.AddForce(rb2D.linearVelocity);
            collision.rigidbody.AddForceY(0.2f);
        }
    }

    private void Move()
    {
        if (psm.CurrentMoveState == PlayerStateManager.MoveState.Dashing || psm.CurrentAttackState == PlayerStateManager.AttackState.Attacking) return;
        Vector2 dir = inputActions.Player.Move.ReadValue<Vector2>();
        animator.SetBool("Run", dir.x != 0f);
        if (dir.x == 0f) return;
        animator.ResetTrigger("Attack");
        // Force Movement
        // rb2D.AddForce(new Vector2(dir.x * moveForce, 0f));

        // Velocity Movement
        rb2D.linearVelocityX = dir.x * moveForce;
        spriteRenderer.flipX = dir.x < 0f;
        psm.UpdateFaceDirection(dir.x < 0f);
    }

    private void DetectState() {
        if (Mathf.Approximately(rb2D.linearVelocityY, 0f)) {
            rb2D.gravityScale = originalGravScale;
            psm.CurrentMoveState = PlayerStateManager.MoveState.Grounded;
        }
        if (rb2D.linearVelocityY < -0.1f) {
            psm.CurrentMoveState = PlayerStateManager.MoveState.Falling;
        }
    }

    private void ProcessFastFalling() {
        
        if(inputActions.Player.FastFall.WasPerformedThisFrame()) {
            if (psm.CurrentMoveState == PlayerStateManager.MoveState.Falling || psm.CurrentMoveState == PlayerStateManager.MoveState.Jumping) {
                // rb2D.linearVelocity = new(rb2D.linearVelocityX, 0f);
                rb2D.gravityScale = fallForce;
            }
        } 
        // else if (inputActions.Player.FastFall.WasReleasedThisFrame() || psm.CurrentState == PlayerStateManager.State.Grounded) {
        //     rb2D.linearVelocity = new(rb2D.linearVelocityX, 0f);
        //     rb2D.gravityScale = originalGravScale;
        // }
    }

    private void ProcessJump(InputAction.CallbackContext context)
    {
        if (psm.CurrentMoveState != PlayerStateManager.MoveState.Grounded) return;
        rb2D.AddForceY(jumpForce, ForceMode2D.Impulse);
        psm.CurrentMoveState = PlayerStateManager.MoveState.Jumping;
    }

    private void ProcessDash(InputAction.CallbackContext context)
    {
        // Horizontal Dash
        // IEnumerator Dash(float xDir) {
        //     // Vector3 goalPos = new(transform.position.x + (xDir * DashDistance), transform.position.y);
        //     Vector2 curVelocity = rb2D.linearVelocity;
        //     float timer = 0f;
        //     while (timer < DashDuration) {
        //         transform.position = Vector2.SmoothDamp(transform.position, goalPos, ref curVelocity, DashDuration);
        //         timer += Time.deltaTime;
        //         yield return null;
        //     }
        //     psm.CurrentState = PlayerStateManager.State.Grounded;
        // }
        // if (Time.time < timeDashUsed + DashCooldown) return;
        // float xDir = inputActions.Player.Move.ReadValue<Vector2>().x;
        // if (Mathf.Approximately(xDir, 0)) return;

        // psm.CurrentState = PlayerStateManager.State.Dashing;
        // timeDashUsed = Time.time;
        // StartCoroutine(Dash(xDir));

        // Directional Dash
        IEnumerator Dash(Vector2 dir) {
            Vector2 goalPos = (Vector2)transform.position + dir * DashDistance;
            Vector2 curVelocity = rb2D.linearVelocity;
            float timer = 0f;
            while (timer < DashDuration) {
                transform.position = Vector2.SmoothDamp(transform.position, goalPos, ref curVelocity, DashDuration);
                timer += Time.deltaTime;
                yield return null;
            }
            psm.CurrentMoveState = PlayerStateManager.MoveState.Grounded;
        }

        if (Time.time < timeDashUsed + DashCooldown) return;
        Vector2 dir = inputActions.Player.Move.ReadValue<Vector2>();
        if (dir == Vector2.zero) return;

        psm.CurrentMoveState = PlayerStateManager.MoveState.Dashing;
        timeDashUsed = Time.time;
        StartCoroutine(Dash(dir));
    } 

    private void UpdateAnimation() {
        animator.SetBool("Fall", psm.CurrentMoveState == PlayerStateManager.MoveState.Falling);
        animator.SetBool("Jump", psm.CurrentMoveState == PlayerStateManager.MoveState.Jumping);
        animator.SetBool("Peak", psm.CurrentMoveState == PlayerStateManager.MoveState.Jumping && rb2D.linearVelocityY < 5 && rb2D.linearVelocityY > 0);
    }
}
