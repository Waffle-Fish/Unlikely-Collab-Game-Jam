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
    private enum EnemyStates { Patrol, Pursue, Attack, Retreat, Dead }
    private EnemyStates enemyState;

    [SerializeField]
    private float enemySpeed = 5f;
    
    [Header("Jump Settings")]
    public float enemyJumpForce = 50f;
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

        pm.DrawDebugPath((Vector2)transform.position);
    }

    private void Die()
    {
        // play death animation?
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

        // FIXME: in situation where enemy is above player on a platform they will not navigate downward
        // if(target.y < transform.position.y && rb.linearVelocityX == 0)
        // {
        //     target = new Vector2(target.x + 5, target.y);
        // }
        // else{
            target = (Vector2)player.transform.position;
        // }

        if ((player.transform.position - transform.position).magnitude < 1f)
        {
            enemyState = EnemyStates.Attack;
        }
        else if ((player.transform.position - transform.position).magnitude > 20f)
        {
            enemyState = EnemyStates.Patrol;
        }
        else if (enemyHealth < 10f)
        {
            enemyState = EnemyStates.Retreat;
        }
    }

    private bool isAtTarget()
    {
        return transform.position.x < target.x + 1f && transform.position.x > target.x - 1f &&
                       transform.position.y < target.y + 1f && transform.position.y > target.y - 1f;
    }

    private void NavigateToTarget()
    {
        // if should jump -> jump
        if (rb.linearVelocityY == 0 && target.y > transform.position.y + enemyJumpThreshold && Mathf.Abs(target.x - transform.position.x) < 3f) 
        {
            // Debug.Log("Trying to Jump with force: "+enemyJumpForce); 

            enemyJumpForce = (target.y - transform.position.y) * 5 + 12;
            
            rb.AddForceY(enemyJumpForce, ForceMode2D.Impulse);

            // when jump go to "Phase Layer"
            gameObject.layer = LayerMask.NameToLayer("Ignore Collision");
            
        }

        // This is hacky fix to enemy getting stuck
        // if(rb.linearVelocityX == 0 && rb.linearVelocityY == 0 && target.y < transform.position.y) // if stuck
        // {
        //     rb.linearVelocityX = Random.Range(0, 2) == 0 ? enemySpeed*2f : -enemySpeed*2f;
        //     enemyJumpForce = (target.y - transform.position.y) * 5f;
        //     rb.AddForceY(enemyJumpForce, ForceMode2D.Impulse);
        // }
        
        rb.linearVelocityX = enemySpeed * (target - (Vector2)transform.position).normalized.x;

        if(gameObject.layer == LayerMask.NameToLayer("Ignore Collision") && rb.linearVelocityY < 0.1f)
        {
            gameObject.layer = LayerMask.NameToLayer("Enemy");
        }
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
                // Debug.Log("Player Detected!");
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
