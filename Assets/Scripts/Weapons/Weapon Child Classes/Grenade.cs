using UnityEngine;
using System.Collections;

public class Grenade : ThrowableWeapon
{
    [Header("Grenade Prefab")]
    public GameObject grenadePrefab;

    public override void TryUse()
    {
        if (currentCount <= 0)
            return;

        animator?.SetTrigger("Fire");
        StartCoroutine(ReleaseAfterDelay(1f));
    }


    // Delay the release to sync with animation
    private IEnumerator ReleaseAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Release();
    }

    // Execute the grenade throw
    public override void Release()
    {
        if (!throwPoint || !grenadePrefab || !cameraTransform)
            return;

        // Spawn grenade
        GameObject grenade = Instantiate(
            grenadePrefab,
            throwPoint.position,
            throwPoint.rotation
        );

        // Apply force to throw
        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        rb.AddForce(cameraTransform.forward * throwForce, ForceMode.VelocityChange);

        currentCount--;

        
        if (currentCount <= 0)
        {
            Debug.Log("Grenade depleted → notifying controller");
            OnDepleted?.Invoke(this);
        }
    }
}
