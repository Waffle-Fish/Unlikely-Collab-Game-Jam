using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerStateManager))]
public class PlayerAttack : MonoBehaviour
{
    [Header("Components")]
    PlayerStateManager psm;
    InputSystem_Actions inputActions;
    Animator animator;

    [Header("Normal Attack")]
    [Tooltip("Time between attacks for it to count as a combo")]
    [SerializeField][Min(0.0000001f)] float comboTime;
    [Tooltip("Short cooldown between attacks. Prevents spam")]
    [SerializeField][Range(0.0000001f, 1f)] float attackCooldown;
    [SerializeField] Collider2D leftWeaponCollider;
    [SerializeField] Collider2D rightWeaponCollider;
    [SerializeField] float combo1damageVal;
    [SerializeField] float combo2damageVal;
    [SerializeField] float combo3damageVal;
    [Tooltip("Number of frames between attacks that will count as a combo")]
    [SerializeField] int comboWindow = 0;

    [Header("Scream Attack")]
    [SerializeField] Collider2D screamCollider;
    [SerializeField][Min(0f)] float screamDamage;
    [SerializeField][Min(0f)] float screamCooldown;
    float screamTimer;

    [Header("Fireball")]
    [SerializeField] GameObject fireBallPrefab;
    [SerializeField] float fireBallForce;
    [SerializeField] Transform rightSpawnLocation;
    [SerializeField] Transform leftSpawnLocation;
    [SerializeField][Min(0f)] float fireBallCooldown;
    float fireBallTimer = 0;
    List<GameObject> fireBalls;

    ContactFilter2D enemyFilter;
    Rigidbody2D rb2d;

    private void Awake() {
        psm = GetComponent<PlayerStateManager>();
        animator = GetComponent<Animator>();
        rb2d = GetComponent<Rigidbody2D>();
        
        fireBalls = new();
        for (int i = 0; i < 11; i++)
        {
            fireBalls.Add(Instantiate(fireBallPrefab, transform.position, quaternion.identity));
            fireBalls[i].SetActive(false);
        }
    }

    private void Update() {
        if (psm.CurrentMoveState != PlayerStateManager.MoveState.Grounded) {
            animator.ResetTrigger("Fireball");
            animator.ResetTrigger("Scream");
        }
    }

    private void Start() {
        leftWeaponCollider.gameObject.SetActive(false);
        rightWeaponCollider.gameObject.SetActive(false);
        screamCollider.gameObject.SetActive(false);

        inputActions = psm.InputActions;
        inputActions.Player.Attack.performed += ProcessAttack;
        inputActions.Player.Scream.performed += ProcessScreamAttack;
        inputActions.Player.Fireball.performed += StartFireballAttack;

        enemyFilter.SetLayerMask(LayerMask.GetMask("Enemy"));
    }

    private void ProcessAttack(InputAction.CallbackContext context)
    {
        if (psm.CurrentAttackState != PlayerStateManager.AttackState.Idle && psm.CurrentAttackState != PlayerStateManager.AttackState.Attacking) return;
        if (psm.CurrentMoveState != PlayerStateManager.MoveState.Grounded) {
            animator.ResetTrigger("Attack");
            return;
        }
        animator.SetTrigger("Attack");
    }

    public void AttackEnemy(int comboNum) {
        psm.CurrentAttackState = PlayerStateManager.AttackState.Attacking;
        List<Collider2D> OverlapResults = new();
        EnableWeapon();
        if (psm.IsFacingLeft) leftWeaponCollider.Overlap(enemyFilter, OverlapResults);
        else rightWeaponCollider.Overlap(enemyFilter, OverlapResults);
        foreach (var enemy in OverlapResults)
        {
            Debug.Log(enemy.name);
            EnemyBehavior eb = enemy.GetComponent<EnemyBehavior>();
            if (comboNum == 1) eb.TakeDamage(combo1damageVal);
            if (comboNum == 2) eb.TakeDamage(combo2damageVal);
            if (comboNum == 3) eb.TakeDamage(combo3damageVal);
        }
        DisableWeapon();

        // comboTimeLimit = Time.time + comboWindow * Time.deltaTime;
    }

    public void EnableWeapon() {
        if (psm.IsFacingLeft) leftWeaponCollider.gameObject.SetActive(true);
        else rightWeaponCollider.gameObject.SetActive(true);
    }

    public void DisableWeapon() {
        if (psm.IsFacingLeft) leftWeaponCollider.gameObject.SetActive(false);
        else rightWeaponCollider.gameObject.SetActive(false);
    }

    public void SetAttackStateToIdle() {
        psm.CurrentAttackState = PlayerStateManager.AttackState.Idle;
    }

    private void ProcessScreamAttack(InputAction.CallbackContext context)
    {
        if (Time.time < screamTimer) return;
        psm.CurrentAttackState = PlayerStateManager.AttackState.Screaming;

        animator.SetTrigger("Scream");
        screamCollider.gameObject.SetActive(true);
        List<Collider2D> EnemiesInRange = new();
        screamCollider.Overlap(enemyFilter, EnemiesInRange);
        foreach (var enemy in EnemiesInRange) {
            enemy.GetComponent<EnemyBehavior>().TakeDamage(screamDamage);
        }

        screamCollider.gameObject.SetActive(false);
        psm.CurrentAttackState = PlayerStateManager.AttackState.Idle;
        screamTimer = Time.time + screamCooldown;
    }

    private void StartFireballAttack(InputAction.CallbackContext context)
    {
        if (Time.time < fireBallTimer) return;
        fireBallTimer = Time.time + fireBallCooldown;
        
        animator.SetTrigger("Fireball");
    }

    public void ProcessFireballAttack(){
        GameObject fireBallObj = null;
        foreach (var fb in fireBalls) {
            if (fb.activeSelf) continue;
            fireBallObj = fb;
            break;
        }
        if (!fireBallObj) return;
        psm.CurrentAttackState = PlayerStateManager.AttackState.Fireball;

        fireBallObj.transform.position = (psm.IsFacingLeft) ? leftSpawnLocation.position : rightSpawnLocation.position; 
        fireBallObj.SetActive(true);
        Rigidbody2D fireballRb2D = fireBallObj.GetComponent<Rigidbody2D>();
        fireballRb2D.linearVelocity = Vector2.zero;

        Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
        mouseScreenPos.z = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        Vector2 dir = (mouseWorldPosition - transform.position);
        float zRot = -Vector2.SignedAngle(dir, Vector2.right);

        dir = dir.normalized;
        fireBallObj.transform.rotation = Quaternion.Euler(0,0,zRot);
        fireballRb2D.AddForce(fireBallForce * dir);
    }


}
