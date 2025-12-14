using UnityEngine;


public class PickUpWeapon : MonoBehaviour
{

    [SerializeField] private MonoBehaviour weaponToUnlock;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetWeaponToUnlock(MonoBehaviour weapon)
    {
        weaponToUnlock = weapon;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Trigger entered by: " + other.name);

            PlayerWeaponController controller = other.GetComponent<PlayerWeaponController>();

            if (controller == null)
                return;

            bool pickedUp = controller.PickUpWeapon(weaponToUnlock);
            Debug.Log("Pickup result: " + pickedUp);

            if (pickedUp)
            {
                Destroy(gameObject);
            }
        }
    }

}
