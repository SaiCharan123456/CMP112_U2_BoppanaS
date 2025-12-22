using UnityEngine;

public abstract class Zombie : Enemy
{
    [Header("Zombie Stats")]
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected float attackCooldown = 1.2f;

    [Header("Zombie Movement")]
    [SerializeField] GameObject[] walkPoint;
    int currentZombiePosition = 0;
    float walkPointRange = 2f;

    [Header("Animation")]
    [SerializeField] protected Animator animator;

    [Header("Audio")]
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected AudioClip walkClip;
    [SerializeField] protected AudioClip runClip;
    [SerializeField] protected AudioClip attackClip;
    [SerializeField] protected AudioClip hitClip;
    [SerializeField] protected AudioClip deathClip;

    protected float currentHealth;
    protected float lastAttackTime;

    protected override void Start()
    {
        base.Start();
        currentHealth = maxHealth;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }


    // Expose read-only properties for derived classes if needed
    public float Health => maxHealth;
    public float Damage => damage;

    // ===================== IDLE BEHAVIOR =====================
    protected override void Idle()
    {
        agent.isStopped = false;
        agent.speed = walkSpeed;
        // Wander between walk points
        if (Vector3.Distance(walkPoint[currentZombiePosition].transform.position,transform.position) < walkPointRange)
        {
            currentZombiePosition = Random.Range(0, walkPoint.Length);
            if (currentZombiePosition >= walkPoint.Length)
            {
                currentZombiePosition = 0;
            }
            
        }

        // Move towards the current walk point
        transform.position = Vector3.MoveTowards(transform.position, walkPoint[currentZombiePosition].transform.position, walkSpeed * Time.deltaTime);
        transform.LookAt(walkPoint[currentZombiePosition].transform.position);
        animator.SetBool("IsWalking", true);
        animator.SetBool("IsRunning", false);
        PlaySound(walkClip);
    }

    // ===================== DECISION =====================
    protected override void Decide()
    {
        if (CanSeePlayer() && (playerInSightRange || playerInAttackRange))
        {
            if (playerInAttackRange)
                currentState = EnemyState.Attack;
            else
                currentState = EnemyState.Chase;

            return;
        }

        if (soundDetected)
        {
            currentState = EnemyState.Investigate;
            return;
        }

        if (!playerInAttackRange && !playerInSightRange)
        {
            currentState = EnemyState.Idle;
        }
    }

    protected virtual bool CanSeePlayer() => true;

    protected override void ChasePlayer()
    {
        if (player == null) return;

        MoveTowards(player.position, runSpeed);

        animator?.SetBool("IsRunning", true);
        animator?.SetBool("IsWalking", false);
        PlaySound(runClip);
    }

    // ===================== COMBAT =====================
    protected override void Attack()
    {
        if (Time.time - lastAttackTime < attackCooldown)
            return;

        lastAttackTime = Time.time;

        animator?.SetTrigger("Attack");
        PlaySound(attackClip);

        Debug.Log($"{name} attacks player for {damage}");
        // PlayerHealth.Instance.TakeDamage(damage);
    }

    public override void TakeDamage(float amount)
    {
        currentHealth -= amount;

        animator?.SetTrigger("damage");
        PlaySound(hitClip);

        if (currentHealth <= 0f)
            Die();
    }

    protected virtual void Die()
    {
        currentState = EnemyState.Idle;

        animator?.SetTrigger("Dead1");
        PlaySound(deathClip);

        agent.isStopped = true;
        Destroy(gameObject, 3f);
    }

    // ===================== AUDIO =====================
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
