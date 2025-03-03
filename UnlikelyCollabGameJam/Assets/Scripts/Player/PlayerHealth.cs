using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField]
    private float maxHealth = 100f;
    public float CurrentHealth { get; private set; }

    [Header("For Testing Purposes")]
    public bool ClickToHeal = false;
    public bool ClickToDamage = false;
    public event Action<float> OnHealthChanged;
    public event Action OnPlayerDeath;

    Animator animator;
    PlayerStateManager psm;

    private void Awake() {
        animator = GetComponent<Animator>();
        psm = GetComponent<PlayerStateManager>();
    }

    private void Start()
    {
        CurrentHealth = maxHealth;
    }

    private void Update() {
        // For testing purposes
        if (ClickToHeal) {
            ClickToHeal = false;
            Heal(10);
        }
        if (ClickToDamage) {
            ClickToDamage = false;
            TakeDamage(10);
        }
    }

    public void TakeDamage(float damage)
    {
        CurrentHealth -= damage;
        OnHealthChanged?.Invoke(CurrentHealth / maxHealth);
        Debug.Log("Ouch! I only have " + CurrentHealth + " health left!");
        if (CurrentHealth <= 0)
        {
            psm.InputActions.Player.Disable();
            animator.SetTrigger("PlayerDeath");
        }
    }
    public void Heal(float healAmount)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth + healAmount , 0, maxHealth);
        OnHealthChanged?.Invoke(CurrentHealth / maxHealth);
    }

    public void Die()
    {
        gameObject.SetActive(false);
        OnPlayerDeath?.Invoke();
    }   
}
