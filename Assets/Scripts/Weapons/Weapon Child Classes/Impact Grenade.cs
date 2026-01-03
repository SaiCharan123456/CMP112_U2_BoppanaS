using UnityEngine;

public class ImpactGrenade : BaseGrenade
{
    [Header("VFX")]
    public GameObject explosionEffect;

    protected override void OnImpact()
    {
        hasExploded = true;

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

        Destroy(gameObject); // Destroy grenade
    }
}
