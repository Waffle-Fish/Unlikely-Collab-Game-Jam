using System;
using UnityEngine;

public class SkillsCooldownSlider : MonoBehaviour
{
    public enum Skills { Dash, Fireball, Scream }
    public Skills skill;

    [SerializeField] RectTransform coverSlider;
    [SerializeField] GameObject NoMark;
    [SerializeField] GameObject YesMark;

    PlayerAttack playerAttack;
    PlayerMovement playerMovement;

    private void Awake() {
        playerAttack = GameObject.FindWithTag("Player").GetComponent<PlayerAttack>();
        playerMovement = GameObject.FindWithTag("Player").GetComponent<PlayerMovement>();
    }

    void Start()
    {
        if (skill == Skills.Dash) playerMovement.OnDashUsed += UpdateCoverUI;
        if (skill == Skills.Fireball) playerAttack.OnFireballUsed += UpdateCoverUI;
        if (skill == Skills.Scream) playerAttack.OnScreamUsed += UpdateCoverUI;
    }

    private void UpdateCoverUI(float percent)
    {
        if (percent > 0.05f) {
            coverSlider.anchorMax = new(Mathf.Min(percent, 0.9f), coverSlider.anchorMax.y);
            NoMark.SetActive(true);
            YesMark.SetActive(false);
        } else {
            coverSlider.anchorMax = new(0, coverSlider.anchorMax.y);
            NoMark.SetActive(false);
            YesMark.SetActive(true);
        }
        coverSlider.offsetMax = new(0, coverSlider.offsetMax.y);
    }
}
