using UnityEngine;

public class Sniper : Weapon
{
    protected override void SetupStats()
    {
        weaponName = "Sniper";
        fireRate = 1f;
        damage = 50f;
        magazineCapacity = 5;
        maxAmmo = 25;
        reloadTime = 3f;
        fireRange = 200f;
    }
}
