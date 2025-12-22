using UnityEngine;

public class NormalZombie : Zombie
{
    [Header("Scream Settings")]
    [SerializeField] float screamThreshold = 0.3f; // scream if health below 30%
    [SerializeField] float screamCooldown = 10f;
    private float lastScreamTime;

    [SerializeField] AudioClip screamClip;

    [Header("Attack Settings")]
    [SerializeField] Camera AttackingRaycastArea;
    [SerializeField] float timeBetweenAttacks = 1.5f;
    private bool alreadyAttacked;


    
    public EnemyState CurrentState
    {
        get { return currentState; }
        private set { currentState = value; }
    }

    private void Awake()
    {
        maxHealth = 100f; 
        damage = 10f;
    }

    
    protected override void Decide()
    {
        if (playerInSightRange || playerInAttackRange)
        {
            if (playerInAttackRange)
            {
                // Decide whether to attack or scream
                if (ShouldScream())
                    CurrentState = EnemyState.Scream;
                else
                    CurrentState = EnemyState.Attack;
            }
            else
            {
                CurrentState = EnemyState.Chase;
            }
        }
        else if (soundDetected)
        {
            CurrentState = EnemyState.Investigate;
        }
        else
        {
            CurrentState = EnemyState.Idle;
        }
    }

    // Determine if the zombie should scream
    private bool ShouldScream()
    {
        if (Time.time - lastScreamTime < screamCooldown)
            return false;

        float healthPercent = currentHealth / maxHealth;
        int alliesAttacking = CountAlliesAttackingPlayer();

        if (healthPercent < screamThreshold || alliesAttacking < 2)
        {
            lastScreamTime = Time.time;
            return true;
        }

        return false;
    }

    // Count how many allied zombies are currently attacking the player
    private int CountAlliesAttackingPlayer()
    {
        NormalZombie[] allZombies = Object.FindObjectsByType<NormalZombie>(FindObjectsSortMode.None);
        int count = 0;
        foreach (var z in allZombies)
        {
            if (z != this && (z.CurrentState == EnemyState.Chase || z.CurrentState == EnemyState.Attack))
                count++;
        }
        return count;
    }

    protected override void Attack()
    {
        if (!alreadyAttacked)
        {
            Debug.Log("Zombie attacks player!");
            base.Attack();
            RotateTowards(player.position);

            RaycastHit hitInfo;
            if (Physics.Raycast(AttackingRaycastArea.transform.position, AttackingRaycastArea.transform.forward, out hitInfo, attackRange))
            {
                //if (hitInfo.collider.CompareTag("Player"))
                //{
                //    Debug.Log($"Player hit for {damage} damage!");
                //    // Here you would call the player's TakeDamage method
                //}

                Debug.Log($"Hit {hitInfo.transform.name} for {damage} damage!");
            }

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
        

        // Implement melee attack logic here
    }

    // Reset attack state
    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    protected override void Act()
    {
        if (CurrentState == EnemyState.Idle)
            Idle();
        else if (CurrentState == EnemyState.Investigate)
            MoveToSound();
        else if (CurrentState == EnemyState.Chase)
            ChasePlayer();
        else if (CurrentState == EnemyState.Attack)
            Attack();
        else if (CurrentState == EnemyState.Scream)
            Scream();
    }

    // Scream to attract other zombies
    private void Scream()
    {
        animator.SetTrigger("Scream");
        PlaySound(screamClip);
        Debug.Log("Zombie screams to attract others!");
        // Play scream sound and alert nearby zombies
        CurrentState = EnemyState.Chase; // After screaming, chase player
    }
}
