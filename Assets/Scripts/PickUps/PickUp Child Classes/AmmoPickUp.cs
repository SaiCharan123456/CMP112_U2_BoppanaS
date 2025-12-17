using UnityEngine;

public class AmmoPickUp : PickUp
{
    [SerializeField] private AmmoType ammoType;
    [SerializeField] private int ammoAmount = 30;

    protected override void OnPickUp(GameObject player)
    {
        PlayerWeaponController controller =
            player.GetComponent<PlayerWeaponController>();

        if (controller == null)
            return;

        bool ammoAdded = controller.AddAmmo(ammoType, ammoAmount);

        if (ammoAdded)
        {
            Destroy(gameObject);
        }
    }
}
