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
        Debug.Log("Ouch! I only have " + CurrentHealth + " health left!");
        if (CurrentHealth <= 0)
        {
            Die();
        }
    }
    public void Heal(float healAmount)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth + healAmount , 0, maxHealth);
    }

    private void Die()
    {
        gameObject.SetActive(false);
    }   
}
