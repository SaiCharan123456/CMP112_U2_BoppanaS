using UnityEngine;


[System.Serializable]
public class GhostSpawnArea
{
    public string ghostTypeName;        // For reference
    public GameObject ghostPrefab;      // Prefab to spawn
    public BoxCollider[] spawnAreas;    // Areas to spawn this type
    public int maxGhosts = 5;           // Max ghosts for this type
}
