using System;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class RangedEnemyBehavior : EnemyBehavior
{

    [Header("Projectile Settings")]
    public GameObject projectile;
    [SerializeField]
    private float projectileSpeed = 15f;
    [SerializeField]
    private float projectileDamage = 5f;
    protected override void Awake()
    {
        base.Awake();
        // things to adjust?
        rayLength = 14;
        enemyAttackCoolDown = 2f;
    }

    protected override void Pursue()
    {
        // instead of go directly at player, find spot with line of site
        // attack when in line of site / then go back to pursue
        // difficult to keep ranged enemy at range?

        // Pursue
        NavigateToTarget();

        target = (Vector2)player.transform.position;

        if (isPlayerInFOV() && rb.linearVelocityY == 0)
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

    // FIXME: Might revert back to this
    protected override void Attack()
    {
        // add a wait timer for windup
        if (enemyAttackTimer <= 0)
        {
            // stop moving
            rb.linearVelocityX = 0f;
            // ANIMATION - Ranged Attack goes here
            // shoot projectile at player
            GameObject projInstance = Instantiate(projectile, transform.position, Quaternion.identity);

            Vector2 direction = (player.transform.position - transform.position).normalized;
            Debug.Log("Firing in Direction: "+direction);

            projInstance.GetComponent<Projectile>().SetVelocity(
                direction.x*projectileSpeed,
                direction.y*projectileSpeed);
            projInstance.GetComponent<Projectile>().SetProjectileDamage(projectileDamage);
            enemyAttackTimer = enemyAttackCoolDown;
        }

        //TODO: add lifetime to projectiles, add a timer to projectiles where in the first ~.5 seconds
        // it will not collide and be destroyed. 
        // 
        // remove gravity and have projectiles shoot directly at player OR
        // make adjustments to current direction of projectiles to account for scenarios like on the same level
        //
        // small randomizations in projectile speed / direction 


        // return to pursue if far enough from player
        if(Vector2.Distance(player.transform.position, transform.position) > 10f)
        {
            enemyState = EnemyStates.Pursue;
        }
    }

//    // TODO: if this implementation works better -> will have to switch detection layer before and after to "default" and back to "player"
//     private bool isPlayerInDirectLineOfSight()
// {
//     // Adjust the origin if needed (e.g., to represent the enemy's "eyes")
//     Vector2 origin = transform.position + new Vector3(0f, 0.5f, 0f);
    
//     // Determine the direction and distance from the enemy to the player
//     Vector2 direction = (player.transform.position - (Vector3)origin).normalized;
//     float distanceToPlayer = Vector2.Distance(origin, player.transform.position);

//     // Cast a ray toward the player using a layer mask (detectionLayer) that should include obstacles and the player
//     RaycastHit2D hit = Physics2D.Raycast(origin, direction, distanceToPlayer, detectionLayer);

//     // Return true only if the first collider hit is the player
//     if (hit.collider == null)
//     {
//         return true;
//     }
//     return false;
// }

    private bool isPlayerInFOV() // could refactor CastRays instead to limit amount of code
    {
        Vector2 origin = transform.position + new Vector3(0f, .5f, 0f);
        float spreadAngle = 360f;
        int numberOfRays = 40;
        float rayLength = 20f;
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
                player = hit.collider.gameObject;
                return true;
            }
        }
        return false;
    }

}
