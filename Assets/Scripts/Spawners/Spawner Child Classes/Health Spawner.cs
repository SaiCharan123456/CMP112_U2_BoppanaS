using UnityEngine;

public class HealthSpawner : MonoBehaviour 
{
    [Header("Health Pickup Prefab")]
    [SerializeField] private GameObject healthPickupPrefab;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Spawn Control")]
    [SerializeField] private float spawnInterval = 10f;

    private float timer;
    private GameObject[] currentHealthPickups; // Track pickup per spawn point

    private void Awake()
    {
        // Initialize tracking array matching spawn points length
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            currentHealthPickups = new GameObject[spawnPoints.Length];
        }
    }

    private void Update()
    {
        if (healthPickupPrefab == null || spawnPoints == null || spawnPoints.Length == 0)
            return;

        timer += Time.deltaTime;

        // For each spawn point, ensure at least one pickup exists. Spawn when missing and interval passed.
        if (timer >= spawnInterval)
        {
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                if (currentHealthPickups[i] == null)
                {
                    Transform sp = spawnPoints[i];
                    currentHealthPickups[i] = Instantiate(healthPickupPrefab, sp.position, sp.rotation);
                }
            }
            timer = 0f;
        }
               
    }
}
