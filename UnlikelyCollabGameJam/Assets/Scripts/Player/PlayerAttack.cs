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
    int comboCount = 0;


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
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle")) {psm.CurrentAttackState = PlayerStateManager.AttackState.Idle;}
        // if (psm.CurrentAttackState == PlayerStateManager.AttackState.Idle) {
        //     animator.ResetTrigger("Attack2");
        //     animator.ResetTrigger("Attack3");
        //     ResetComboCount(0);
        // }
    }

    private void ProcessAttack(InputAction.CallbackContext context)
    {
        if (psm.CurrentAttackState == PlayerStateManager.AttackState.Screaming) return;
        switch (comboCount) {
            case 0:
                animator.SetTrigger("Attack1");
            break;
            case 1:
                animator.SetTrigger("Attack2");
            break;
            case 2:
                animator.SetTrigger("Attack3");
            break;
            default:
            break;
        }
    }

    public void AttackEnemy(float damageVal) {
        psm.CurrentAttackState = PlayerStateManager.AttackState.Attacking;
        List<Collider2D> OverlapResults = new();
        EnableWeapon();
        if (psm.IsFacingLeft) leftWeaponCollider.Overlap(enemyFilter, OverlapResults);
        else rightWeaponCollider.Overlap(enemyFilter, OverlapResults);
        
        foreach (var enemy in OverlapResults)
        {
            Debug.Log(enemy.name);
            EnemyBehavior eb = enemy.GetComponent<EnemyBehavior>();
            if (comboCount == 1) eb.TakeDamage(combo1damageVal);
            if (comboCount == 2) eb.TakeDamage(combo2damageVal);
            if (comboCount == 3) {
                eb.TakeDamage(combo3damageVal);
            }
        }
        Debug.Log("ComboCount: " + comboCount);
        DisableWeapon();
    }

    public void IncrementComboCount() {
        StopCoroutine(nameof(DelayComboCount));
        comboCount++;
    }

    public void ResetComboCount(float frameDelay){
        StartCoroutine(DelayComboCount(frameDelay));
    }

    private IEnumerator DelayComboCount(float numFrames)
    {
        yield return new WaitForSeconds(numFrames * Time.deltaTime);
        comboCount = 0;
    }

    public void EnableWeapon() {
        if (psm.IsFacingLeft) leftWeaponCollider.gameObject.SetActive(true);
        else rightWeaponCollider.gameObject.SetActive(true);
    }

    public void DisableWeapon() {
        if (psm.IsFacingLeft) leftWeaponCollider.gameObject.SetActive(false);
        else rightWeaponCollider.gameObject.SetActive(false);
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
