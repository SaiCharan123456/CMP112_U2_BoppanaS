using UnityEngine;

public class Rifle : Weapon
{
    protected override void SetupStats()
    {
        weaponName = "Rifle";
        fireRate = 10f;
        damage = 25f;
        magazineCapacity = 30;
        maxAmmo = 300;
        reloadTime = 2f;
        fireRange = 100f;
    }
}
