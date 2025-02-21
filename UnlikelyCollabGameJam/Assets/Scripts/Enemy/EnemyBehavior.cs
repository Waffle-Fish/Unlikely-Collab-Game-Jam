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
    }

    // private void SelectRandomPatrolPointFromList()
    // {
    //     int randPatrolPointIndex = Random.Range(0, patrolPoints.Count);
    //     target = new Vector2(patrolPoints[randPatrolPointIndex].transform.position.x, patrolPoints[randPatrolPointIndex].transform.position.y);
    // }

    void Update()
    {
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
        return transform.position.x < target.x + 0.5f && transform.position.x > target.x - 0.5f &&
                       transform.position.y < target.y + 0.5f && transform.position.y > target.y - 0.5f;
    }

    private void NavigateToTarget()
    {
        // if should jump -> jump
        if (Math.Abs(rb.linearVelocityY) < .01f && target.y > transform.position.y + enemyJumpThreshold) 
        {
            rb.AddForce(new Vector2(0f, enemyJumpForce), ForceMode2D.Impulse);
        }
        rb.linearVelocity = new Vector2(enemySpeed, 0f) * (target - (Vector2)transform.position).normalized;
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
                Debug.Log("Player Detected!");
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
