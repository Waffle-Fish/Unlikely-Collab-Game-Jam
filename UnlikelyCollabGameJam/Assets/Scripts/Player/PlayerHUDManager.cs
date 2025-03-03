using System;
using TMPro;
using UnityEngine;

public class PlayerHUDManager : MonoBehaviour
{
    PlayerHealth playerHealth;
    [SerializeField] GameObject DeathOverlay;
    [SerializeField] RectTransform healthSlider;

    void Awake()
    {
        playerHealth = GameObject.FindWithTag("Player").GetComponent<PlayerHealth>();
    }

    void Start()
    {
        DeathOverlay.SetActive(false);
        playerHealth.OnHealthChanged += UpdateHealth;
        playerHealth.OnPlayerDeath += ProcessDeathOverlay;
    }

    private void UpdateHealth(float healthPercentage)
    {
        healthSlider.anchorMax = new(healthPercentage, healthSlider.anchorMax.y);
        healthSlider.offsetMax = new(0, healthSlider.offsetMax.y);
    }

    private void ProcessDeathOverlay()
    {
        DeathOverlay.SetActive(true);
    }
}
