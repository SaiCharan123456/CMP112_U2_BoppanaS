using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

public abstract class ThrowableWeapon : MonoBehaviour
{
    [Header("Throwable Info")]
    [SerializeField] protected string weaponName;
    [SerializeField] protected Sprite icon;              // UI icon for the grenade
    [SerializeField] protected int maxCount = 5;         // total grenades player can carry
    [SerializeField] protected float throwForce = 15f;   // strength of the throw
    [SerializeField] protected float damage = 50f;        // damage dealt on explosion

    [Header("References")]
    [SerializeField] protected Transform throwPoint;     // hand-adjusted position
    protected Animator animator;
    protected Transform cameraTransform;
    [SerializeField] private GameObject pickupPrefab;

    [Header("UI")]
    [SerializeField] protected TextMeshProUGUI max;
    [SerializeField] protected TextMeshProUGUI current;


    // Event triggered when the throwable weapon is depleted
    public Action<ThrowableWeapon> OnDepleted;


    protected int currentCount;


    // Initialize the throwable weapon with necessary references
    public virtual void Initialize(Transform cam, Animator anim, Transform spawnPoint)
    {
        cameraTransform = cam;
        animator = anim;
        throwPoint = spawnPoint;
        currentCount = maxCount;
    }

    public Sprite GetIcon() => icon;

    public GameObject GetPickupPrefab() => pickupPrefab;

    protected void Update()
    {
        max.text = maxCount.ToString();
        current.text = 1.ToString();
    }

    public virtual void TryUse()
    {
        if (currentCount > 0)
        {
            animator?.SetTrigger("Fire");
        }

        
    }

       
    public virtual void Release() 
    {
        
        currentCount--;

        // Check for depletion

        if (currentCount <= 0)
        {
            OnDepleted?.Invoke(this);
        }

    }
}

