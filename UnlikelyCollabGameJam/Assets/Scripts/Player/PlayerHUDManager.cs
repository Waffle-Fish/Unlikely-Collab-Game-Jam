using System;
using TMPro;
using UnityEngine;

public class PlayerHUDManager : MonoBehaviour
{
    TextMeshProUGUI healthTMP;

    PlayerHealth playerHealth;
    void Awake()
    {
        playerHealth = GameObject.FindWithTag("Player").GetComponent<PlayerHealth>();
        healthTMP = GetComponentInChildren<TextMeshProUGUI>();
    }

    void Start()
    {
        playerHealth.OnHealthChanged += UpdateHealth;
    }

    private void UpdateHealth(float currentHealth)
    {
        healthTMP.text = "Health: " + currentHealth.ToString();
    }
}
