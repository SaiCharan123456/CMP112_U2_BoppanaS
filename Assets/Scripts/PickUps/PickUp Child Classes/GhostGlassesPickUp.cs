using UnityEngine;

public class GhostGlassesPickUp : PickUp
{

    [SerializeField] private GameObject ghostGlasses;

    protected override void OnPickUp(GameObject player)
    {
        GameManager.hasGhostGlasses = true;

        ghostGlasses.SetActive(true);

        Destroy(gameObject);
    }
}
