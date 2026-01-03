using UnityEngine;

public class GhostGun : Weapon
{
    [Header("Ghost Gun")]
    [SerializeField] private GameObject soundWavePrefab;

    protected override void SetupStats()
    {
        weaponName = "Ghost Gun";
        magazineCapacity = 10;
        maxAmmo = 100;
        fireRate = 1f;
        reloadTime = 2f;
        damage = 100f;
        fireRange = 50f;
    }

    protected override void Awake()
    {
        SetupStats();

        currentMagazine = magazineCapacity;
        currentAmmo = maxAmmo;
    }

    protected override void Update()
    {
        ammoDisplay.text = currentMagazine.ToString();
        magazineDisplay.text = currentAmmo.ToString();
        Debug.Log($"Ghost Gun Ammo: {currentMagazine}/{currentAmmo}");
    }

    protected override void Shoot()
    {

        if (audioSource != null && shootClip != null)
            audioSource.PlayOneShot(shootClip);

        // Spawn sound wave in the direction of firePoint
        GameObject wave = Instantiate(soundWavePrefab, firePoint.position, firePoint.rotation);

        // Pass forward direction to bullet
        SoundWaveBullet bullet = wave.GetComponent<SoundWaveBullet>();
        if (bullet != null)
        {
            bullet.SetDirection(firePoint.forward);
        }

        currentMagazine--;
    }
}
