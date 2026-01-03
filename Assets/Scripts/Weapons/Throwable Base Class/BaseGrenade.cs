using UnityEngine;
using System.Collections;

public abstract class BaseGrenade : MonoBehaviour
{
    [Header("Grenade Settings")]
    public float damage = 50f;
    public float radius = 5f;
    public LayerMask enemyLayer;

    protected bool hasExploded = false;

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (!hasExploded)
        {
            OnImpact();
        }
    }

    protected abstract void OnImpact(); // Different for each grenade type
}
