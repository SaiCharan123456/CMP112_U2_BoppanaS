using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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


    public void SelectSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= weaponSlots.Count) return;

        MonoBehaviour selected = weaponSlots[slotIndex];

        if (currentWeapon == selected)
        {
            SetWeaponActive(currentWeapon, false, 0);
            currentWeapon = null;
            currentSlotIndex = -1;
        }
        else
        {
            SetWeaponActive(currentWeapon, false, 0);

            currentWeapon = selected;
            currentSlotIndex = slotIndex;
            SetWeaponActive(currentWeapon, true, 1);
        }

        UpdateUISlots();
    }

    private void SetWeaponActive(MonoBehaviour weapon, bool value, float weight)
    {
        if (weapon == null) return;
        weapon.gameObject.SetActive(value);
        animator.SetLayerWeight(animator.GetLayerIndex(weapon.gameObject.name), weight);
    }

    private void UpdateUISlots()
    {
        for (int i = 0; i < uiSlots.Count; i++)
        {
            uiSlots[i].SetSelected(i == currentSlotIndex);
        }
    }

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

    public void OnDropWeapon(InputValue value)
    {
        if (!value.isPressed)
            return;

        DropCurrentWeapon();
    }

    private void DropCurrentWeapon()
    {
        if (currentWeapon == null || currentSlotIndex < 0)
            return;

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
            uiSlots[slotIndex].Clear();

        // 4. Spawn pickup in world
        SpawnPickup(weaponToDrop);

        Debug.Log($"Dropped weapon: {weaponToDrop.name}");
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

    private void SpawnPickup(MonoBehaviour weapon)
    {
        GameObject prefab = null;

        if (weapon is Weapon gun)
            prefab = gun.GetPickupPrefab();
        else if (weapon is Grenade gren)
            prefab = gren.GetPickupPrefab();

        if (prefab == null || dropPoint == null)
            return;

        GameObject pickup = Instantiate(
            prefab,
            dropPoint.position,
            Quaternion.identity
        );

        Rigidbody rb = pickup.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 force =
                cameraTransform.forward * dropForwardForce +
                Vector3.up * dropUpForce;

            rb.AddForce(force, ForceMode.Impulse);
        }
    }



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
