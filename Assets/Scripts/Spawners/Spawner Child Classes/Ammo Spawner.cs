using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AmmoSpawner : Spawner
{
    [Header("Ammo Prefabs")]
    [SerializeField] private GameObject[] ammoPickupPrefabs;

    [Header("Spawn Count")]
    [SerializeField] private int minAmmoToSpawn = 5;
    [SerializeField] private int maxAmmoToSpawn = 10;

    [Header("Respawn")]
    [SerializeField] private float respawnTime = 10f;

    private readonly List<GameObject> activePickups = new();
    private int targetSpawnCount;

    protected override void Start()
    {
        base.Start();

        // Decide how many ammo pickups to spawn this round
        targetSpawnCount = Random.Range(minAmmoToSpawn, maxAmmoToSpawn + 1);
    }

    protected override void Spawn()
    {
        if (ammoPickupPrefabs == null || ammoPickupPrefabs.Length == 0)
        {
            Debug.LogWarning("AmmoSpawner: No ammo prefabs assigned.");
            return;
        }

        // First: guarantee one of each ammo type
        if (activePickups.Count == 0)
        {
            foreach (GameObject prefab in ammoPickupPrefabs)
            {
                SpawnPickup(prefab);
            }
        }

        // Then: spawn random ammo until target count reached
        while (activePickups.Count < targetSpawnCount)
        {
            GameObject randomPrefab =
                ammoPickupPrefabs[Random.Range(0, ammoPickupPrefabs.Length)];

            SpawnPickup(randomPrefab);
        }
    }

    private void SpawnPickup(GameObject prefab)
    {
        Transform spawnPoint = GetRandomSpawnPoint();

        GameObject pickup = Instantiate(
            prefab,
            spawnPoint.position,
            prefab.transform.rotation
        );

        activePickups.Add(pickup);

        StartCoroutine(WatchPickup(pickup));
    }

    private IEnumerator WatchPickup(GameObject pickup)
    {
        // Wait until pickup is destroyed (picked up)
        yield return new WaitUntil(() => pickup == null);

        activePickups.Remove(pickup);

        yield return new WaitForSeconds(respawnTime);

        Spawn();
    }
}
