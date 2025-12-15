using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.EventSystems.EventTrigger;

public class PlayerWeaponController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform grenadeThrowPoint;
    [SerializeField] private Transform cameraTransform;

    [Header("UI")]
    [SerializeField] private List<WeaponSlotUI> uiSlots; // Assign 4 UI slots

    [Header("Weapons in hands")]
    [SerializeField] private List<Weapon> guns = new List<Weapon>();
    [SerializeField] private Grenade grenade;

    [Header("Drop Settings")]
    [SerializeField] private float dropForwardForce = 6f;
    [SerializeField] private float dropUpForce = 2f;
    [SerializeField] private Transform dropPoint;

    // Internal state
    private List<MonoBehaviour> weaponSlots = new List<MonoBehaviour>();
    private MonoBehaviour currentWeapon;
    private int currentSlotIndex = -1;


    private void Awake()
    {
        // Create empty slots
        for (int i = 0; i < 4; i++)
            weaponSlots.Add(null);

        // Setup guns (DO NOT add to slots)
        foreach (var gun in guns)
        {
            gun.SetReferences(firePoint, animator);
            gun.gameObject.SetActive(false);
        }

        // Setup grenade
        if (grenade != null)
        {
            grenade.Initialize(cameraTransform, animator, grenadeThrowPoint);
            grenade.gameObject.SetActive(false);
        }
    }


    // Select weapon slot
    public void SelectSlot(int slotIndex)
    {
        // Validate slot index
        if (slotIndex < 0 || slotIndex >= weaponSlots.Count) 
        { 
            return;
        }

        MonoBehaviour selected = weaponSlots[slotIndex];


        // If slot is empty, unequip current weapon
        if (currentWeapon == selected)
        {
            // Unequip current weapon
            SetWeaponActive(currentWeapon, false, 0);
            currentWeapon = null;
            currentSlotIndex = -1;
        }
        else
        {

            // Switch weapons
            SetWeaponActive(currentWeapon, false, 0);

            // Equip selected weapon
            currentWeapon = selected;
            currentSlotIndex = slotIndex;
            SetWeaponActive(currentWeapon, true, 1);
        }

        // Update UI
        UpdateUISlots();
    }


    // Activate or deactivate weapon and set animator layer weight
    private void SetWeaponActive(MonoBehaviour weapon, bool value, float weight)
    {
        if (weapon == null)
        {
            return;
        }

        weapon.gameObject.SetActive(value);
        animator.SetLayerWeight(animator.GetLayerIndex(weapon.gameObject.name), weight);
    }


    // Update UI to reflect current slot selection
    private void UpdateUISlots()
    {
        for (int i = 0; i < uiSlots.Count; i++)
        {
            uiSlots[i].SetSelected(i == currentSlotIndex);
        }
    }


    // Input handlers
    public void OnShoot(InputValue value)
    {
        if (!value.isPressed) return;

        if (currentWeapon is Weapon gun) gun.TryShoot();
        else if (currentWeapon is Grenade gren) gren.TryUse();
    }

    public void OnReload(InputValue value)
    {
        if (!value.isPressed) return;

        if (currentWeapon is Weapon gun) gun.TryReload();
    }

    public void OnDrop(InputValue value)
    {
        if (!value.isPressed)
            return;

        Debug.Log("Drop weapon input received");

        DropCurrentWeapon();
    }

    public void OnSlot1(InputValue value)
    {
        if (value.isPressed) SelectSlot(0);
    }

    public void OnSlot2(InputValue value)
    {
        if (value.isPressed) SelectSlot(1);
    }

    public void OnSlot3(InputValue value)
    {
        if (value.isPressed) SelectSlot(2);
    }

    public void OnSlot4(InputValue value)
    {
        if (value.isPressed) SelectSlot(3);
    }


    // Drop currently equipped weapon
    private void DropCurrentWeapon()
    {
        if (currentWeapon == null || currentSlotIndex < 0)
        {
            return;
        }

        
        MonoBehaviour weaponToDrop = currentWeapon;
        int slotIndex = currentSlotIndex;

        // 1. Unequip
        SetWeaponActive(weaponToDrop, false, 0);

        currentWeapon = null;
        currentSlotIndex = -1;

        // 2. Remove from slot
        weaponSlots[slotIndex] = null;

        // 3. Clear UI
        if (uiSlots.Count > slotIndex)
        {
            uiSlots[slotIndex].Clear();
        }
             

         // 4. Spawn pickup in world
         SpawnPickup(weaponToDrop);

         Debug.Log($"Dropped weapon: {weaponToDrop.name}");
    }


    // Spawn weapon pickup in the world
    private void SpawnPickup(MonoBehaviour weapon)
    {
        GameObject prefab = null;

        if (weapon is Weapon gun)
            prefab = gun.GetPickupPrefab();
        else if (weapon is Grenade gren)
            prefab = gren.GetPickupPrefab();

        if (prefab == null || dropPoint == null)
        {
            Debug.LogWarning("Pickup prefab or drop point is missing.");
            return;
        }
            

        // Add a small offset to avoid the weapon being stuck in the drop point
        Vector3 spawnPosition = dropPoint.position + new Vector3(0, 1, 0); // Slightly above

        // Instantiate the weapon prefab as a pickup
        GameObject pickup = Instantiate(prefab, spawnPosition, Quaternion.identity);

        //` Set weapon in pickup script
        WeaponPickUp pickupScript = pickup.GetComponent<WeaponPickUp>();

        if (pickupScript != null)
        {
            pickupScript.SetWeapon(weapon);
        }
        else
        {
            Debug.LogWarning("PickUpWeapon script is missing on " );
        }

        // Disable trigger during drop

        Collider weaponCollider = pickup.GetComponent<Collider>();

        if (weaponCollider != null)
        {
            weaponCollider.isTrigger = false; 
        }


        Rigidbody rb = pickup.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 force =
                cameraTransform.forward * dropForwardForce +
                Vector3.up * dropUpForce;

            rb.AddForce(force, ForceMode.Impulse);
        }
        else
        {
            Debug.LogWarning("Rigidbody missing on the pickup prefab.");
        }

        StartCoroutine(WaitAfterDroping(2f));

        weaponCollider.isTrigger = true; // Enable trigger after droping
    }


    // Wait after dropping to enable trigger
    IEnumerator WaitAfterDroping(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
    }


    // Pick up weapon and add to slots
    public bool PickUpWeapon(MonoBehaviour weapon)
    {
        if (weapon == null)
            return false;

        if (weaponSlots.Contains(weapon))
            return false;

        for (int i = 0; i < weaponSlots.Count; i++)
        {
            if (weaponSlots[i] == null)
            {
                RegisterWeapon(weapon);
                weaponSlots[i] = weapon;

                // Update UI
                if (uiSlots.Count > i)
                {
                    if (weapon is Weapon gun)
                        uiSlots[i].SetIcon(gun.GetIcon());
                    else if (weapon is Grenade gren)
                        uiSlots[i].SetIcon(gren.GetIcon());
                }

                return true;
            }
        }

        Debug.Log("Weapon slots full!");
        return false;
    }


    // Register weapon and set references
    private void RegisterWeapon(MonoBehaviour weapon)
    {
        // GUN
        if (weapon is Weapon gun)
        {
            gun.SetReferences(firePoint, animator);
            gun.gameObject.SetActive(false);
        }
        // GRENADE
        else if (weapon is Grenade grenade)
        {
            grenade.Initialize(cameraTransform, animator, grenadeThrowPoint);
            grenade.OnDepleted -= RemoveThrowable;
            grenade.OnDepleted += RemoveThrowable;
            grenade.gameObject.SetActive(false);


        }

    }


    // Remove throwable weapon when depleted
    private void RemoveThrowable(ThrowableWeapon throwable)
    {
        int index = weaponSlots.IndexOf(throwable);
        if (index == -1)
            return;

        // If currently equipped → unequip
        if (currentWeapon == throwable)
        {
            SetWeaponActive(currentWeapon, false, 0);
            currentWeapon = null;
            currentSlotIndex = -1;
        }

        // Remove from slot
        weaponSlots[index] = null;

        // Clear UI
        if (uiSlots.Count > index)
            uiSlots[index].Clear();

        // Disable in hands
        throwable.gameObject.SetActive(false);

        Debug.Log($"{throwable.name} depleted and removed");
    }


}