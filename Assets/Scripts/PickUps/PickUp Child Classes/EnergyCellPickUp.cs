using UnityEngine;

public class EnergyCellPickUp : PickUp
{
    protected override void OnPickUp(GameObject player)
    {
        GameManager.energyCells += 1;

        Destroy(gameObject);
    }
}
