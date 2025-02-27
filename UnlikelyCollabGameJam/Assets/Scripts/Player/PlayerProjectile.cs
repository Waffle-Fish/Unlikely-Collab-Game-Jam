using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerProjectile : MonoBehaviour
{
    [SerializeField] float damageVal;
    [SerializeField] float lifeSpan;

    void OnEnable()
    {
        StartCoroutine(ProcessLifeSpan());
    }

    private IEnumerator ProcessLifeSpan()
    {
        float finalTime = Time.time + lifeSpan;
        while (Time.time < finalTime)
        {
            yield return null;
        }
        gameObject.SetActive(false);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.CompareTag("Enemy")) {
            collision.gameObject.GetComponent<EnemyBehavior>().TakeDamage(damageVal);
        }
        gameObject.SetActive(false);
    }
}
