// using System.Numerics;
using UnityEngine;

public class BossBehavior : MonoBehaviour
{

    private enum BossStates {Attack, SwordAttack, FireballAttack, ScreamAttack, Enrage, Weak, Dead, Healed};

    private BossStates bossState;

    [SerializeField]
    private float bossAttackCoolDown = 5f;

    [SerializeField]
    private float bossEnragedAttackCoolDown = 3f;
    private float bossAttackTimer = 0f;

    private bool enraged = false;

    [SerializeField]
    private float bossEnrageHealth = 75f;
    [SerializeField]
    private float bossWeakenedHealth = 15f;

    [SerializeField]
    private float deltaScan = 2f;


    [SerializeField]
    private float bossMaxHealth = 250f;
    private float bossHealth;

    void Awake()
    {
        bossHealth = bossMaxHealth;
        bossState = BossStates.Attack;
    }
    void Start()
    {
        // called after all awakes in scene
    }

    void Update()
    {
        if (bossState == BossStates.Attack)
        {
            Attack();
        }
        else if (bossState == BossStates.SwordAttack)
        {
            SwordAttack();
        }
        else if (bossState == BossStates.FireballAttack)
        {
            FireballAttack();
        }
        else if (bossState == BossStates.ScreamAttack)
        {
            ScreamAttack();
        }
        else if (bossState == BossStates.Enrage)
        {
            // when boss reaches X health, boss enrages 
            // attacks become faster and more powerful
            Enrage();
        }
        else if (bossState == BossStates.Weak)
        {
            // boss is low enough health to give player the choice of healing or killing
            Weak();
        }
        else if (bossState == BossStates.Healed)
        {
            // boss reverts to OG player
            Heal();
        }
        else if (bossState == BossStates.Dead)
        {
            // death animation
            Die();
        }
    }


    private void Attack()
    {
        bossAttackTimer -= Time.deltaTime;
        if(bossAttackTimer <= 0f)
        {
            switch(Random.Range(0,2))
            {
                case 0:
                    bossState = BossStates.SwordAttack;
                    break;
                case 1:
                    bossState = BossStates.ScreamAttack;
                    break;
                case 2:
                    bossState = BossStates.FireballAttack;
                    break;
                default:
                    break;
            }
            bossAttackTimer = enraged ? bossEnragedAttackCoolDown : bossAttackCoolDown;
        }

        // maybe attack a few times before returning to scan
    }

    private void SwordAttack()
    {
        Debug.Log("Sword Attack!");
        // left or right sword stab (repetive thrust)
        // pick side player is on
        // pick correct y level
        // stab stab stab
    }

    private void ScreamAttack()
    {
        // difficult - have scream attack to damage to everything in direct line of sight
        Debug.Log("Scream Attack!");
    }

    private void FireballAttack()
    {
        Debug.Log("Fireball Attack!");

        // shoot stream of fireballs directly upward
        // fireballs spawn randomly above and rain down
    }

    private void SummonMobs()
    {
        // spawn a few enemies
        // will have to assign a few gameobjects and choose from them which to spawn
    }

    private void Enrage()
    {
        // ANIMATION - Enrage
        // add small wait timer
        enraged = true;
    }

    private void Weak()
    {
        // ANIMATION - Weakened
        // give player to choice to heal or kill
        // perhaps create a variable that says canTakeDamage and set to false in this mode
    }

    private void Heal()
    {
        // ANIMATION - Healed
        // have boss patrol or maybe do a silly dance?
    }

    private void Die()
    {
        // ANIMATION - Death
        gameObject.SetActive(false);
    }

    public void TakeDamage(float damage)
    {
        bossHealth -= damage;
        if(bossHealth <= bossEnrageHealth)
        {
            bossState = BossStates.Enrage;
        }
        else if(bossHealth <= bossWeakenedHealth)
        {
            bossState = BossStates.Weak;
        }
        else if(bossHealth <= 0)
        {
            bossState = BossStates.Dead;
        }
    }
}
