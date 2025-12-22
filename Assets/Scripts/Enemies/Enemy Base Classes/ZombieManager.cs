using UnityEngine;

public class ZombieManager : MonoBehaviour
{
    public static ZombieManager Instance { get; private set; }

    [Header("Global Zombie Limit")]
    [SerializeField] private int maxZombies = 30;
    public int MaxZombies => maxZombies;

    public int CurrentZombies { get; private set; }

    [Header("Scene References")]
    [SerializeField] private Transform player;
    [SerializeField] private GameObject[] wayPoints;

    public Transform Player => player;
    public GameObject[] WayPoints => wayPoints;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // returns true if we can spawn more zombies
    public bool CanSpawn(int count = 1)
    {
        return CurrentZombies + count <= maxZombies;
    }

    public void RegisterZombie()
    {
        CurrentZombies++;
    }

    public void UnregisterZombie()
    {
        CurrentZombies = Mathf.Max(0, CurrentZombies - 1);
    }
}
