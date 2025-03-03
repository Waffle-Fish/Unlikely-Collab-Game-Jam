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
    // [Tooltip("How much to reduce the final velocity of the player after releasing move buttons")]
    // [SerializeField][Range(0f,1f)] float finalXVelocityReduction = 0f;

    [Header("Dash")]
    [Tooltip("How far player goes in a single dash")]
    [SerializeField] float DashDistance = 0f;
    [Tooltip("Time between dashes")]
    [SerializeField] float DashCooldown = 0f;
    [Tooltip("Time it takes to dash")]
    [SerializeField][Min(0.0000001f)] float DashDuration = 1f;
    float timeDashUsed;

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

    [Header("Extra")]
    [SerializeField] private bool StartWithFall = false;
    public event Action<float> OnDashUsed;

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
        animator.SetBool("Fall", StartWithFall);

        timeDashUsed = -DashCooldown;
    }

    void OnDisable()
    {
        // reset animator
        foreach (var p in animator.parameters)
        {
            if (p.type == AnimatorControllerParameterType.Bool) animator.SetBool(p.name, false);
            if (p.type == AnimatorControllerParameterType.Trigger) animator.ResetTrigger(p.name);
        }
        rb2D.linearVelocity = Vector2.zero;
    }

    void Update()
    {
        Move();
        DetectState();
        ProcessFastFalling();
        UpdateAnimation();
        UpdateDashHUD();
        // if(!inputActions.Player.Move.WasPerformedThisFrame() && psm.CurrentMoveState == PlayerStateManager.MoveState.Grounded) rb2D.linearVelocityX = 0;
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
        if (psm.CurrentMoveState == PlayerStateManager.MoveState.Dashing) return;
        Vector2 dir = inputActions.Player.Move.ReadValue<Vector2>();
        animator.SetBool("Run", dir.x != 0f);
        if (dir.x == 0f) {
            rb2D.linearVelocityX = 0;
            return;
        }
        float finalMoveForce = moveForce;
        if (psm.CurrentAttackState != PlayerStateManager.AttackState.Idle) finalMoveForce = moveForce * 0.1f;
        rb2D.linearVelocityX = dir.x * finalMoveForce;
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
        // Base Conditions
        if (psm.CurrentAttackState != PlayerStateManager.AttackState.Idle) return;
        if (Time.time < timeDashUsed + DashCooldown) return;
        Vector2 dir = inputActions.Player.Move.ReadValue<Vector2>();
        if (dir == Vector2.zero) return;
        
        StartCoroutine(Dash(dir));

        IEnumerator Dash(Vector2 dir) {
            Collider2D collider2D = GetComponent<Collider2D>();
            psm.CurrentMoveState = PlayerStateManager.MoveState.Dashing;
            timeDashUsed = Time.time;

            Vector2 goalPos = (Vector2)transform.position + dir * DashDistance;
            Debug.DrawLine(transform.position, goalPos, Color.red, 10f);
            Collider2D hitCollider = Physics2D.OverlapPoint(goalPos);
            if (hitCollider) {
                float newDist = Vector2.Distance(hitCollider.ClosestPoint(goalPos), transform.position);
                goalPos = (Vector2)transform.position + dir * newDist;
                Debug.DrawLine(transform.position, goalPos, Color.blue, 10f);
            }

            Vector2 curVelocity = rb2D.linearVelocity;
            float timer = 0f;
            while (timer < DashDuration) {
                transform.position = Vector2.SmoothDamp(transform.position, goalPos, ref curVelocity, DashDuration);
                timer += Time.deltaTime;
                yield return null;
            }

            psm.CurrentMoveState = PlayerStateManager.MoveState.Grounded;
            
        }
    } 

    private void UpdateAnimation() {
        animator.SetBool("Fall", psm.CurrentMoveState == PlayerStateManager.MoveState.Falling);
        animator.SetBool("Jump", psm.CurrentMoveState == PlayerStateManager.MoveState.Jumping);
        animator.SetBool("Peak", psm.CurrentMoveState == PlayerStateManager.MoveState.Jumping && rb2D.linearVelocityY < 5 && rb2D.linearVelocityY > 0);
    }

    private void UpdateDashHUD()
    {
        float dashPercentDone = (timeDashUsed + DashCooldown - Time.time) / DashCooldown;
        OnDashUsed?.Invoke(dashPercentDone);
    }

}
