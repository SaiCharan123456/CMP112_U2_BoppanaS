using UnityEngine;

public class ZombieAreaTracker : MonoBehaviour
{
    private ZombieSpawnArea area;

    // Initialize with the spawn area reference
    public void Init(ZombieSpawnArea spawnArea)
    {
        area = spawnArea;
    }

    // On destroy, inform the spawn area to decrement its count
    private void OnDestroy()
    {
        if (area != null)
            area.UnregisterZombie();
    }
}
