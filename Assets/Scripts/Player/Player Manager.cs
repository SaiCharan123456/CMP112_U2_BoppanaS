using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{

    public static float Health = 100;

    public static PlayerManager Instance;

    [SerializeField] private GameObject normalPlayer;
    [SerializeField] private GameObject ghostPlayer;

    [SerializeField] private Transform ghostCameraTarget;
    [SerializeField] private Transform normalCameraTarget;

    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider ghostHealthSlider;
    [SerializeField] private Slider shootingHealthSlider;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        ResetHealth();
        SwitchToNormalPlayer();
    }

    public void ResetHealth()
    {
        Health = 100;
        healthSlider.value = Health;
        ghostHealthSlider.value = Health;
        shootingHealthSlider.value = Health;
    }

    public float GetHealth()
    {
        return Health;
    }

    public void IncreaseHealth(float amount)
    {
        Health += amount;
        if (Health > 100)
        {
            Health = 100;
        }

        healthSlider.value = Health;
        ghostHealthSlider.value = Health;
        shootingHealthSlider.value = Health;
    }

    public void DecreaseHealth(float amount)
    {
        Health -= amount;
        Debug.Log("Player Health: " + Health);
        healthSlider.value = Health;
        ghostHealthSlider.value = Health;
        shootingHealthSlider.value = Health;

        if (Health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
               // Handle player death (e.g., respawn, game over screen, etc.)
        Debug.Log("Player has died.");
    }

    public void SwitchToNormalPlayer()
    {
        normalPlayer.transform.position = ghostPlayer.transform.position;
        normalPlayer.transform.rotation = ghostPlayer.transform.rotation;

        normalPlayer.SetActive(true);
        ghostPlayer.SetActive(false);        

        CameraManager.Instance.SetFollowTarget(normalCameraTarget);
    }

    public void SwitchToGhostPlayer()
    {
        ghostPlayer.transform.position = normalPlayer.transform.position;
        ghostPlayer.transform.rotation = normalPlayer.transform.rotation;

        normalPlayer.SetActive(false);
        ghostPlayer.SetActive(true);

        CameraManager.Instance.SetFollowTarget(ghostCameraTarget);
    }

}
