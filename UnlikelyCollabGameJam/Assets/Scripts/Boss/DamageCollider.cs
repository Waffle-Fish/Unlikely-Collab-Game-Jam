using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DamageCollider : MonoBehaviour
{
    public float DamageVal = 0;
    public float Cooldown = 0;
    float timer = 0;
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        collision.GetComponent<PlayerHealth>().TakeDamage(DamageVal);
    }
}
