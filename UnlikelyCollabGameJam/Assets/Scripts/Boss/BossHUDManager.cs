using UnityEngine;
using TMPro;

public class BossHUDManager : MonoBehaviour
{
    TextMeshProUGUI healthTMP;
    [SerializeField] BossHealth bossHealth;
    [SerializeField] RectTransform healthSlider;

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

    private void UpdateHealth(float healthPercent)
    {
        healthSlider.anchorMax = new(healthPercent, healthSlider.anchorMax.y);
        healthSlider.offsetMax = new(0, healthSlider.offsetMax.y);
    }
}
