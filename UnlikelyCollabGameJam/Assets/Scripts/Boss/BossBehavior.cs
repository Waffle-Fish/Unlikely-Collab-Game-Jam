// using System.Numerics;
using System;
using System.Collections;
using UnityEditor.Callbacks;
using UnityEngine;
using Random = UnityEngine.Random;


public class BossBehavior : MonoBehaviour
{

    private enum BossStates {Attack, SwordAttack, FireballAttack, ScreamAttack, Enrage, Weak, Dead, Healed};

    private BossStates bossState;
    [Header("Attack CoolDown Settings")]

    [SerializeField]
    private float bossAttackCoolDown = 3f;

    [SerializeField]
    private float bossEnragedAttackCoolDown = 2f;
    private float bossAttackTimer = 0f;

    private bool enraged = false;
    [Header("Boss Health Settings")]

    [SerializeField]
    private float bossEnrageHealth = 75f;
    [SerializeField]
    private float bossWeakenedHealth = 15f;


    [SerializeField]
    private float bossMaxHealth = 250f;
    private float bossHealth;

    private bool isAttacking = false;
    
    private Rigidbody2D swordRB;

    private GameObject player;

    [Header("Sword Attack Settings")]
    [SerializeField]
    private GameObject sword;
    [SerializeField]
    private float swordDamage = 20f;
    [SerializeField]
    private float swordDamageCoolDown = 1f;
    private float swordDamageTimer;
    [SerializeField]
    private float swordWindUpTime = 3f;
    [SerializeField]
    private float swordWindUpSpeed = 10f;
    [SerializeField]
    private int swordStabCount = 4;
    private int swordStabCounter;
    [SerializeField]
    private float swordStabReach = 15f;
    [SerializeField]
    private float swordStabSpeed = 30f;
    private float swordInitialX;
    private float swordStabDirection;
    private float swordWindUpTimer;

    [Header("Fireball Attack Settings")]
    [SerializeField]
    private GameObject fireball;
    [SerializeField]
    private int numFireballs = 50;
    [SerializeField]
    private float fireballDamage = 10f;
    [SerializeField]
    private float fireballLifeTime = 8f;
    [SerializeField]
    private float fireballSpreadX = 25f;
    [SerializeField]
    private float fireballSpawnY = 30f;
    [SerializeField]
    private float fireballSpreadY = 5f;
    [SerializeField]
    private float fireballSpeedY = 10f;
    private float fireballX;
    private float fireballY;

    [Header("Scream Attack Settings")]
    [SerializeField]
    private GameObject scream;
    [SerializeField]
    private float screamDamage = 5f;
    [SerializeField]
    private float screamGrowthRate = .5f;
    [SerializeField]
    private float screamDuration = 10f;
    private float screamDurationTimer;
    [SerializeField]
    private float screamDamageCooldown = 1f;
    private float screamDamageTimer;

    [Header("Summon Mobs Settings")]
    [SerializeField]
    private GameObject basicEnemy;
    [SerializeField]
    private GameObject rangedEnemy;

