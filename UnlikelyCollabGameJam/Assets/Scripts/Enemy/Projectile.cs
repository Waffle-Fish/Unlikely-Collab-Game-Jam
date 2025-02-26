using UnityEngine;

public class Projectile : MonoBehaviour
{
    Rigidbody2D rb;
    [SerializeField]
    private float lifetime = 3f;

    [SerializeField]
    protected float projectileDamage = 5f;
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
        projectileDamage = damage;
    }

    public void SetLifeTime(float time)
    {
        lifetime = time;
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) {
            collision.GetComponent<PlayerHealth>().TakeDamage(projectileDamage);
            gameObject.SetActive(false);
        } 
    }
}
