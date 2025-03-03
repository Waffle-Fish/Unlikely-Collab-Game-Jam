using System;
using UnityEngine;

public class BossHealth : MonoBehaviour, IDamageable
{
    [Header("Boss Health Settings")]
    [SerializeField]
    private float bossEnrageHealth = 75f;

    [SerializeField]
    private float maxHealth = 250f;
    private float hp;
    private BossAttack bossAttack;
    private bool isEnrage = false;

    public event Action<float> OnHealthChanged;
    public event Action OnPlayerDeath;

    private void Awake() {
        bossAttack = GetComponent<BossAttack>();
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
        Debug.Log("Dead");
        throw new NotImplementedException();
    }
}