    void Awake()
    {
        bossHealth = bossMaxHealth;
        swordRB = sword.GetComponent<Rigidbody2D>();
        player = GameObject.FindWithTag("Player");
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
            if(!isAttacking)
            {
                isAttacking = true;
                StartCoroutine(SwordAttack());
            }
        }
        else if (bossState == BossStates.FireballAttack)
        {
            if(!isAttacking)
            {
                isAttacking = true;    
                StartCoroutine(FireballAttack());
            }
        }
        else if (bossState == BossStates.ScreamAttack)
        {
            if(!isAttacking)
            {
                isAttacking = true;    
                StartCoroutine(ScreamAttack());
            }
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
            switch(Random.Range(0,3))
            {
                case 0:
                    bossState = BossStates.ScreamAttack;
                    break;
                case 1:
                    bossState = BossStates.ScreamAttack;
                    break;
                case 2:
                    bossState = BossStates.ScreamAttack;
                    break;
                default:
                    break;
            }
            bossAttackTimer = enraged ? bossEnragedAttackCoolDown : bossAttackCoolDown;
        }
    }

    private IEnumerator SwordAttack()
    {
        Debug.Log("Sword Attack!");
        sword.SetActive(true);
        // left or right sword stab (repetive thrust)
        // pick side player is on
        // pick correct y level
        // stab stab stab
        yield return new WaitForSeconds(1f);
        Debug.Log("Starting Sword Windup");

        swordWindUpTimer = swordWindUpTime;
        swordDamageTimer = 0f;

        while(swordWindUpTimer > 0)
        {
            swordWindUpTimer -= Time.deltaTime;
            swordDamageTimer -= Time.deltaTime;
            Vector2 targetPosition = new Vector2(sword.transform.position.x, player.transform.position.y);
            // Move the sword towards the target position at a constant speed.
            sword.transform.position = Vector2.MoveTowards(sword.transform.position, targetPosition, swordWindUpSpeed * Time.deltaTime);
            if (swordDamageTimer <= 0)
            {
                CheckSwordCollision();
            }
            yield return null;
        }
        swordRB.linearVelocityY = 0f;

        yield return new WaitForSeconds(1f);
        Debug.Log("Starting sword stab");
        swordStabCounter = swordStabCount;

        
        // Record the sword's starting horizontal position
        swordInitialX = sword.transform.position.x;
        // Determine stab direction based on player's relative position
        swordStabDirection = (player.transform.position - sword.transform.position).normalized.x;

        while (swordStabCounter > 0)
        {
            // Forward stab: move until the sword has traveled the specified reach
            while (Mathf.Abs(sword.transform.position.x - swordInitialX) < swordStabReach)
            {
                swordDamageTimer -= Time.deltaTime;
                swordRB.linearVelocity = new Vector2(swordStabDirection * swordStabSpeed, 0f);
                if (swordDamageTimer <= 0)
                {
                    CheckSwordCollision();
                }
                yield return null;
            }
            swordRB.linearVelocity = Vector2.zero;
            // yield return new WaitForSeconds(1f);

            // Return stab: move back to near the original position
            while (Mathf.Abs(sword.transform.position.x - swordInitialX) > 0.1f)
            {
                swordDamageTimer -= Time.deltaTime;
                swordRB.linearVelocity = new Vector2(-swordStabDirection * swordStabSpeed, 0f);
                if (swordDamageTimer <= 0)
                {
                    CheckSwordCollision();
                }
                yield return null;
            }
            swordRB.linearVelocity = Vector2.zero;
            // yield return new WaitForSeconds(1f);

            swordStabCounter--;
        }
        Debug.Log("Finishing Sword Attack");

        sword.SetActive(false);
        bossState = BossStates.Attack;
        isAttacking = false;
        yield return new WaitForEndOfFrame();
    }

    private void CheckSwordCollision()
    {
        // Get the sword's collider component (assumes a BoxCollider2D)
        BoxCollider2D swordCollider = sword.GetComponent<BoxCollider2D>();
        
        // Use OverlapBox to see if the sword's collider overlaps any collider on the "Player" layer.
        // Make sure the player is assigned to a layer named "Player" (or adjust the layer mask accordingly).
        Collider2D hit = Physics2D.OverlapBox(swordCollider.bounds.center, swordCollider.bounds.size, 0f, LayerMask.GetMask("Player"));
        
        if(hit != null)
        {
            Debug.Log("Sword hit the player!");
            player.GetComponent<PlayerHealth>().TakeDamage(swordDamage);
            swordDamageTimer = swordDamageCoolDown;
        }
    }

    private IEnumerator ScreamAttack()
    {
        // Debug.Log("Scream Attack!");
        // play scream sound effect
        screamDamageTimer = 0f;
        screamDurationTimer = screamDuration;

        GameObject screamInstance = Instantiate(scream, transform.position, Quaternion.identity);

        while(screamDurationTimer > 0)
        {
            screamDamageTimer -= Time.deltaTime;
            screamDurationTimer -= Time.deltaTime;

            screamInstance.transform.localScale += new Vector3(screamGrowthRate, screamGrowthRate, 0f);

            if(screamDamageTimer <= 0)
            {
                CheckScreamCollision(screamInstance);
            }
            yield return null;
        }
        
        screamInstance.SetActive(false);
        bossState = BossStates.Attack;
        isAttacking = false;
        yield return new WaitForEndOfFrame();
    }

    private void CheckScreamCollision(GameObject screamInstance)
    {
        // Get the scream's collider component (assumes a CircleCollider2D)
        CircleCollider2D screamCollider = scream.GetComponent<CircleCollider2D>();
        
        Collider2D hit = Physics2D.OverlapCircle(screamInstance.transform.position, screamInstance.transform.localScale.x*screamCollider.radius, LayerMask.GetMask("Player"));
        
        if(hit != null)
        {
            // Debug.Log("Scream hit the player!");
            player.GetComponent<PlayerHealth>().TakeDamage(swordDamage);
            screamDamageTimer = screamDamageCooldown;
        }
    }

    private IEnumerator FireballAttack()
    {
        // Debug.Log("Fireball Attack!");

        // implement stream of "fireballs" upward as indicator

        // Debug.Log("Fireball indicator");
        yield return new WaitForSeconds(2f);

        // fireball rain, randomly spawn them above player and have them rain down
        

        // Debug.Log("Spawning Fireballs Above at Random X");

        for(int i = 0; i < numFireballs; i++)
        {
            fireballX = Random.Range(transform.position.x - fireballSpreadX, transform.position.x + fireballSpreadX);
            fireballY = Random.Range(fireballSpawnY - fireballSpreadY, fireballSpawnY + fireballSpreadY);
            GameObject fireballInstance = Instantiate(fireball, new Vector2(fireballX, fireballY), Quaternion.identity);
            fireballInstance.GetComponent<Fireball>().SetLifeTime(fireballLifeTime);
            fireballInstance.GetComponent<Fireball>().SetProjectileDamage(fireballDamage);
            fireballInstance.GetComponent<Fireball>().SetVelocity(0f, fireballSpeedY);
        }
        

        // Debug.Log("Finished Fireball Attack");

        bossState = BossStates.Attack;
        isAttacking = false;
        yield return new WaitForEndOfFrame();
    }

    private void SummonMobs()
    {
        // spawn a few enemies
        Instantiate(basicEnemy, transform.position, Quaternion.identity);
        Instantiate(rangedEnemy, transform.position, Quaternion.identity);

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
