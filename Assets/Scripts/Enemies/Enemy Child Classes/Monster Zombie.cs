using UnityEngine;

public class MonsterZombie : Zombie
{
    [Header("Attack Settings")]
    [SerializeField] Camera AttackingRaycastArea;
    [SerializeField] float timeBetweenAttacks = 2f;
    private bool alreadyAttacked;

    [Header("Bullet Settings")]
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform firePoint;
    [SerializeField] float bulletSpeed = 20f;


    private void Awake()
    {
        maxHealth = 50f;
        damage = 10f;
        attackRange = 5f;
    }

    protected override void ChasePlayer()
    {
        if (player == null) return;

        // Walk toward player instead of running
        MoveTowards(player.position, walkSpeed);

        animator.SetBool("IsWalking", true);
        animator.SetBool("IsRunning", false);
    }

    //===================== ATTACK BEHAVIOR =====================
    protected override void Attack()
    {
        if (!alreadyAttacked)
        {
            Debug.Log($"MonsterZombie attacks for {damage} damage!");
            RotateTowards(player.position);

            // Play attack animation & sound
            animator.SetTrigger("Attack");
            PlaySound(attackClip);

            if (agent != null)
                agent.isStopped = true;

            RaycastHit hitInfo;
            if (Physics.Raycast(AttackingRaycastArea.transform.position, AttackingRaycastArea.transform.forward, out hitInfo, attackRange))
            {
                //if (hitInfo.collider.CompareTag("Player"))
                //{
                //    Debug.Log($"Player hit for {damage} damage!");
                //    
                //}

                if (bulletPrefab != null)
                {
                    GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
                    Rigidbody rb = bullet.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.linearVelocity = firePoint.forward * bulletSpeed;

                    }

                    Destroy(bullet, 3f); // Destroy bullet after 3 seconds
                }



                Debug.Log($"Hit {hitInfo.transform.name} for {damage} damage!");
            }

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
            
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;

        if (agent != null)
            agent.isStopped = false;
    }

}
