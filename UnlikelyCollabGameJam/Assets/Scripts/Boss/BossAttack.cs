using UnityEngine;
using System.Collections;
using UnityEditor.EditorTools;

public class BossAttack : MonoBehaviour
{
     private enum BossStates {Attack, SwordAttack, FireballAttack, ScreamAttack, Enrage, Weak, Dead, Healed};
    private enum AttackStates {Sword, Fireball, Scream}

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
    [SerializeField] private GameObject sword;
    [SerializeField] private float swordDamage = 20f;
    [SerializeField] private float swordDamageCoolDown = 1f;
    [SerializeField] private float swordWindUpTime = 3f;
    [SerializeField] private float swordWindUpSpeed = 10f;
    [SerializeField] private int swordStabIterations = 4;
    [SerializeField] private int swordStabCount = 4;
    [Tooltip("Time between wind up and actually stabbing")]
    [SerializeField] private float swordTransitionStabTime = 4;
    private int swordStabCounter;
    [SerializeField] private float swordStabReach = 15f;
    [SerializeField] private float swordStabSpeed = 30f;
    private float swordDamageTimer;
    private Vector2 swordInitialPos;
    private Vector2 swordStabDirection;
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
    private float screamGrowthRate = 1.25f;
    private Vector3 intialScreamScale;
    private Vector3 inverseParentScale;

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

    Animator animator;

    void Awake()
    {
        swordRB = sword.GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        player = GameObject.FindWithTag("Player");
    }

    void Start()
    {
        bossHealth = bossMaxHealth;
        intialScreamScale = scream.transform.localScale;
        inverseParentScale = new Vector3(
            1f / transform.lossyScale.x,
            1f / transform.lossyScale.y,
            1f
        );
        bossState = BossStates.Attack;
    }

    void Update()
    {
        if (isAttacking) return;
        if (Time.time < bossAttackTimer) return;
        Attack();
    }

    private void Attack()
    {
        isAttacking = true;
        switch(Random.Range(2,2))
        {
            case 0:
                StartCoroutine(SwordAttack());
                break;
            case 1:
                StartCoroutine(FireballAttack());
                break;
            case 2:
                animator.SetBool("Scream", true);
                Debug.Log("Scream Attack!");
                break;
            default:
                break;
        }
    }

    public void ActivateSword() {
        sword.SetActive(true); 
    }
    private IEnumerator SwordAttack()
    {
        Debug.Log("Sword Attack!");
        animator.SetBool("Sword", true);
        yield return null;
        float clipDuration = animator.GetCurrentAnimatorStateInfo(0).length;
        Debug.Log("Length: " + clipDuration);
        yield return new WaitForSeconds(clipDuration);

        for (int i = swordStabIterations; i > 0; i--)
        {
            swordWindUpTimer = swordWindUpTime;
            swordDamageTimer = 0f;

            while(swordWindUpTimer > 0)
            {
                // Update timers
                swordWindUpTimer -= Time.deltaTime;
                swordDamageTimer -= Time.deltaTime;

                SwordWindUp();

                if (swordDamageTimer <= 0)
                {
                    CheckSwordCollision();
                }
                yield return null;
            }
            // Ensure sword is not moving after finishing windup
            swordRB.linearVelocityY = 0f;

            yield return new WaitForSeconds(swordTransitionStabTime);

            // Debug.Log("Starting sword stab");
            swordStabCounter = swordStabCount;
            // Record the sword's starting horizontal position
            swordInitialPos = sword.transform.position;
            // Determine stab direction based on player's relative position
            swordStabDirection = -sword.transform.right;
            float reach = Vector2.Distance(player.transform.position, sword.transform.position);
            while (swordStabCounter > 0)
            {
                // Forward stab: move until the sword has traveled the specified reach
                while (IsSwordMovingToStabPosition(reach))
                {
                    // Update timer
                    swordDamageTimer -= Time.deltaTime;

                    SwordStab();

                    if (swordDamageTimer <= 0)
                    {
                        CheckSwordCollision();
                    }
                    yield return null;
                }
                swordRB.linearVelocity = Vector2.zero;

                // Return stab: move back to near the original position
                while (IsSwordMovingToInitialPosition())
                {
                    // Update timer
                    swordDamageTimer -= Time.deltaTime;

                    SwordRetract();

                    if (swordDamageTimer <= 0)
                    {
                        CheckSwordCollision();
                    }
                    yield return null;
                }
                swordRB.linearVelocity = Vector2.zero;

                swordStabCounter--;
            }
            // Debug.Log("Finishing Sword Attack");
           
        }

        sword.SetActive(false);
        bossState = BossStates.Attack;
        isAttacking = false;
        animator.SetBool("Sword", false);
        ResetBossCooldownTimer();
        yield return new WaitForEndOfFrame();
    }

