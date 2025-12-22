using UnityEngine;

public class BlindZombie : Zombie
{

    [Header("Attack Settings")]
    [SerializeField] Camera AttackingRaycastArea;
    [SerializeField] float timeBetweenAttacks = 4f;
    private bool alreadyAttacked;

    private void Awake()
    {
        maxHealth = 100f;
        damage = 10f;
        attackRange = 1f;
    }

    protected override bool CanSeePlayer() => false;

    protected override void Decide()
    {
        // Blind zombie ONLY reacts to sound
        if (soundDetected)
        {
            float dist = Vector3.Distance(transform.position, player.position);

            if (dist <= attackRange)
            {
                currentState = EnemyState.Attack;
            }
            else
            {
                currentState = EnemyState.Chase;
            }

            return;
        }

        currentState = EnemyState.Idle;
    }


    protected override void Attack()
    {
        if (!alreadyAttacked)
        {
            animator.SetTrigger("Neck Bitting"); // unique attack animation
            PlaySound(attackClip);               // play attack sound
            Debug.Log($"BlindZombie attacks for {damage} damage!");

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
        
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    protected override void Die()
    {
        animator.SetTrigger("Dead2"); // unique death animation
        PlaySound(deathClip);

        agent.isStopped = true;
        currentState = EnemyState.Idle;
        Destroy(gameObject, 3f); // delay for death animation to play
    }
}
