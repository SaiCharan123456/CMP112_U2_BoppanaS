using UnityEngine;

public class Pistol : Weapon
{
    // Define pistol-specific stats
    protected override void SetupStats()
    {
        weaponName = "Pistol";
        fireRate = 1f;
        damage = 10f;
        magazineCapacity = 6;
        maxAmmo = 20;
        reloadTime = 1.5f;
        fireRange = 50f;
    }
}
