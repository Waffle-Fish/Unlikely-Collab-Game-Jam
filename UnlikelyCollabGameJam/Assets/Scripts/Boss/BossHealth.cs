using System;
using UnityEngine;
using UnityEngine.Playables;

public class BossHealth : MonoBehaviour, IDamageable
{
    [Header("Boss Health Settings")]
    [SerializeField] private float bossEnrageHealth = 75f;
    [SerializeField] private float maxHealth = 250f;
    [SerializeField] private PlayableDirector playableDirector;

    public event Action<float> OnHealthChanged;
    public event Action OnPlayerDeath;

    private BossAttack bossAttack;
    private Animator animator;

    private float hp;
    private bool isEnrage = false;

    private void Awake() {
        bossAttack = GetComponent<BossAttack>();
        animator = GetComponent<Animator>();
    }

    private void Start() {
        hp = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        Debug.Log("Boss Current heatlh: " + hp);
        hp -= damage;
        if(!isEnrage && hp <= bossEnrageHealth)
        {
            isEnrage = true;
            bossAttack.Enrage();
        }
        if(hp <= 0)
        {
            Dead();
        }
    }

    private void Dead()
    {
        ResetAnimParams();
        animator.SetBool("Dead", true);
        playableDirector.Play();
    }

    private void ResetAnimParams() {
        foreach (var p in animator.parameters)
        {
            if (p.type == AnimatorControllerParameterType.Bool) animator.SetBool(p.name, false);
            if (p.type == AnimatorControllerParameterType.Trigger) animator.ResetTrigger(p.name);
        }
    }
}
