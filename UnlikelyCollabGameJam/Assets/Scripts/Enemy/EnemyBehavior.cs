using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class EnemyBehavior : MonoBehaviour
{

    /*
    behaviors
     patrol
         - enemy moves around specific location in search of player
         - raycast a 2D cone
     pursue
         - navigate to player
     attack
         - hurrr durr attack
     MAYBE retreat
         - on low health run from player
    
    */
    Rigidbody2D rb;

    PatrolManager pm;

    [Header("Vision Settings")]
    public float rayLength = 10f;
    public float spreadAngle = 45f;
    public int numberOfRays = 10;

    [Header("Detection Settings")]
    public LayerMask detectionLayer;

    [Header("Patrol & Movement Settings")]
    // public List<GameObject> patrolPoints = new();
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

    private float enemyAttackDamage = 5f;

    void Awake()
    {
        pm = GameObject.Find("PatrolManager").GetComponent<PatrolManager>();
        // SelectRandomPatrolPointFromList(); 
        target = pm.GetNextPatrolPointInPath((Vector2)transform.position);
        forwardDir = transform.right;
        rb = GetComponent<Rigidbody2D>();
        enemyState = EnemyStates.Patrol;
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Ignore Collision"), 0, true);
    }

    // private void SelectRandomPatrolPointFromList()
    // {
    //     int randPatrolPointIndex = Random.Range(0, patrolPoints.Count);
    //     target = new Vector2(patrolPoints[randPatrolPointIndex].transform.position.x, patrolPoints[randPatrolPointIndex].transform.position.y);
    // }

    void Update()
    {
        // Debug.Log("Velocity: "+rb.linearVelocity);
        SetForwardDirection();




        if (enemyState == EnemyStates.Patrol)
        {
            //Patrol
            // add movement left and right (maybe up and down? random jumping?)
            // hacky movement code (only works in x atm)

            NavigateToTarget();


            // if near a patrol point, select a new one
            // this will eventually get next patrol point in path, 
            // if at last patrol point select a random one and get a new list of patrol points
            if (isAtTarget())
            {
                target = pm.GetNextPatrolPointInPath((Vector2)transform.position);
            }

            bool CanSeePlayer = CastRays();

            if (CanSeePlayer)
            {
                // target = (Vector2)player.transform.position;
                // enemyState = EnemyStates.Pursue;
            }
        }
        else if (enemyState == EnemyStates.Pursue)
        {
            // Pursue
            target = (Vector2)player.transform.position;
            NavigateToTarget();
            if((player.transform.position - transform.position).magnitude < 1f)
            {
                enemyState = EnemyStates.Attack;
            }
            else if((player.transform.position - transform.position).magnitude > 20f)
            {
                enemyState = EnemyStates.Patrol;
            }
            else if(enemyHealth < 10f)
            {
                enemyState = EnemyStates.Retreat;
            }
        }
        else if (enemyState == EnemyStates.Attack)
        {
            // Attack
            player.GetComponent<PlayerHealth>().TakeDamage(enemyAttackDamage);
            enemyState = EnemyStates.Pursue;
        }
        else if (enemyState == EnemyStates.Retreat)
        {
            // RETREATTT
            // TODO: pick a patrol point FAR from player and increase speed temporarily and heal up?
        }
        else if(enemyState == EnemyStates.Dead)
        {
            // play death animation?
            gameObject.SetActive(false);
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
        if (rb.linearVelocityY == 0 && target.y > transform.position.y + enemyJumpThreshold && (target.x - transform.position.x) < 6f) 
        {
            // Debug.Log("Trying to Jump with force: "+enemyJumpForce);

            enemyJumpForce = (target.y - transform.position.y) * 5 + 15;
            
            rb.AddForceY(enemyJumpForce, ForceMode2D.Impulse);

            // when jump go to "Phase Layer"
            gameObject.layer = LayerMask.NameToLayer("Ignore Collision");
            
        }
        // if(rb.linearVelocityX == 0 || rb.linearVelocityX == 0 && rb.linearVelocityY == 0) // if stuck
        // {
        //     // rb.linearVelocityX = Random.Range(0, 2) == 0 ? enemySpeed*2f : -enemySpeed*2f;
        //     // enemyJumpForce = (target.y - transform.position.y) * 5f;
        //     // rb.AddForceY(enemyJumpForce, ForceMode2D.Impulse);
        // }
        // else{

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
            // Debug.DrawRay(origin, rayDirection * rayLength, Color.red, .1f);  // For visual debugging in the Scene view

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
