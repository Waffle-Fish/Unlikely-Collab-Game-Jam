using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class EnemyBehavior : MonoBehaviour
{
    protected Rigidbody2D rb;
    protected PatrolManager pm;

    [Header("Vision Settings")]
    [SerializeField]
    protected float rayLength = 10f;
    [SerializeField]
    protected float spreadAngle = 45f;
    [SerializeField]
    protected int numberOfRays = 10;

    [Header("Detection Settings")]
    protected LayerMask detectionLayer;

    [Header("Patrol & Movement Settings")]
    protected Vector2 target;
    protected enum EnemyStates { Patrol, Pursue, Attack, Retreat, Dead, ImStuck }
    protected EnemyStates enemyState;

    [SerializeField]
    protected float enemySpeed = 4.5f;
    
    [Header("Jump Settings")]
    protected float enemyJumpForce = 55f;
    protected float dynamicJumpForce;
    protected float enemyJumpThreshold = 1f;

    protected Vector2 forwardDir;

    [SerializeField]
    protected float maxEnemyHealth = 100f;
    protected float enemyHealth = 100f;

    protected GameObject player;

    [Header("Attack Settings")]
    [SerializeField]
    protected float enemyAttackDamage = 5f;

    [Tooltip("Number of Seconds Between Attacks")]
    [SerializeField]
    protected float enemyAttackCoolDown = 10f;
    protected float enemyAttackTimer = 0f;

    protected int randomStuckDirection;
    protected float initialStuckY;

    protected virtual void Awake()
    {
        pm = GetComponentInChildren<PatrolManager>();
        forwardDir = transform.right;
        rb = GetComponent<Rigidbody2D>();
        enemyState = EnemyStates.Patrol;
        detectionLayer = LayerMask.GetMask("Player");
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Ignore Collision"), 0, true);
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Ignore Collision"), LayerMask.NameToLayer("Player"), true);
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Ignore Collision"), LayerMask.NameToLayer("Ignore Collision"), true);
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Enemy"), LayerMask.NameToLayer("Player"), true);
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Enemy"), LayerMask.NameToLayer("Enemy"), true);
    }

    void Start()
    {
        target = pm.GetNextPatrolPointInPath((Vector2)transform.position);
        enemyHealth = maxEnemyHealth;
    }

    void Update()
    {
        SetForwardDirection();
        enemyAttackTimer -= Time.deltaTime;

        if (enemyState == EnemyStates.Patrol)
        {
            Patrol();
        }
        else if (enemyState == EnemyStates.Pursue)
        {
            // If player is dead go back to patrolling
            // if(player.GetComponent<PlayerHealth>().CurrentHealth <= 0)
            // {
            //     enemyState = EnemyStates.Patrol;
            // }
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
        pm.DrawDebugPath((Vector2)transform.position);
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

    protected virtual void Patrol()
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

    protected virtual void Attack()
    {
        // Attack
        if (enemyAttackTimer <= 0)
        {
            // ANIMATION - Attack goes here
            player.GetComponent<PlayerHealth>().TakeDamage(enemyAttackDamage);
            enemyAttackTimer = enemyAttackCoolDown;
        }
        enemyState = EnemyStates.Pursue;
    }

    protected virtual void Pursue()
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

    protected bool isFarFromPlayer()
    {
        return (player.transform.position - transform.position).magnitude > 20f;
    }

    protected bool isNearPlayer()
    {
        return (player.transform.position - transform.position).magnitude < 1f;
    }

    protected bool isStuck()
    {
        return target.y < transform.position.y && (int)Math.Abs(rb.linearVelocity.magnitude) == 0;
    }

    protected bool isAtTarget()
    {
        return transform.position.x < target.x + 1f && transform.position.x > target.x - 1f &&
                transform.position.y < target.y + 1f && transform.position.y > target.y - 1f;
    }

    protected virtual void NavigateToTarget()
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
        
        // FIXME? hacky fix for enemy falling through floor occasionally
        if (transform.position.y < -100f)
        {
            Debug.Log("I've fallen, AND I CANT GET UP");
            gameObject.layer = LayerMask.NameToLayer("Enemy");
            transform.position = new Vector2(transform.position.x, 100f);
            enemyState = EnemyStates.Patrol;
            rb.linearVelocityY = 0f;
        }
    }

    private bool isAtApexOfJump()
    {
        // possible fix for enemy falling through floor would be keeping track of last y value and current,
        // checking to see when the last y value is higher than current (this first instance would be apex)
        return gameObject.layer == LayerMask.NameToLayer("Ignore Collision") && rb.linearVelocityY < 0.15f;
    }

    private void Jump()
    {
        dynamicJumpForce = (target.y - transform.position.y) * 5f + 15f;
        dynamicJumpForce = Math.Clamp(dynamicJumpForce, 25f, 55f);
        // Debug.Log("Jump Force: "+dynamicJumpForce);

        rb.AddForceY(dynamicJumpForce, ForceMode2D.Impulse);

        // when jump go to "Phase Layer"
        gameObject.layer = LayerMask.NameToLayer("Ignore Collision");
    }

    private bool ShouldJump()
    {
        return rb.linearVelocityY == 0 && target.y > transform.position.y + enemyJumpThreshold && Mathf.Abs(target.x - transform.position.x) < 2.2f;
    }

    protected void SetForwardDirection()
    {
        if (Math.Abs(rb.linearVelocity.x) > 0.1)
        {
            forwardDir = rb.linearVelocity.x > 0 ? transform.right : -transform.right;
        }
    }

    protected bool CastRays()
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
