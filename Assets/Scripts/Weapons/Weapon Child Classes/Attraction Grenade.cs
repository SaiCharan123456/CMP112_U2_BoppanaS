using UnityEngine;
using System.Collections;

public class AttractionGrenade : BaseGrenade
{
    [Header("Attraction Settings")]
    public float attractionDuration = 3f; // seconds to attract enemies
    public AudioSource attractionSound;

    [Header("VFX")]
    public GameObject explosionEffect;

    protected override void OnImpact()
    {
        hasExploded = true;
        StartCoroutine(AttractThenExplode());
    }

    private IEnumerator AttractThenExplode()
    {
        // Play sound to attract enemies
        if (attractionSound != null)
            attractionSound.Play();

        // Optional: enemies can have AI script that reacts to this sound
        yield return new WaitForSeconds(attractionDuration);

        // Show explosion VFX
        if (explosionEffect != null)
            Instantiate(explosionEffect, transform.position, Quaternion.identity);

        // Damage all enemies in range
        Collider[] hits = Physics.OverlapSphere(transform.position, radius, enemyLayer);
        foreach (Collider col in hits)
        {
            IDamageable enemy = col.GetComponentInParent<IDamageable>();
            if (enemy != null)
                enemy.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}
