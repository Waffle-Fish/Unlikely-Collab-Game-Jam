using UnityEngine;

public class Projectile : MonoBehaviour
{
    Rigidbody2D rb;
    [SerializeField]
    private float lifetime = 3f;

    [SerializeField]
    private float projectileDamgage = 5f;
    [SerializeField]
    private float shotRandomness = 0.25f;
    void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
    }
    void Update()
    {
        lifetime -= Time.deltaTime;
        if(lifetime <= 0)
        {
            gameObject.SetActive(false);
        }
    }

    public void SetVelocity(float vx, float vy)
    {
        vx += Random.Range(-shotRandomness, shotRandomness);
        vy += Random.Range(-shotRandomness, shotRandomness);
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
            gameObject.SetActive(false);
        } 
    }
}
