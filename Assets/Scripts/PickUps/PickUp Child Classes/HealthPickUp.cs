using UnityEngine;

public class HealthPickUp : PickUp
{

    [SerializeField] private float healthAmount = 25;

    protected override void OnPickUp(GameObject player)
    {
        if (PlayerManager.Instance.GetHealth() >= 100)
        {
            return;
        }
        else if (PlayerManager.Instance.GetHealth() + healthAmount > 100)
        {
            PlayerManager.Instance.ResetHealth();
            Destroy(gameObject);
        }
        else
        {
            PlayerManager.Instance.IncreaseHealth(healthAmount);
            Destroy(gameObject);
        }
    }
}
