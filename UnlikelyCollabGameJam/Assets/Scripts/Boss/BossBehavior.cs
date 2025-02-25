using UnityEngine;

public class BossBehavior : MonoBehaviour
{

    private enum BossStates {Scan, Attack, Enrage, Weak, Dead, Healed};

    private BossStates bossState = BossStates.Scan;

    [SerializeField]
    private float bossAttackCoolDown = 5f;
    [SerializeField]
    private float bossEnragedAttackCoolDown = 3f;
    private float bossAttackTimer = 0f;

    private bool enraged = false;

    void Awake()
    {
        // called before start
    }
    void Start()
    {
        // called after all awakes in scene
    }

    void Update()
    {
        bossAttackTimer -= Time.deltaTime;

        if (bossState == BossStates.Scan)
        {
            // slow scan left to right
            // when found player go into attack mode
            Scan();
        }
        else if (bossState == BossStates.Attack)
        {
            Attack();
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
            // boss is cool now... maybe just patrols around randomly?
            Heal();
        }
        else if (bossState == BossStates.Dead)
        {
            // death animation
            Die();
        }
    }


    private void Scan()
    {
        // would be super cool if boss casted a beam of light where it was looking for player to avoid
        // if we see player -> attack
        // bossState = BossStates.Attack;
    }

    private void Attack()
    {
        if(bossAttackTimer <= 0f)
        {
            switch(Random.Range(0,2))
            {
                case 0:
                    SwordAttack();
                    break;
                case 1:
                    ScreamAttack();
                    break;
                case 2:
                    FireballAttack();
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
        // maybe have sword stretch out at the player's location
        // giving time to dodge
    }

    private void ScreamAttack()
    {
        // difficult - have scream attack to damage to everything in direct line of sight
    }

    private void FireballAttack()
    {
        // spew fireballs (projectiles - maybe child class of) everywhere
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
}
