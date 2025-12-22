using UnityEngine;

public class GhostSpawner : Spawner
{
    [Header("Ghost Spawn Areas")]
    [SerializeField] private GhostSpawnArea[] ghostAreas;

    [Header("Player Reference")]
    [SerializeField] private Transform playerTransform;

    // Spawn ghosts in defined areas
    protected override void Spawn()
    {
        foreach (var area in ghostAreas)
        {
            if (area.ghostPrefab == null || area.spawnAreas == null || area.spawnAreas.Length == 0)
                continue;

            int ghostsToSpawn = area.maxGhosts;

            for (int i = 0; i < ghostsToSpawn; i++)
            {
                // Pick a random spawn area
                BoxCollider spawnArea = area.spawnAreas[Random.Range(0, area.spawnAreas.Length)];

                // Get a random position inside that area
                Vector3 randomPos = GetRandomPointInBox(spawnArea);

                // Spawn the ghost
                GameObject ghost = Instantiate(area.ghostPrefab, randomPos, Quaternion.identity);

                // Initialize the ghost with player reference
                if (ghost.TryGetComponent(out Ghost ghostComp))
                {
                    ghostComp.Initialize(playerTransform);
                }
            }
        }
    }

    // Get a random point within a BoxCollider
    private Vector3 GetRandomPointInBox(BoxCollider box)
    {
        Vector3 center = box.center + box.transform.position;
        Vector3 size = box.size;

        float x = Random.Range(center.x - size.x / 2f, center.x + size.x / 2f);
        float y = center.y; // Keep Y same as center, or adjust if needed
        float z = Random.Range(center.z - size.z / 2f, center.z + size.z / 2f);

        return new Vector3(x, y, z);
    }
}
