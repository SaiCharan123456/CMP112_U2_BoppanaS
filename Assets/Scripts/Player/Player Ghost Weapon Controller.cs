using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGhostWeaponController : MonoBehaviour
{
    [Header("Ghost Weapon")]
    [SerializeField] private Weapon ghostGun;
    [SerializeField] private GameObject ghostGunUI;

    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Header("UI")]
    [SerializeField] private GameObject ammoDisplay;
    [SerializeField] private GameObject crosshair;
    [SerializeField] private GameObject reload;

    private bool isUnlocked = false;
    private bool isEquipped = false;

    // Called by pickup
    public void UnlockGhostGun()
    {
        isUnlocked = true;
        isEquipped = false;
        UpdateState();
    }

    private void OnEnable()
    {
        UpdateState();
    }

    private void UpdateState()
    {
        bool active = isUnlocked && isEquipped;

        ghostGun.gameObject.SetActive(active);

        if (ghostGunUI != null)
            ghostGunUI.SetActive(isUnlocked);

        if (active)
        {
            animator.SetLayerWeight(animator.GetLayerIndex("Ghost Gun"),1);
            ammoDisplay.SetActive(true);
            crosshair.SetActive(true);
            reload.SetActive(true);
            CameraManager.Instance.SwitchToThirdPersonAim();
        }
        else
        {
            animator.SetLayerWeight(animator.GetLayerIndex("Ghost Gun"), 0);
            ammoDisplay.SetActive(false);
            crosshair.SetActive(false);
            reload.SetActive(false);
            CameraManager.Instance.SwitchToThirdPerson();
        }
    }

    public void ToggleWeapon()
    {
        if (!isUnlocked) return;

        isEquipped = !isEquipped;
        UpdateState();
    }

    public void UiReload()
    {
        if (!isUnlocked || !isEquipped) return;
        ghostGun.TryReload();
    }

    public void OnShoot(InputValue value)
    {
        if (!value.isPressed) return;
        if (!isUnlocked || !isEquipped) return;

        ghostGun.TryShoot();
    }

    public void OnReload(InputValue value)
    {
        if (!value.isPressed) return;
        if (!isUnlocked || !isEquipped) return;

        ghostGun.TryReload();
    }

    public void OnSlot1(InputValue value)
    {
        if (!value.isPressed) return;
        ToggleWeapon();
    }

    public bool HasGhostGun()
    {
        return isUnlocked;
    }
}
