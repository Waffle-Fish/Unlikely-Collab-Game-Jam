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
    [SerializeField] Collider2D weaponCollider;
    int comboCount = 0;
    float weaponTimer = 0f;


    [Header("Special Attack")]
    [SerializeField] Collider2D screamCollider;
    [SerializeField] float screamDuration;
    [Tooltip("Damage dealt per second")]
    [SerializeField] float screamDamage;

    private void Awake() {
        psm = GetComponent<PlayerStateManager>();
        animator = GetComponent<Animator>();
    }

    private void Start() {
        weaponCollider.gameObject.SetActive(false);
        screamCollider.gameObject.SetActive(false);

        inputActions = psm.InputActions;
        inputActions.Player.Attack.performed += ProcessAttack;
        inputActions.Player.Special.performed += ProcessSpecialAttack;
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

    public void AttackEnemy() {
        psm.CurrentAttackState = PlayerStateManager.AttackState.Attacking;
        List<Collider2D> OverlapResults = new();
        EnableWeapon();
        weaponCollider.Overlap(OverlapResults);
        foreach (var enemy in OverlapResults)
        {
            if (enemy.CompareTag("Enemy")) OverlapResults.Remove(enemy);
        }

        foreach (var enemy in OverlapResults)
        {
            Debug.Log(enemy.name);
        }
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
        weaponCollider.gameObject.SetActive(true);
    }

    public void DisableWeapon() {
        weaponCollider.gameObject.SetActive(false);
    }

    private void ProcessSpecialAttack(InputAction.CallbackContext context)
    {
        IEnumerator Scream() {
            List<Collider2D> EnemiesInRange = new();
            float finalTime = Time.time + screamDuration;
            while (Time.time < finalTime) {
                ContactFilter2D screamFilter = new();
                screamFilter.SetLayerMask(LayerMask.GetMask("Enemy"));
                screamCollider.Overlap(screamFilter, EnemiesInRange);
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
