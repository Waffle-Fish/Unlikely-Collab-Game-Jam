using UnityEngine;

public class FlyingEnemyBehavior : EnemyBehavior
{
    
    private float wingFlapCoolDown = .5f;
    private float wingFlapTimer = 0f;

    protected override void Patrol()
    {
        //Patrol
        NavigateToTarget();

        if (isAtTarget())
        {
            target = pm.GetNextPatrolPointInPath((Vector2)transform.position);
        }
        // if target is directly below me -> get next point in path
        // if (target.y < transform.position.y && transform.position.x < target.x + 2f && transform.position.x > target.x - 2f)
        // {
        //     target = pm.GetNextPatrolPointInPath((Vector2)transform.position);
        // }
        // else if (target.y > transform.position.y + 5.5f && transform.position.x < target.x + 2f && transform.position.x > target.x - 2f)
        // {
        //     target = pm.GetNextPatrolPointInPath((Vector2)transform.position);
        // }

        bool CanSeePlayer = CastRays();

        if (CanSeePlayer)
        {
            target = (Vector2)player.transform.position;
            enemyState = EnemyStates.Pursue;
        }
    }

    protected override void NavigateToTarget()
    {

        if(transform.position.y < target.y - 1f && wingFlapTimer <= 0)
        {
            rb.AddForceY(10f, ForceMode2D.Impulse);
            wingFlapTimer = wingFlapCoolDown;
        }
        wingFlapTimer -= Time.deltaTime;
        
        // ANIMATION - Walking left / right - use "forwardDir"
        if (enemyState == EnemyStates.Patrol)
        {
            // constant speed left / right
            rb.linearVelocityX = enemySpeed * Mathf.Sign(target.x - transform.position.x);
            // rb.linearVelocityY = enemySpeed * Mathf.Sign(target.y - transform.position.y);

        }
        else if (enemyState == EnemyStates.Pursue)
        {
            // slower speed when approaching player - makes it a little easier? can change
            rb.linearVelocityX = enemySpeed * (target - (Vector2)transform.position).normalized.x;
            // rb.linearVelocityY = enemySpeed * (target - (Vector2)transform.position).normalized.y;
        }
    }
}
