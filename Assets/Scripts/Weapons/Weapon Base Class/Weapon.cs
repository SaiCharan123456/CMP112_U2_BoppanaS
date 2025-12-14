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

        SetupStats();

        currentMagazine = magazineCapacity;
        currentAmmo = maxAmmo;

    }

    public Sprite GetIcon() => weaponIcon;

    public GameObject GetPickupPrefab() => pickupPrefab;

    protected virtual void SetupStats() { }

    public void SetReferences(Transform sharedFirePoint, Animator sharedAnimator) 
    { 
        firePoint = sharedFirePoint; 
        animator = sharedAnimator; 
    }

    public virtual void TryShoot()
    {
        if (Time.time >= nextFireTime && currentMagazine > 0 && !isReloading)
        {
            Shoot();
            nextFireTime = Time.time + 1f / fireRate;
            animator.SetTrigger("Fire");
        }
    }

    public virtual void TryReload()
    {
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

        RaycastHit hit;

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

            if (impactEffect != null)
            {
                GameObject impactGO = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impactGO, 2f);
            }

        }

        currentMagazine--;

    }


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
