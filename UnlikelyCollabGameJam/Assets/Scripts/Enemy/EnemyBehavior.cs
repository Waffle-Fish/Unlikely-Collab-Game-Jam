using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class EnemyBehavior : MonoBehaviour
{
    Rigidbody2D rb;
    PatrolManager pm;

    [Header("Vision Settings")]
    public float rayLength = 10f;
    public float spreadAngle = 45f;
    public int numberOfRays = 10;

    [Header("Detection Settings")]
    public LayerMask detectionLayer;

    [Header("Patrol & Movement Settings")]
    private Vector2 target;
    private enum EnemyStates { Patrol, Pursue, Attack, Retreat, Dead, ImStuck }
    private EnemyStates enemyState;

    [SerializeField]
    private float enemySpeed = 4.5f;
    
    [Header("Jump Settings")]
    public float enemyJumpForce = 55f;
    private float dynamicJumpForce;
    private float enemyJumpThreshold = 1f;

    private Vector2 forwardDir;

    [SerializeField]
    private float maxEnemyHealth = 100f;
    private float enemyHealth = 100f;

    private GameObject player;

    [SerializeField]
    private float enemyAttackDamage = 5f;

    [SerializeField]
    private int enemyAttackCoolDown = 10;
    private int enemyAttackTimer = 0;

    private int randomStuckDirection;
    private float initialStuckY;

    void Awake()
    {
        pm = GameObject.Find("PatrolManager").GetComponent<PatrolManager>();
        target = pm.GetNextPatrolPointInPath((Vector2)transform.position);
        forwardDir = transform.right;
        rb = GetComponent<Rigidbody2D>();
        enemyState = EnemyStates.Patrol;
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Ignore Collision"), 0, true);
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Ignore Collision"), LayerMask.NameToLayer("Player"), true);
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Enemy"), LayerMask.NameToLayer("Player"), true);
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Enemy"), LayerMask.NameToLayer("Enemy"), true);
    }

    void Start()
    {
        enemyHealth = maxEnemyHealth;
    }

    void Update()
    {
        SetForwardDirection();

        if (enemyState == EnemyStates.Patrol)
        {
            Patrol();
        }
        else if (enemyState == EnemyStates.Pursue)
        {
            Pursue();
        }
        else if (enemyState == EnemyStates.ImStuck)
        {
            GetUnStuck();
        }
        else if (enemyState == EnemyStates.Attack)
        {
            Attack();
        }
        else if (enemyState == EnemyStates.Retreat)
        {
            // RETREATTT
            // TODO: pick a patrol point FAR from player and increase speed temporarily and heal up?
            enemyState = EnemyStates.Pursue;
        }
        else if(enemyState == EnemyStates.Dead)
        {
            Die();
        }

        // DRAW DEBUG PATH
        // pm.DrawDebugPath((Vector2)transform.position);
    }

    private void GetUnStuck()
    {
        rb.linearVelocityX = enemySpeed * randomStuckDirection;

        if (initialStuckY > transform.position.y)
        {
            enemyState = EnemyStates.Pursue;
        }
    }

    private void Die()
    {
        // ANIMATION - Death Animation
        gameObject.SetActive(false);
    }

    private void Patrol()
    {
        //Patrol
        NavigateToTarget();

        if (isAtTarget())
        {
            target = pm.GetNextPatrolPointInPath((Vector2)transform.position);
        }
        // if target is directly below me -> get next point in path
        if (target.y < transform.position.y && transform.position.x < target.x + 2f && transform.position.x > target.x - 2f)
        {
            target = pm.GetNextPatrolPointInPath((Vector2)transform.position);
        }
        else if (target.y > transform.position.y + 5.5f && transform.position.x < target.x + 2f && transform.position.x > target.x - 2f)
        {
            target = pm.GetNextPatrolPointInPath((Vector2)transform.position);
        }

        bool CanSeePlayer = CastRays();

        if (CanSeePlayer)
        {
            target = (Vector2)player.transform.position;
            enemyState = EnemyStates.Pursue;
        }
    }

    private void Attack()
    {
        // Attack
        if (enemyAttackTimer <= 0)
        {
            // ANIMATION - Attack goes here
            player.GetComponent<PlayerHealth>().TakeDamage(enemyAttackDamage);
            enemyAttackTimer = enemyAttackCoolDown;
        }
        // SCUFFED timer -> need better implementation
        enemyAttackTimer--;
        enemyState = EnemyStates.Pursue;
    }

    private void Pursue()
    {
        // Pursue
        NavigateToTarget();

        target = (Vector2)player.transform.position;

        if (isNearPlayer())
        {
            enemyState = EnemyStates.Attack;
        }
        else if (isFarFromPlayer())
        {
            enemyState = EnemyStates.Patrol;
        }
        else if (enemyHealth < 10f)
        {
            enemyState = EnemyStates.Retreat;
        }

        if (isStuck())
        {
            randomStuckDirection = Random.Range(0, 2) > 0 ? 1 : -1;
            initialStuckY = transform.position.y;
            enemyState = EnemyStates.ImStuck;
        }
    }

    private bool isFarFromPlayer()
    {
        return (player.transform.position - transform.position).magnitude > 20f;
    }

    private bool isNearPlayer()
    {
        return (player.transform.position - transform.position).magnitude < 1f;
    }

    private bool isStuck()
    {
        return target.y < transform.position.y && (int)Math.Abs(rb.linearVelocity.magnitude) == 0;
    }

    private bool isAtTarget()
    {
        return transform.position.x < target.x + 1f && transform.position.x > target.x - 1f &&
                       transform.position.y < target.y + 1f && transform.position.y > target.y - 1f;
    }

    private void NavigateToTarget()
    {
        if (ShouldJump())
        {
            // ANIMATION - JUMP
            Jump();
        }

        // ANIMATION - Walking left / right - use "forwardDir"
        if (enemyState == EnemyStates.Patrol)
        {
            // constant speed left / right
            rb.linearVelocityX = enemySpeed * Mathf.Sign(target.x - transform.position.x);
        }
        else if (enemyState == EnemyStates.Pursue)
        {
            // slower speed when approaching player - makes it a little easier? can change
            rb.linearVelocityX = enemySpeed * (target - (Vector2)transform.position).normalized.x;
        }

        if (isAtApexOfJump())
        {
            // ANIMATION - Top of jump reached... falling animation?
            gameObject.layer = LayerMask.NameToLayer("Enemy");
        }
    }

    private bool isAtApexOfJump()
    {
        return gameObject.layer == LayerMask.NameToLayer("Ignore Collision") && rb.linearVelocityY < 0.1f;
    }

    private void Jump()
    {
        dynamicJumpForce = (target.y - transform.position.y) * 5f + 15f;

        rb.AddForceY(dynamicJumpForce, ForceMode2D.Impulse);

        // when jump go to "Phase Layer"
        gameObject.layer = LayerMask.NameToLayer("Ignore Collision");
    }

    private bool ShouldJump()
    {
        return rb.linearVelocityY == 0 && target.y > transform.position.y + enemyJumpThreshold && Mathf.Abs(target.x - transform.position.x) < 2.2f;
    }

    private void SetForwardDirection()
    {
        if (Math.Abs(rb.linearVelocity.x) > 0.1)
        {
            forwardDir = rb.linearVelocity.x > 0 ? transform.right : -transform.right;
        }
    }

    bool CastRays()
    {
        Vector2 origin = transform.position + new Vector3(0f, .5f, 0f);
        float halfAngle = spreadAngle / 2f;

        for (int i = 0; i < numberOfRays; i++)
        {
            // Interpolate angles from -halfAngle to +halfAngle
            float angle = Mathf.Lerp(-halfAngle, halfAngle, i / (float)(numberOfRays - 1));
            // Rotate the forward direction by the current angle
            Vector2 rayDirection = Quaternion.Euler(0, 0, angle) * forwardDir;

            RaycastHit2D hit = Physics2D.Raycast(origin, rayDirection, rayLength, detectionLayer);
            Debug.DrawRay(origin, rayDirection * rayLength, Color.red, .1f);  // For visual debugging in the Scene view

            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                player = hit.collider.gameObject;
                return true;
            }
        }
        return false;
    }


    public void TakeDamage(float amount)
    {
        enemyHealth -= amount;
        if(enemyHealth <= 0)
        {
            enemyState = EnemyStates.Dead;
        }
    }
    
}
