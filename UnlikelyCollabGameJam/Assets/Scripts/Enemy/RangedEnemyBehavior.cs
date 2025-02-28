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
    [SerializeField]
    private Transform projectileSpawnPos;
    protected override void Awake()
    {
        base.Awake();
        rayLength = 14;
        enemyAttackCoolDown = 2f;
        pursueThreshold = 10f;
    }

    protected override void Pursue()
    {
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
    protected override void Attack()
    {
        // cooldown timer
        if (enemyAttackTimer <= 0)
        {
            // stop moving
            rb.linearVelocityX = 0f;
            // can add another timer here for a "windup"
            // ANIMATION - Ranged Attack goes here
            base.animator.SetTrigger("Attack");
            // shoot projectile at player
            GameObject projInstance = Instantiate(projectile, projectileSpawnPos.position, Quaternion.identity);

            Vector2 direction = (player.transform.position - transform.position).normalized;
            Debug.Log("Firing in Direction: "+direction);

            projInstance.GetComponent<Projectile>().SetVelocity(
                direction.x*projectileSpeed,
                direction.y*projectileSpeed);
            projInstance.GetComponent<Projectile>().SetProjectileDamage(projectileDamage);
            enemyAttackTimer = enemyAttackCoolDown;
        }

        // return to pursue if far enough from player
        if(Vector2.Distance(player.transform.position, transform.position) > pursueThreshold)
        {
            enemyState = EnemyStates.Pursue;
        }
    }

    

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
