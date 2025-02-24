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
        rayLength = 20;
        enemyAttackCoolDown = .5f;
    }

    protected override void Pursue()
    {
        // instead of go directly at player, find spot with line of site
        // attack when in line of site / then go back to pursue
        // difficult to keep ranged enemy at range?

        // Pursue
        NavigateToTarget();

        target = (Vector2)player.transform.position;

        if (isPlayerInFOV())
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
    // protected override void Attack()
    // {
    //     // add a wait timer for windup
    //     if (enemyAttackTimer <= 0)
    //     {
    //         // stop moving
    //         rb.linearVelocityX = 0f;
    //         // ANIMATION - Ranged Attack goes here
    //         // shoot projectile at player
    //         GameObject projInstance = Instantiate(projectile, transform.position, Quaternion.identity);

    //         Vector2 direction = (player.transform.position - transform.position).normalized;

    //         projInstance.GetComponent<Projectile>().SetVelocity(
    //             direction.x*projectileSpeed,
    //             direction.y*projectileSpeed + (player.transform.position.y - transform.position.y));
    //         enemyAttackTimer = enemyAttackCoolDown;
    //     }

    //     // return to pursue if far enough from player
    //     if(Vector2.Distance(player.transform.position, transform.position) > 10f)
    //     {
    //         enemyState = EnemyStates.Pursue;
    //     }
    // }

    protected override void Attack()
    {
        if (enemyAttackTimer <= 0)
        {
            // Stop moving during the windup
            rb.linearVelocityX = 0f;

            // Instantiate the projectile at the enemy's position
            GameObject projInstance = Instantiate(projectile, transform.position, Quaternion.identity);

            // Define start and target positions
            Vector2 startPos = transform.position;
            Vector2 targetPos = player.transform.position;

            // Compute horizontal and vertical distances
            float dx = targetPos.x - startPos.x;
            float dy = targetPos.y - startPos.y;

            // Launch speed (v₀)
            float v0 = projectileSpeed;

            // Get gravity (assumes Physics2D.gravity.y is negative)
            float g = Mathf.Abs(Physics2D.gravity.y);

            Vector2 velocity;  // This will hold our computed initial velocity

            // If dx is nearly zero, shoot straight up or down.
            if (Mathf.Abs(dx) < 0.001f)
            {
                float angle = (dy >= 0) ? Mathf.PI / 2 : -Mathf.PI / 2;
                velocity = new Vector2(0f, v0 * Mathf.Sin(angle));
            }
            else
            {
                float v0sqr = v0 * v0;
                float discriminant = v0sqr * v0sqr - g * (g * (dx * dx) + 2 * dy * v0sqr);

                if (discriminant < 0)
                {
                    // No valid solution exists with the given speed.
                    // Fallback: fire directly toward the player.
                    Vector2 directDir = (targetPos - startPos).normalized;
                    velocity = directDir * v0;
                    velocity.y += directDir.y;
                    // Debug.Log("Shooting directly at player");
                }
                else
                {
                    float sqrtDisc = Mathf.Sqrt(discriminant);
                    // Compute the lower (direct) trajectory angle.
                    float angle = Mathf.Atan((v0sqr - sqrtDisc) / (g * Mathf.Abs(dx)));

                    // Compute horizontal and vertical velocity components.
                    // (If you find the projectile overshoots horizontally by a factor of 2,
                    // try applying a correction factor to the x-component.)
                    bool applyXCorrection = true; // Set to true if you want to halve the horizontal component.
                    float vx = v0 * Mathf.Cos(angle) * Mathf.Sign(dx);
                    float vy = v0 * Mathf.Sin(angle);

                    if (applyXCorrection)
                    {
                        vx *= 0.5f; // This correction halves the horizontal component.
                    }

                    velocity = new Vector2(vx, vy);
                    // Debug.Log("Fancy math shot");

                }
            }

            // Pass the computed velocity to the projectile.
            projInstance.GetComponent<Projectile>().SetVelocity(velocity.x, velocity.y);
            projInstance.GetComponent<Projectile>().SetProjectileDamage(projectileDamage);

            enemyAttackTimer = enemyAttackCoolDown;
        }

        // Return to pursue state if the player is far away.
        if (Vector2.Distance(player.transform.position, transform.position) > 10f)
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
