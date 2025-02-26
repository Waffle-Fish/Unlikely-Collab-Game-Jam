// using System.Numerics;
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
    [SerializeField]
    private float bossEnrageHealth = 75f;
    [SerializeField]
    private float bossWeakenedHealth = 15f;

    [SerializeField]
    private float bossScanCoolDown = .05f;
    private float bossScanTimer = 0f;
    private float rayLength = 25f;
    private float scanAngle = 0f;
    [SerializeField]
    private float deltaScan = 2f;

    private LayerMask detectionLayer;
    private float timeSinceSeenPlayer;

    [SerializeField]
    private float deagroTime = 5f;

    [SerializeField]
    private float bossMaxHealth = 250f;
    private float bossHealth;

    void Awake()
    {
        // called before start
        detectionLayer = LayerMask.GetMask("Default", "Player");
        bossHealth = bossMaxHealth;
        bossState = BossStates.Scan;
    }
    void Start()
    {
        // called after all awakes in scene
    }

    void Update()
    {
        bossAttackTimer -= Time.deltaTime;
        bossScanTimer -= Time.deltaTime;

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
        
        if (bossScanTimer <= 0)
        {
            bossScanTimer = bossScanCoolDown;

            Vector2 origin = transform.position + new Vector3(0f, .5f, 0f);

            // Interpolate angles from -halfAngle to +halfAngle
            if (scanAngle > 180 || scanAngle < 0) deltaScan *= -1;
            
            scanAngle += deltaScan;
            // Rotate the forward direction by the current angle
            Vector2 rayDirection = Quaternion.Euler(0, 0, scanAngle) * transform.right;
            RaycastHit2D hit = Physics2D.Raycast(origin, rayDirection, rayLength, detectionLayer);
            
            Debug.DrawRay(origin, rayDirection * ((Vector2)transform.position - hit.point).magnitude, Color.red, .5f);  // For visual debugging in the Scene view

            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                Debug.Log("Player Detected!");
                bossState = BossStates.Attack;
            }
        
        }
    
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
        
        
        if (CanSeePlayer())
        {
            timeSinceSeenPlayer = 0;
        }
        else
        {
            timeSinceSeenPlayer += Time.deltaTime;
        }

        if(timeSinceSeenPlayer > deagroTime && !enraged) //after enraging boss you can no longer hide
        {
            bossState = BossStates.Scan;
        }

        // maybe attack a few times before returning to scan
    }

    private void SwordAttack()
    {
        // maybe have sword stretch out at the player's location
        // giving time to dodge
        Debug.Log("Sword Attack!");
    }

    private void ScreamAttack()
    {
        // difficult - have scream attack to damage to everything in direct line of sight
        Debug.Log("Scream Attack!");
    }

    private void FireballAttack()
    {
        // spew fireballs (projectiles - maybe child class of) everywhere
        Debug.Log("Fireball Attack!");
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

    protected bool CanSeePlayer()
    {
        Vector2 origin = transform.position + new Vector3(0f, .5f, 0f);
        float spreadAngle = 180f;
        float halfAngle = spreadAngle / 2f;
        float numRays = 60;

        for (int i = 0; i < numRays; i++)
        {
            // Interpolate angles from -halfAngle to +halfAngle
            float angle = Mathf.Lerp(-halfAngle, halfAngle, i / (float)(numRays - 1));
            // Rotate the forward direction by the current angle
            Vector2 rayDirection = Quaternion.Euler(0, 0, angle) * transform.up;

            RaycastHit2D hit = Physics2D.Raycast(origin, rayDirection, rayLength, detectionLayer);
            Debug.DrawRay(origin, rayDirection * ((Vector2)transform.position - hit.point).magnitude, Color.red, .5f);  // For visual debugging in the Scene view

            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
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
