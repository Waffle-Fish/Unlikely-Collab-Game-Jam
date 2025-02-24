using UnityEngine;

public class Projectile : MonoBehaviour
{
    Rigidbody2D rb;

    [SerializeField]
    private float projectileDamgage = 5f;
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

    public void SetProjectileDamage(float damage)
    {
        projectileDamgage = damage;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) {
            collision.GetComponent<PlayerHealth>().TakeDamage(projectileDamgage);
        } 
        gameObject.SetActive(false);
    }
}
