using UnityEngine;

public class Fireball : Projectile
{
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) {
            collision.GetComponent<PlayerHealth>().TakeDamage(projectileDamage);
        } 
        gameObject.SetActive(false);
    }
}
