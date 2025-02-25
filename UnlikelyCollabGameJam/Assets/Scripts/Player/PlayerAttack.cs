using System;
using System.Collections;
using System.Collections.Generic;
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
    float comboTimeLimit;


    [Header("Special Attack")]
    [SerializeField] Collider2D screamCollider;
    [SerializeField] float screamDuration;
    [Tooltip("Damage dealt per second")]
    [SerializeField] float screamDamage;

    ContactFilter2D enemyFilter;

    private void Awake() {
        psm = GetComponent<PlayerStateManager>();
        animator = GetComponent<Animator>();
    }

    private void Start() {
        leftWeaponCollider.gameObject.SetActive(false);
        rightWeaponCollider.gameObject.SetActive(false);
        screamCollider.gameObject.SetActive(false);

        inputActions = psm.InputActions;
        inputActions.Player.Attack.performed += ProcessAttack;
        inputActions.Player.Special.performed += ProcessSpecialAttack;

        enemyFilter.SetLayerMask(LayerMask.GetMask("Enemy"));
    }

    private void Update()
    {

    }

    private void ProcessAttack(InputAction.CallbackContext context)
    {
        if (psm.CurrentAttackState == PlayerStateManager.AttackState.Screaming) return;
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

    private void ProcessSpecialAttack(InputAction.CallbackContext context)
    {
        IEnumerator Scream() {
            List<Collider2D> EnemiesInRange = new();
            float finalTime = Time.time + screamDuration;
            while (Time.time < finalTime) {
                
                screamCollider.Overlap(enemyFilter, EnemiesInRange);
                foreach (var enemy in EnemiesInRange)
                {
                    Debug.Log("Screamed at: " + enemy.name);
                    enemy.GetComponent<EnemyBehavior>().TakeDamage(screamDamage * Time.deltaTime);
                }
                yield return null;
            }
            screamCollider.gameObject.SetActive(false);
            psm.CurrentAttackState = PlayerStateManager.AttackState.Idle;
        }

        psm.CurrentAttackState = PlayerStateManager.AttackState.Screaming;
        screamCollider.gameObject.SetActive(true);
        Debug.Log("Screaming");
        StartCoroutine(Scream());
    }
}
