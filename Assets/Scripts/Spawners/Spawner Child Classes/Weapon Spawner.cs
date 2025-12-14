using UnityEngine;

public class WeaponSpawner : Spawner
{
    [System.Serializable]
    public class WeaponSpawnEntry
    {
        public GameObject pickupPrefab;
        public MonoBehaviour weapon;
    }

    [SerializeField] private WeaponSpawnEntry[] weapons;

    protected override void Start()
    {
        Spawn();
    }

    protected override void Spawn()
    {
        if (weapons == null || weapons.Length == 0)
            return;

        foreach (var entry in weapons)
        {
            Transform spawnPoint = GetRandomSpawnPoint();

            GameObject pickup = Instantiate(
                entry.pickupPrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

            PickUpWeapon pickupScript =
                pickup.GetComponent<PickUpWeapon>();

            if (pickupScript != null)
            {
                pickupScript.SetWeaponToUnlock(entry.weapon);
            }
        }
    }
}
