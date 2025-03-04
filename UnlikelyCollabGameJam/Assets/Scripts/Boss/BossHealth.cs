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
    private BossSFX bossSFX;

    private float hp;
    private bool isEnrage = false;

    private void Awake() {
        bossAttack = GetComponent<BossAttack>();
        animator = GetComponent<Animator>();
        bossSFX = GetComponent<BossSFX>();
    }

    private void Start() {
        hp = maxHealth;
        
    }

    public void TakeDamage(float damage)
    {
        hp -= damage;
        OnHealthChanged?.Invoke(hp/maxHealth);
        bossSFX.PlayTakeDamageSFX();
        if(!isEnrage && hp <= bossEnrageHealth)
        {
            isEnrage = true;
            bossSFX.PlayRageSFX();
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
