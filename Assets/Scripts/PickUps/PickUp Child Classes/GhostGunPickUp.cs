using UnityEngine;

public class GhostGunPickUp : PickUp
{

    [SerializeField] private PlayerGhostWeaponController ghostController;


    protected override void OnPickUp(GameObject player)
    {            

        if (ghostController == null)
        {
            Debug.LogWarning("GhostWeaponController not found!");
            return;
        }

        ghostController.UnlockGhostGun();

        GameManager.hasGhostGun = true;

        // Force switch back to normal player after pickup
        //PlayerManager.Instance.SwitchToNormalPlayer();

        Destroy(gameObject);
    }
}
