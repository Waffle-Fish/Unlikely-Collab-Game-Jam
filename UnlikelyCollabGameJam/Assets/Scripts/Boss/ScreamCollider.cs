using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ScreamCollider : MonoBehaviour
{
    PlayerHealth playerHealth;
    public float damage;

    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.name + " has entered");
    }
    
    private void OnTriggerStay2D(Collider2D other) {
        Debug.Log("Fish");
        // if (!collision.CompareTag("Player")) return;
        other.GetComponent<PlayerHealth>();
        playerHealth.TakeDamage(damage * Time.deltaTime);
    }
}
