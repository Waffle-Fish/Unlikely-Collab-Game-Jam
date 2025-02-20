using UnityEngine;

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

    [Header("Vision Settings")]
    public float rayLength = 10f;         // How far each ray will check for the player.
    public float spreadAngle = 45f;       // Total angle (in degrees) of the vision cone.
    public int numberOfRays = 10;         // How many rays to cast across the cone.

    [Header("Detection Settings")]
    public LayerMask detectionLayer;      // Use this to limit the rays to specific layers (e.g., the player).
    

    private enum enemyStates {Patrol, Pursue, Attack, Retreat};

    enemyStates enemyState;

    private float enemySpeed = 0f;
    private float enemyHealth = 100f;

    void Awake()
    {
        enemyState = enemyStates.Patrol;
    }

    void Update()
    {
        if (enemyState == enemyStates.Patrol)
        {
            //Patrol
            CastVisionRays();
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

    void CastVisionRays()
    {
        Vector2 origin = transform.position;
        // Assume the enemy faces to the right (i.e. transform.right is its forward direction)
        Vector2 forwardDir = transform.right;
        float halfAngle = spreadAngle / 2f;

        for (int i = 0; i < numberOfRays; i++)
        {
            // Interpolate angles from -halfAngle to +halfAngle
            float angle = Mathf.Lerp(-halfAngle, halfAngle, i / (float)(numberOfRays - 1));
            // Rotate the forward direction by the current angle
            Vector2 rayDirection = Quaternion.Euler(0, 0, angle) * forwardDir;

            // Cast a ray in the calculated direction
            RaycastHit2D hit = Physics2D.Raycast(origin, rayDirection, rayLength, detectionLayer);
            Debug.DrawRay(origin, rayDirection * rayLength, Color.red);  // For visual debugging in the Scene view

            // Check if we hit something, and if it has the "Player" tag
            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                Debug.Log("Player detected!");
                // You can add additional logic here (e.g., alert the enemy, change behavior, etc.)
            }
        }
    }



    
}
