using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class Weapon : MonoBehaviour
{

    [Header("Weapon Info")]
    [SerializeField] protected string weaponName;
    [SerializeField] protected Sprite weaponIcon;
    [SerializeField] private GameObject pickupPrefab;


    [Header("Shooting")]
    [SerializeField] protected Transform firePoint;
    [SerializeField] protected float fireRate = 10f;
    [SerializeField] protected float fireRange = 100f;
    [SerializeField] protected float damage = 5f;
    [SerializeField] protected float impactForce = 30f;

    [Header("Ammo")]
    [SerializeField] protected int magazineCapacity = 30;
    [SerializeField] protected int maxAmmo = 120;
    [SerializeField] protected float reloadTime = 3f;

    [Header("Ammo Type")]
    [SerializeField] private AmmoType ammoType;
    public AmmoType AmmoType => ammoType;

    [Header("Animation")]
    [SerializeField] protected Animator animator;

    [Header("Audio")]
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected AudioClip shootClip;

    [Header("Effects")]
    [SerializeField] protected ParticleSystem muzzleFlash;
    [SerializeField] protected GameObject impactEffect;

    protected int currentMagazine;
    protected int currentAmmo;
    protected float nextFireTime;
    protected bool isReloading;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Awake()
    {
        // Setup weapon stats

        SetupStats();

        // Initialize ammo

        currentMagazine = magazineCapacity;
        currentAmmo = 0;

    }

    public Sprite GetIcon() => weaponIcon;

    public GameObject GetPickupPrefab() => pickupPrefab;

    protected virtual void SetupStats() { }


    public bool CanReceiveAmmo() => currentAmmo < maxAmmo;

    public void AddAmmo(int amount)
    {
        currentAmmo = Mathf.Clamp(currentAmmo + amount, 0, maxAmmo);
        Debug.Log($"{weaponName} received ammo. Current: {currentAmmo}");
    }

    public void SetReferences(Transform sharedFirePoint, Animator sharedAnimator) 
    {
        // Set shared references from player weapon controller
        firePoint = sharedFirePoint; 
        animator = sharedAnimator; 
    }


    // Try to shoot the weapon
    public virtual void TryShoot()
    {

        // Shoot if fire rate allows and has ammo
        if (Time.time >= nextFireTime && currentMagazine > 0 && !isReloading)
        {
            Shoot();
            nextFireTime = Time.time + 1f / fireRate;
            animator.SetTrigger("Fire");
        }
    }

    public virtual void TryReload()
    {

        // Start reloading if not full and has ammo
        if (currentMagazine < magazineCapacity && currentAmmo > 0 && !isReloading)
        {
            StartCoroutine(Reload());
            animator.SetTrigger("Reload");
        }
    }

    protected virtual void Shoot()
    {
        // Muzzle flash
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        // Audio
        if (audioSource != null && shootClip != null)
        {
            audioSource.PlayOneShot(shootClip);
        }

        Debug.Log("Shooting...");

        // Raycast hit info
        RaycastHit hit;

        // Raycast to detect hit
        if (Physics.Raycast(firePoint.position, firePoint.forward, out hit, fireRange))
        {
            Debug.Log("Hit: " + hit.transform.name);

            //Target target = hit.transform.GetComponent<Target>();

            //if (target != null)
            //{
            //    target.TakeDamage(damage);
            //}

            //if (hit.rigidbody != null)
            //{
            //    hit.rigidbody.AddForce(-hit.normal * impactForce);
            //}


            // Impact effect
            if (impactEffect != null)
            {
                // Create impact effect at hit point
                GameObject impactGO = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impactGO, 2f);
            }

        }

        // Decrease ammo
        currentMagazine--;

    }


    // Reloading coroutine
    protected virtual IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Reloading...");

        int ammoToReload = Mathf.Min(magazineCapacity - currentMagazine, currentAmmo);

        yield return new WaitForSeconds(reloadTime);

        currentMagazine += ammoToReload;
        currentAmmo -= ammoToReload;

        isReloading = false;
    }


}
