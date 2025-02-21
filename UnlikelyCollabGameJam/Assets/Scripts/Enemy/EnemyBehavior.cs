using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
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
    Rigidbody2D rigidbody2D;

    [Header("Vision Settings")]
    public float rayLength = 10f;         // How far each ray will check for the player.
    public float spreadAngle = 45f;       // Total angle (in degrees) of the vision cone.
    public int numberOfRays = 10;         // How many rays to cast across the cone.

    [Header("Detection Settings")]
    public LayerMask detectionLayer;      // Use this to limit the rays to specific layers (e.g., the player).

    private Vector2 forwardDir;

    private enum enemyStates {Patrol, Pursue, Attack, Retreat};

    public List<GameObject> patrolPoints = new();

    private Vector2 target;

    private enemyStates enemyState;

    private float enemySpeed = 1f;
    private float enemyJumpForce = 5f;
    private float enemyHealth = 100f;

    void Awake()
    {
        SelectRandomPatrolPointFromList();
        forwardDir = transform.right;
        rigidbody2D = GetComponent<Rigidbody2D>();
        enemyState = enemyStates.Patrol;
    }

    private void SelectRandomPatrolPointFromList()
    {
        int randPatrolPointIndex = Random.Range(0, patrolPoints.Count);
        target = new Vector2(patrolPoints[randPatrolPointIndex].transform.position.x, patrolPoints[randPatrolPointIndex].transform.position.y);
    }

    void Update()
    {
        SetForwardDirection();

        if (enemyState == enemyStates.Patrol)
        {
            //Patrol
            // add movement left and right (maybe up and down? random jumping?)
            if (Math.Abs(rigidbody2D.linearVelocityX) < 3f) // 3 is arbitrary number idk
            {
                rigidbody2D.AddForce(new Vector2(enemySpeed, 0f) * target.normalized, ForceMode2D.Force);
            }

            // if near a patrol point, select a new one
            if(transform.position.x < target.x+1 && transform.position.x > target.x-1 &&
               transform.position.y < target.y+1 && transform.position.y > target.y-1)
               {
                    SelectRandomPatrolPointFromList();
               }

            

            bool CanSeePlayer = CastRays();

            if (CanSeePlayer)
            {
                // enemyState = enemyStates.Pursue;
            }
        }
        else if (enemyState == enemyStates.Pursue)
        {
            // Pursue
        }
        else if (enemyState == enemyStates.Attack)
        {
            // Attack
        }
        else if (enemyState == enemyStates.Retreat)
        {
            // RETREATTT
        }

    }

    private void SetForwardDirection()
    {
        if (Math.Abs(rigidbody2D.linearVelocity.x) > 0.1)
        {
            forwardDir = rigidbody2D.linearVelocity.x > 0 ? transform.right : -transform.right;
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
                return true;
            }
        }
        return false;
    }



    
}
