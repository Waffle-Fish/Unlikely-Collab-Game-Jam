using System;
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

    [Header("Combo Controls")]
    [Tooltip("Time between attacks for it to count as a combo")]
    [SerializeField][Min(0.0000001f)] float comboTime;
    [Tooltip("Short cooldown between attacks. Prevents spam")]
    [SerializeField][Range(0.0000001f, 1f)] float attackCooldown;
    [SerializeField] Collider2D weaponCollider;
    int comboCount;
    float weaponTimer = 0f;
    
    private void Awake() {
        psm = GetComponent<PlayerStateManager>();
        animator = GetComponent<Animator>();
    }

    private void Start() {
        inputActions = psm.InputActions;
        inputActions.Player.Attack.performed += ProcessAttack;
        weaponCollider.gameObject.SetActive(false);
    }

    private void ProcessAttack(InputAction.CallbackContext context)
    {
        // animator.setTrigger("Attacking");
        
    }

    public void AttackEnemy() {
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

    public void EnableWeapon() {
        weaponCollider.gameObject.SetActive(true);
    }

    public void DisableWeapon() {
        weaponCollider.gameObject.SetActive(false);
    }
}
