using UnityEngine;

public abstract class Spawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] protected Transform[] spawnPoints;
    [SerializeField] protected bool spawnOnStart = true;
    [SerializeField] protected float spawnDelay = 0f;

    protected virtual void Start()
    {
        if (spawnOnStart)
        {
            if (spawnDelay > 0f)
                Invoke(nameof(Spawn), spawnDelay);
            else
                Spawn();
        }
    }

    protected Transform GetRandomSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
            return transform;

        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }

    protected abstract void Spawn();
}
