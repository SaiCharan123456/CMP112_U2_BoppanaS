using UnityEngine;
using System.Collections;          
using System.Collections.Generic;  

public class WeaponSpawner : Spawner
{
    // Class to hold weapon spawn entry data
    [System.Serializable]
    public class WeaponSpawnEntry
    {
        public GameObject pickupPrefab;
        public MonoBehaviour weapon;
    }

    [SerializeField] private WeaponSpawnEntry[] weapons;
    [SerializeField] private int maxItemsToSpawn = 5;  // Max number of items to spawn at once
    [SerializeField] private float spawnSpacing = 1.0f; // Spacing between items at the same spawn point

    protected override void Start()
    {
        Spawn();
    }


    // Implement the Spawn method to spawn weapon pickups
    protected override void Spawn()
    {
        if (weapons == null || weapons.Length == 0)
            return;

        // Loop through each weapon spawn entry
        foreach (var entry in weapons)
        {
            
            int numberOfItemsToSpawn = Random.Range(0, maxItemsToSpawn + 1);
            numberOfItemsToSpawn = Mathf.Max(numberOfItemsToSpawn, 1); // Ensures at least 1 item is spawned

            // Tracks used spawn points for each weapon to avoid spawning at the same point
            HashSet<Transform> usedSpawnPoints = new HashSet<Transform>();

            // If number of spawn points is less than the total number of items to spawn,
            // spawn multiple items at the same spawn points
            for (int i = 0; i < numberOfItemsToSpawn; i++)
            {
                Transform spawnPoint = GetRandomSpawnPoint();

                // Ensures the spawn point has not been used already
                while (usedSpawnPoints.Contains(spawnPoint))
                {
                    spawnPoint = GetRandomSpawnPoint();
                }

                // Adds the spawn point to the used set
                usedSpawnPoints.Add(spawnPoint);

                // Adds random offset for spawn spacing to avoid overlap at the same spawn point
                Vector3 spawnOffset = new Vector3(
                    Random.Range(-spawnSpacing, spawnSpacing),
                    0f, 
                    Random.Range(-spawnSpacing, spawnSpacing)
                );

                GameObject pickup = Instantiate(
                    entry.pickupPrefab,
                    spawnPoint.position + spawnOffset,
                    spawnPoint.rotation
                );

                WeaponPickUp pickupScript = pickup.GetComponent<WeaponPickUp>();

                if (pickupScript != null)
                {
                    pickupScript.SetWeapon(entry.weapon);
                    pickupScript.SetTriggerState(false); // Initially, set collider to non-trigger
                    StartCoroutine(WaitAfterSpawning()); // Optional wait time

                    pickupScript.EnableTriggerAfterSpawn(); // Enable trigger after spawning
                }
                else
                {
                    Debug.LogWarning("PickUpWeapon script is missing on " + entry.pickupPrefab.name);
                }
            }
        }
    }

    private IEnumerator WaitAfterSpawning()
    {
        yield return new WaitForSeconds(2);
    }
}
