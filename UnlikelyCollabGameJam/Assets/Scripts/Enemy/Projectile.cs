using UnityEngine;

public class Projectile : MonoBehaviour
{
    Rigidbody2D rb;
    void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
    }
    void Update()
    {
        // check for collision with someth ing -> call its take damage function
    }

    public void SetVelocity(float vx, float vy)
    {
        rb.linearVelocityX = vx;
        rb.linearVelocityY = vy;
    }
}
