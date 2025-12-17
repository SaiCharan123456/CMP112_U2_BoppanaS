using UnityEngine;
using System.Collections;

public abstract class Spawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] protected Transform[] spawnPoints;
    [SerializeField] protected bool spawnOnStart = true;
    [SerializeField] protected float spawnDelay = 0f;

    protected virtual void Start()
    {
        if (spawnOnStart)
            StartCoroutine(SpawnRoutine());
    }

    protected IEnumerator SpawnRoutine()
    {
        if (spawnDelay > 0f)
            yield return new WaitForSeconds(spawnDelay);

        Spawn();
    }


    // Get a random spawn point from the array
    protected Transform GetRandomSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
            return transform;

        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }


    // Abstract method to be implemented by child classes for spawning logic
    protected abstract void Spawn();
}