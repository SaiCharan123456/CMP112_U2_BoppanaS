using UnityEngine;
using System.Collections;

public class WeaponPickUp : PickUp {

    [SerializeField] private MonoBehaviour weaponToUnlock;

    private Collider weaponCollider; // Reference to the weapon's collider

    protected override void Awake()
    {
        StartCoroutine(WaitAfterSpawning());

        // Get the collider to control trigger state
        weaponCollider = GetComponent<Collider>();
    }

    private IEnumerator WaitAfterSpawning()
    {
        yield return new WaitForSeconds(2);
        GetComponent<Collider>().isTrigger = true;
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<Rigidbody>().useGravity = false;
        
        // Ensure the weapon is slightly above ground
        var pos = transform.position;
        pos.y = 0.5f;
        transform.position = pos;
    }


    // Define pick-up behavior
    protected override void OnPickUp(GameObject player) {

        // Get the PlayerWeaponController component from the player

        PlayerWeaponController controller = player.GetComponent<PlayerWeaponController>();

        if (controller == null || weaponToUnlock == null)
        {
            Debug.Log("Weapon pickup failed.");
            return;
        }

        // Attempt to pick up the weapon

        bool pickedUp = controller.PickUpWeapon(weaponToUnlock);
        Debug.Log("Pickup result: " + pickedUp);

        if (pickedUp)
        {
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Weapon pickup failed.");
        }

    }


    // Method to set the weapon to unlock
    public void SetWeapon(MonoBehaviour weapon) { 
        weaponToUnlock = weapon;
    }


    // Method to set the collider's trigger state
    public void SetTriggerState(bool isTrigger)
    {
        if (weaponCollider != null)
        {
            weaponCollider.isTrigger = isTrigger;
        }
    }


    // Method to enable trigger after spawning
    public void EnableTriggerAfterSpawn()
    {
        if (weaponCollider != null)
        {
            weaponCollider.isTrigger = true;  // Ensure it's set to true after spawning
        }
    }
}