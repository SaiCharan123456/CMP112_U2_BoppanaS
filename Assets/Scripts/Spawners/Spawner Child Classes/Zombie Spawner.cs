using UnityEngine;
using System.Collections;

public class ZombieSpawnArea : Spawner
{
    [Header("Zombie Prefabs")]
    [SerializeField] private GameObject[] zombiePrefabs;

    [Header("Spawn Area")]
    [SerializeField] private Vector3 areaSize = new Vector3(10f, 0f, 10f);
    [SerializeField] private bool drawGizmos = true;

    [Header("Spawn Control")]
    [SerializeField] private int maxZombiesInThisArea = 8;
    [SerializeField] private int zombiesPerSpawn = 2;
    [SerializeField] private float spawnInterval = 5f;

    private int currentZombiesInArea;

    protected override void Start()
    {
        base.Start();
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            Spawn();
        }
    }

    // Spawn zombies considering area and global limits
    protected override void Spawn()
    {
        if (zombiePrefabs.Length == 0)
            return;

        // Check area limit
        int areaSpaceLeft = maxZombiesInThisArea - currentZombiesInArea;
        if (areaSpaceLeft <= 0)
            return;

        // Check global limit
        int globalSpaceLeft =
            ZombieManager.Instance.MaxZombies - ZombieManager.Instance.CurrentZombies;

        if (globalSpaceLeft <= 0)
            return;

        // Determine how many to spawn
        int spawnCount = Mathf.Min(
            zombiesPerSpawn,
            areaSpaceLeft,
            globalSpaceLeft
        );

        // Spawn the zombies
        for (int i = 0; i < spawnCount; i++)
        {
            SpawnOneZombie();
        }
    }

    // Spawn a single zombie at a random position in the area
    private void SpawnOneZombie()
    {
        Vector3 pos = GetRandomPointInArea();
        GameObject prefab =
            zombiePrefabs[Random.Range(0, zombiePrefabs.Length)];

        GameObject zombie = Instantiate(prefab, pos, Quaternion.identity);

        // Register to THIS area
        ZombieAreaTracker tracker = zombie.AddComponent<ZombieAreaTracker>();
        tracker.Init(this);

        currentZombiesInArea++;
    }

    public void UnregisterZombie()
    {
        currentZombiesInArea = Mathf.Max(0, currentZombiesInArea - 1);
    }

    // Get a random point within the defined area
    private Vector3 GetRandomPointInArea()
    {
        Vector3 center = transform.position;

        // Random point within area size
        float x = Random.Range(-areaSize.x / 2f, areaSize.x / 2f);
        float z = Random.Range(-areaSize.z / 2f, areaSize.z / 2f);

        return new Vector3(center.x + x, center.y, center.z + z);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, areaSize);
    }
}