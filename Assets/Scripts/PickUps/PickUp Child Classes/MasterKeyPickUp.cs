using UnityEngine;

public class MasterKeyPickUp : PickUp
{
    protected override void OnPickUp(GameObject player)
    {
        GameManager.masterKey += 1;

        Destroy(gameObject);
    }
}
