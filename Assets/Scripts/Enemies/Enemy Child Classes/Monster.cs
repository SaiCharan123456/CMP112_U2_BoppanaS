using UnityEngine;
using UnityEngine.Audio;

public class Monster : Enemy
{
    [Header("Attack Settings")]
    [SerializeField] Camera attackRaycastArea;
    [SerializeField] float timeBetweenAttacks = 3f;
    private bool alreadyAttacked;

    [Header("Special Powers")]
    [SerializeField] float special1Cooldown = 10f;
    [SerializeField] float special2Cooldown = 15f;
    private float lastSpecial1Time;
    private float lastSpecial2Time;

    [Header("Special Power 1 - Projectile")]
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] Transform projectileSpawnPoint;
    [SerializeField] float projectileSpeed = 25f;
    [SerializeField] float special1Range = 12f;
    [SerializeField] float special1CastTime = 1.2f; // animation wind-up

    private bool isUsingSpecial;



    [Header("Dodge Settings")]
    [SerializeField] float dodgeDistance = 3f;
    [SerializeField] float dodgeCooldown = 5f;
    private float lastDodgeTime;

    [Header("Audio Clips")]
    [SerializeField] AudioClip attackClip;
    [SerializeField] AudioClip deathClip;
    [SerializeField] AudioClip hitClip;

    [SerializeField] AudioSource audioSource;

    [Header("Animator")]
    [SerializeField] Animator animator;

    [Header("Stats")]
    [SerializeField] float damage;
    [SerializeField] float maxHealth;
    private float currentHealth;



    private void Awake()
    {
        maxHealth = 150f;
        damage = 25f;
        currentHealth = maxHealth;
    }

    protected override void Act()
    {
        switch (currentState)
        {
            case EnemyState.Dodge:
                Dodge();
                break;

            case EnemyState.Special1:
                SpecialPower1();
                break;

            case EnemyState.Special2:
                SpecialPower2();
                break;

            case EnemyState.Attack:
                Attack();
                break;

            case EnemyState.Chase:
                ChasePlayer();
                break;

            default:
                Idle();
                break;
        }
    }


    protected override void Decide()
    {
        if (player == null) return;
        if (isUsingSpecial && currentState != EnemyState.Attack && currentState != EnemyState.Chase) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // 1. Attack (highest priority)
        if (playerInAttackRange)
        {
            currentState = EnemyState.Attack;
            return;
        }

        // 2. Dodge (highest priority)
        if (Time.time - lastDodgeTime >= dodgeCooldown && ShouldDodge())
        {
            currentState = EnemyState.Dodge;
            return;
        }

       
        

        // 3. Chase (must happen before specials)
        if (playerInSightRange)
        {
            currentState = EnemyState.Chase;

            // Allow special power 1 ONLY while chasing AND at distance
            if (distance > attackRange + 1f &&
                distance <= special1Range &&
                Time.time - lastSpecial1Time >= special1Cooldown)
            {
                currentState = EnemyState.Special1;
            }

            return;
        }

        // 4. Allow special power 2 ONLY if player is close but not attacking
        if (distance <= attackRange + 0.5f &&
            Time.time - lastSpecial2Time >= special2Cooldown)
        {
            currentState = EnemyState.Special2;
            return;
        }

        // 5. Idle
        currentState = EnemyState.Idle;
    }

    protected override void Idle()
    {
        agent.isStopped = true;
        animator.SetBool("IsRunning", false);
    }


    protected override void ChasePlayer()
    {
        if (player == null) return;

        // Walk toward player
        MoveTowards(player.position, runSpeed);
        animator.SetBool("IsRunning", true);
    }

    protected override void Attack()
    {
        if (!alreadyAttacked)
        {
            
            RotateTowards(player.position);

            animator.SetTrigger("Attack");
            PlaySound(attackClip);

            RaycastHit hitInfo;
            if (Physics.Raycast(attackRaycastArea.transform.position, attackRaycastArea.transform.forward, out hitInfo, attackRange))
            {
                Debug.Log($"Monster hit {hitInfo.transform.name} for {damage} damage!");
            }
            alreadyAttacked = true;

            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    private bool ShouldDodge()
    {
       
        return Random.value > 0.7f;
    }

    //===================== DODGE BEHAVIOR =====================
    private void Dodge()
    {
        if (player == null) return;

        Vector3 dodgeDirection = Vector3.Cross((player.position - transform.position).normalized, Vector3.up);
        if (Random.value > 0.5f) dodgeDirection *= -1;

        Vector3 dodgeTarget = transform.position + dodgeDirection * dodgeDistance;
        MoveTowards(dodgeTarget, walkSpeed * 1.5f);

        if (dodgeDirection.x < 0)
            animator.SetTrigger("Dodge Left");
        else
            animator.SetTrigger("Dodge Right");
      
       // PlaySound(runClip);

        Debug.Log("Monster dodges!");

        lastDodgeTime = Time.time;
        currentState = EnemyState.Chase;
    }

    //===================== SPECIAL POWER 1 =====================
    private void SpecialPower1()
    {
        if (isUsingSpecial) return;


        // Prevent spam
        if (Time.time - lastSpecial1Time < special1Cooldown)
            return;

        isUsingSpecial = true;
        lastSpecial1Time = Time.time;

        //STOP movement
        agent.isStopped = true;

        RotateTowards(player.position);

        animator.ResetTrigger("Speacial Power 1");
        animator.SetTrigger("Speacial Power 1");
        PlaySound(attackClip);

        Debug.Log("Monster uses Special Power 1 - Projectile!");

        Invoke(nameof(FireProjectile), special1CastTime);
    }


    private void FireProjectile()
    {
        if (projectilePrefab == null || projectileSpawnPoint == null)
            return;

        GameObject projectile = Instantiate(
            projectilePrefab,
            projectileSpawnPoint.position,
            projectileSpawnPoint.rotation
        );

        // Set projectile velocity
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity =
                (player.position - projectileSpawnPoint.position).normalized
                * projectileSpeed;
        }

        Destroy(projectile, 5f);

        // Resume movement after attack
        agent.isStopped = false;
        isUsingSpecial = false;
        currentState = EnemyState.Chase;
    }


    //===================== SPECIAL POWER 2 =====================
    private void SpecialPower2()
    {

        if (isUsingSpecial) return;

        lastSpecial2Time = Time.time;
        isUsingSpecial = true;

        RotateTowards(player.position);

        Debug.Log("Monster uses Special Power 2!");

        int num = Random.Range(0, 2);

        if (num == 0) {
            animator.SetTrigger("Speacial Power 2.1");
            PlaySound(attackClip);
            Invoke(nameof(EndSpecial2), 2.2f);
        }
        else
        {
            animator.SetTrigger("Speacial Power 2.2");
            PlaySound(attackClip);
            Invoke(nameof(EndSpecial2), 3.7f);
        }
        
        
   
    }

    //
    private void EndSpecial2()
    {
        agent.isStopped = false;
        isUsingSpecial = false;
        currentState = EnemyState.Chase;
    }

    public override void TakeDamage(float amount)
    {
        currentHealth -= amount;

        animator?.SetTrigger("Damage");
        PlaySound(hitClip);

        if (currentHealth <= 0f)
            Die();
    }

    protected virtual void Die()
    {
        animator.SetTrigger("Dead");
        PlaySound(deathClip);
        agent.isStopped = true;
        Destroy(gameObject, 4f); // delay for death animation
    }

    protected void PlaySound(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;
        if (!audioSource.isPlaying)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}