    private void SwordRetract()
    {
        swordRB.linearVelocity = -swordStabDirection * swordStabSpeed;
    }

    private void SwordStab()
    {
        swordRB.linearVelocity = swordStabDirection * swordStabSpeed;
    }

    private void SwordWindUp()
    {
        Vector2 dirToPlayer = (player.transform.position - sword.transform.position).normalized;
        float angle = Vector2.SignedAngle(Vector2.left, dirToPlayer);
        sword.transform.rotation = Quaternion.Euler(new(0, 0, angle));
    }

    private bool IsSwordMovingToInitialPosition()
    {
        return Vector2.Distance(sword.transform.position, swordInitialPos) > 0.1f;
    }

    private bool IsSwordMovingToStabPosition(float reach)
    {
        return Vector2.Distance(sword.transform.position,swordInitialPos) < reach;
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

    public void ActivateScreamAttack() {
        StartCoroutine(ScreamAttack());
    }

    private IEnumerator ScreamAttack()
    {
        scream.SetActive(true);
        
        // Reset the scream's scale so that its world scale matches the intended (uniform) initial scale.
        scream.transform.localScale = Vector3.zero;

        screamDamageTimer = 0f;
        screamDurationTimer = screamDuration;

        while (screamDurationTimer > 0)
        {
            // Update Timers
            screamDamageTimer -= Time.deltaTime;
            screamDurationTimer -= Time.deltaTime;

            // Additive growth
            scream.transform.localScale += screamGrowthRate * Time.deltaTime * Vector3.one;

            // Compute effective radius from the world (lossy) scale.
            // After counteracting the parent, the x and y should be roughly the same.
            float effectiveRadius = scream.transform.lossyScale.x/2f;

            if (screamDamageTimer <= 0)
            {
                CheckScreamCollision(effectiveRadius);
            }
            yield return null;
        }
        
        animator.SetBool("Scream", false);
        yield return null;
        scream.SetActive(false);
        // Reset the scream's scale back to its initial value (counteracting parent's scale)
        scream.transform.localScale = Vector3.Scale(intialScreamScale, inverseParentScale);

        bossState = BossStates.Attack;
        isAttacking = false;
        ResetBossCooldownTimer();
        yield return new WaitForEndOfFrame();
    }

    private void CheckScreamCollision(float effectiveRadius)
    {
        // Use the effective radius (from world space) to perform a circular overlap test.
        Collider2D hit = Physics2D.OverlapCircle(scream.transform.position, effectiveRadius, LayerMask.GetMask("Player"));
        
        if (hit != null)
        {
            // Debug.Log("Scream hit the player!");
            player.GetComponent<PlayerHealth>().TakeDamage(screamDamage);
            screamDamageTimer = screamDamageCooldown;
        }
    }


    private IEnumerator FireballAttack()
    {
        Debug.Log("Fireball Attack!");

        // implement stream of "fireballs" upward as indicator
        // Debug.Log("Fireball indicator");
        yield return new WaitForSeconds(2f);

        // fireball rain, randomly spawn them above player and have them rain down
        // Debug.Log("Spawning Fireballs Above at Random X");

        for(int i = 0; i < numFireballs; i++)
        {
            SpawnFireball();
        }

        // Debug.Log("Finished Fireball Attack");

        bossState = BossStates.Attack;
        isAttacking = false;
        yield return new WaitForEndOfFrame();
    }

    private void SpawnFireball()
    {
        // Pick Random X and Y's within specified ranges (Serialized Fields)
        fireballX = Random.Range(transform.position.x - fireballSpreadX, transform.position.x + fireballSpreadX);
        fireballY = Random.Range(fireballSpawnY - fireballSpreadY, fireballSpawnY + fireballSpreadY);

        // Instantitate a new fireball (expensive)
        GameObject fireballInstance = Instantiate(fireball, new Vector2(fireballX, fireballY), Quaternion.identity);

        // Update fireball instance stuff
        fireballInstance.GetComponent<Fireball>().SetLifeTime(fireballLifeTime);
        fireballInstance.GetComponent<Fireball>().SetProjectileDamage(fireballDamage);
        fireballInstance.GetComponent<Fireball>().SetVelocity(0f, fireballSpeedY);
    }

    private void ResetBossCooldownTimer(){
        bossAttackTimer = Time.time + (enraged ? bossEnragedAttackCoolDown : bossAttackCoolDown);
    }
}
