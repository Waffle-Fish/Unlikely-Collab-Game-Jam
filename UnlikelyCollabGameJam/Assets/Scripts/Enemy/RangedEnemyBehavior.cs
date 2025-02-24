using Unity.VisualScripting;
using UnityEngine;

public class RangedEnemyBehavior : EnemyBehavior
{

    public GameObject projectile;
    protected override void Awake()
    {
        base.Awake();
        // things to adjust?
        // adjust casyrays parameters: spreadAngle, numRays, rayLength
        rayLength = 20;
        // rb = GetComponent<Rigidbody2D>();
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

    protected override void Attack()
    {
        // stop moving
        rb.linearVelocityX = 0f;

        // add a wait timer for windup

        // shoot projectile at player
        GameObject projInstance = Instantiate(projectile, transform.position, Quaternion.identity);
        projInstance.GetComponent<Projectile>().SetVelocity(1f, 1f);

        // add a wait timer for cool down before continuing pursuit

        // return to pursue
        enemyState = EnemyStates.Pursue;
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
            Debug.DrawRay(origin, rayDirection * rayLength, Color.red, .1f);  // For visual debugging in the Scene view

            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                player = hit.collider.gameObject;
                return true;
            }
        }
        return false;
    }

}
