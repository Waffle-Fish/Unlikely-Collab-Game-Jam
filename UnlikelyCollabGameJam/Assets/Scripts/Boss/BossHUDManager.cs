using UnityEngine;
using TMPro;

public class BossHUDManager : MonoBehaviour
{
    TextMeshProUGUI healthTMP;
    [SerializeField] BossHealth bossHealth;

    void Awake()
    {
        healthTMP = GetComponentInChildren<TextMeshProUGUI>();
    }

    void Start()
    {
        // DeathOverlay.SetActive(false);
        bossHealth.OnHealthChanged += UpdateHealth;
        // bossHealth.OnPlayerDeath += ProcessDeathOverlay;
    }

    private void UpdateHealth(float currentHealth)
    {
        healthTMP.text = "Health: " + currentHealth.ToString();
    }

    // private void ProcessDeathOverlay()
    // {
    //     DeathOverlay.SetActive(true);
    // }
}
