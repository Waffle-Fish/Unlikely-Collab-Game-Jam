using System;
using TMPro;
using UnityEngine;

public class PlayerHUDManager : MonoBehaviour
{
    TextMeshProUGUI healthTMP;
    PlayerHealth playerHealth;
    [SerializeField] GameObject DeathOverlay;

    void Awake()
    {
        playerHealth = GameObject.FindWithTag("Player").GetComponent<PlayerHealth>();
        healthTMP = GetComponentInChildren<TextMeshProUGUI>();
    }

    void Start()
    {
        DeathOverlay.SetActive(false);
        playerHealth.OnHealthChanged += UpdateHealth;
        playerHealth.OnPlayerDeath += ProcessDeathOverlay;
    }

    private void UpdateHealth(float currentHealth)
    {
        healthTMP.text = "Health: " + currentHealth.ToString();
    }

    private void ProcessDeathOverlay()
    {
        DeathOverlay.SetActive(true);
    }
}
