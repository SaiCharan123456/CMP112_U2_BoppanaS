using UnityEngine;
using UnityEngine.AI;

public class BossAlien : Enemy
{
    [Header("Stats")]
    [SerializeField] float maxHealth = 200f;
    [SerializeField] float damage = 30f;
    [SerializeField] float lowHpThreshold = 0.3f;
    private float currentHealth;

    [Header("Attack")]
    [SerializeField] Camera attackRaycastArea;
    [SerializeField] float timeBetweenAttacks = 3f;
    private bool alreadyAttacked;

    [Header("Special Powers & Movement")]
    [SerializeField] float special1Cooldown = 12f; // Teleport behind player
    [SerializeField] float special2Cooldown = 15f; // Speed boost
    [SerializeField] float special3Cooldown = 20f; // Push player + backward
    [SerializeField] float speedBoostDuration = 5f;
    [SerializeField] float speedBoostMultiplier = 2f;
    [SerializeField] float dodgeDistance = 5f;
    [SerializeField] float dodgeCooldown = 5f;
    [SerializeField] float jumpForce = 5f;
    [SerializeField] float flyDuration = 1.5f;
    [SerializeField] float dodgePredictionFactor = 0.5f;
    [SerializeField] float teleportDistanceBehindPlayer = 3f;

    private float lastSpecial1Time;
    private float lastSpecial2Time;
    private float lastSpecial3Time;
    private float lastDodgeTime;
    private bool isUsingSpecial;
    private bool isFlying;
    private Vector3 lastPlayerPosition;

    

    [Header("Animator & Audio")]
    [SerializeField] Animator animator;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip attackClip;
    [SerializeField] AudioClip lowHpAttackClip;
    [SerializeField] AudioClip specialClip;
    [SerializeField] AudioClip hitClip;
    [SerializeField] AudioClip deathClip;

    private enum BossPhase { Phase1, Phase2, Phase3 }
    private BossPhase currentPhase = BossPhase.Phase1;

    private float stuckTimer;

    private Rigidbody rb;
  

    protected override void Start()
    {
        base.Start();
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

    }

    protected override void Update()
    {
        base.Update();

        if (!agent.isStopped && agent.velocity.magnitude < 0.05f && currentState == EnemyState.Chase)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer > 1.2f)
            {
                RecoverAgent();
                agent.SetDestination(player.position);
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;
        }

        UpdatePhase();
        HandleStuckRecovery();
    }

    
    private void HandleStuckRecovery()
    {
        if (currentState != EnemyState.Chase) return;
        if (agent.isStopped) return;

        if (agent.remainingDistance > agent.stoppingDistance) return;

        if (agent.velocity.sqrMagnitude < 0.01f)
        {
            stuckTimer += Time.deltaTime;

            if (stuckTimer > 1.2f)
            {
                RecoverAgent();
                agent.SetDestination(player.position);
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;
        }
    }


    // Phase management based on health
    private void UpdatePhase()
    {
        float healthPercent = currentHealth / maxHealth;
        if (healthPercent <= 0.4f && currentPhase != BossPhase.Phase3)
        {
            currentPhase = BossPhase.Phase3;
            EnterPhase3();
        }
        else if (healthPercent <= 0.7f && currentPhase != BossPhase.Phase2)
        {
            currentPhase = BossPhase.Phase2;
            EnterPhase2();
        }
    }

    // Phase effects
    private void EnterPhase2()
    {
        runSpeed *= 1.2f;
        dodgeCooldown *= 0.8f;
        special1Cooldown *= 0.9f;
        special2Cooldown *= 0.9f;
        special3Cooldown *= 0.9f;
    }

    private void EnterPhase3()
    {
        runSpeed *= 1.5f;
        dodgeCooldown *= 0.7f;
        special1Cooldown *= 0.8f;
        special2Cooldown *= 0.8f;
        special3Cooldown *= 0.8f;
    }

    protected override void Decide()
    {
        if (player == null) return;
        float distance = Vector3.Distance(transform.position, player.position);

        // Idle if player is far
        if (distance > sightRange)
        {
            currentState = EnemyState.Idle;
            return;
        }

        // Dodge logic
        if (!isUsingSpecial && !alreadyAttacked && Time.time - lastDodgeTime >= dodgeCooldown && Random.value > 0.5f)
        {
            currentState = EnemyState.Dodge;
            return;
        }

        // Low HP attack
        if (currentHealth / maxHealth <= lowHpThreshold && distance <= attackRange)
        {
            currentState = EnemyState.LowHpAttack;
            return;
        }

        // Normal attack
        if (distance <= attackRange)
        {
            currentState = EnemyState.Attack;
            return;
        }

        // Chase
        if (distance <= sightRange)
        {
            currentState = EnemyState.Chase;

            // Special power 1 (teleport behind) when at mid-range
            if (distance > attackRange + 1f && distance <= sightRange * 0.6f &&
                Time.time - lastSpecial1Time >= special1Cooldown)
            {
                currentState = EnemyState.Special1;
            }

            // Special power 2 (speed boost) chance while chasing
            if (Time.time - lastSpecial2Time >= special2Cooldown && Random.value > 0.8f)
            {
                currentState = EnemyState.Special2;
            }

            // Special power 3 (push) chance when close
            if (distance <= attackRange + 1f && Time.time - lastSpecial3Time >= special3Cooldown)
            {
                currentState = EnemyState.Special3;
            }
        }
    }

    protected override void Act()
    {
        switch (currentState)
        {
            case EnemyState.Idle:
                Idle();
                break;
            case EnemyState.Chase:
                ChasePlayer();
                break;
            case EnemyState.Attack:
                Attack();
                break;
            case EnemyState.LowHpAttack:
                LowHpAttack();
                break;
            case EnemyState.Dodge:
                Dodge();
                break;
            case EnemyState.Special1:
                SpecialPower1();
                break;
            case EnemyState.Special2:
                SpecialPower2();
                break;
            case EnemyState.Special3:
                SpecialPower3();
                break;
        }
    }

    protected override void Idle()
    {
        agent.isStopped = true;
        animator.SetBool("IsRunning", false);
    }

    protected override void ChasePlayer()
    {
        if (player == null) return;
        agent.isStopped = false;
        agent.SetDestination(player.position);
        animator.SetBool("IsRunning", true);
    }

    protected override void Attack()
    {
        if (alreadyAttacked) return;
        alreadyAttacked = true;

        agent.isStopped = true;
        RotateTowards(player.position);


        // Randomly choose one of four attack animations
        int attackType = Random.Range(0, 4);

        if (attackType == 0)
        {
            animator.SetTrigger("Attack 1");
            Debug.Log($"{name} uses Attack 1!");
        }
        else if (attackType == 1)
        {
            animator.SetTrigger("Attack 2");
            Debug.Log($"{name} uses Attack 2!");
        }
        else if (attackType == 2)
        {
            animator.SetTrigger("Attack 3");
            Debug.Log($"{name} uses Attack 3!");
        }
        else
        {
            animator.SetTrigger("Attack 4");
            Debug.Log($"{name} uses Attack 4!");
        }

        PlaySound(attackClip);

        Invoke(nameof(ResetAttack), timeBetweenAttacks);
    }

    // Low HP Attack
    private void LowHpAttack()
    {
        if (alreadyAttacked) return;
        alreadyAttacked = true;

        agent.isStopped = true;
        RotateTowards(player.position);

        int attackType = Random.Range(0, 3);

        if (attackType == 0) {
            animator.SetTrigger("HpAttack 1");
            Debug.Log($"{name} uses Low HP Attack 1!");
        }
        else if (attackType == 1)
        {
            animator.SetTrigger("HpAttack 2");
            Debug.Log($"{name} uses Low HP Attack 2!");
        }
        else
        {
            animator.SetTrigger("HpAttack 3");
            Debug.Log($"{name} uses Low HP Attack 3!");
        }

        PlaySound(lowHpAttackClip);

        Invoke(nameof(ResetAttack), timeBetweenAttacks);
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
        agent.isStopped = false;
    }

    // Suspend NavMeshAgent during special move
    private void SuspendAgent()
    {
        agent.isStopped = true;
        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.ResetPath();
    }

    // Resume NavMeshAgent after special move
    private void ResumeAgent()
    {

        // Snap back safely to navmesh
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 0.5f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }

        agent.updatePosition = true;
        agent.updateRotation = true;
        agent.isStopped = false;
    }

    // Dodge implementation
    private void Dodge()
    {
        if (player == null || isUsingSpecial) return;

        isUsingSpecial = true;
        SuspendAgent();

        bool doJumpFly = Random.value > 0.5f;
        Vector3 dodgeDir;

        // Jump and fly dodge
        if (doJumpFly)
        {
            // Choose random dodge direction: left, right, or forward
            float choice = Random.value;
            if (choice < 0.33f) dodgeDir = transform.right;
            else if (choice < 0.66f) dodgeDir = -transform.right;
            else dodgeDir = transform.forward;

            // Apply jump force
            if (rb != null && !isFlying)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                isFlying = true;
                animator.SetTrigger("Flying");
                Invoke(nameof(EndFly), flyDuration);
            }

            // Move in dodge direction
            transform.position += dodgeDir * dodgeDistance;



            Debug.Log($"{name} jumps and flies to ");
        }
        // Standard dodge
        else
        {
            // Predict player's movement
            Vector3 playerMovement = (player.position - lastPlayerPosition) / Time.deltaTime;
            Vector3 predictedPos = player.position + playerMovement * dodgePredictionFactor;

            dodgeDir = (transform.position - predictedPos).normalized;

            transform.position += dodgeDir * dodgeDistance;

            animator.SetTrigger("Dodge");
            Debug.Log($"{name} dodges to ");
        }

        lastDodgeTime = Time.time;
        lastPlayerPosition = player.position;
        Invoke(nameof(EndDodge), 1.5f);
    }

    private void EndDodge()
    {
        isUsingSpecial = false;
        ResumeAgent();
        currentState = EnemyState.Chase;
    }

    private void EndFly()
    {
        isFlying = false;
        ResumeAgent();
    }

    // Recover NavMeshAgent if stuck
    private void RecoverAgent()
    {
        if (!agent.enabled) agent.enabled = true;

        agent.isStopped = false;
        agent.ResetPath();
        agent.velocity = Vector3.zero;

        // Snap back to NavMesh if needed
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 0.5f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }
    }

    // Special Power 1: Teleport behind player
    private void SpecialPower1()
    {
        if (isUsingSpecial || Time.time - lastSpecial1Time < special1Cooldown) return;

        isUsingSpecial = true;
        lastSpecial1Time = Time.time;

        // calculate position behind player
        Vector3 behindPlayer = player.position - player.forward * teleportDistanceBehindPlayer;
        transform.position = behindPlayer;
        RotateTowards(player.position);
       
        PlaySound(specialClip);

        Debug.Log($"{name} teleports behind the player!");

        Invoke(nameof(EndSpecial), 0.5f);
    }

    // Special Power 2: Speed Boost
    private void SpecialPower2()
    {
        if (isUsingSpecial || Time.time - lastSpecial2Time < special2Cooldown) return;
        isUsingSpecial = true;
        lastSpecial2Time = Time.time;


        PlaySound(specialClip);

        // Increase speed
        runSpeed *= speedBoostMultiplier;

        Debug.Log($"{name} activates speed boost!");

        Invoke(nameof(EndSpeedBoost), speedBoostDuration);
    }

    // End Speed Boost
    private void EndSpeedBoost()
    {
        runSpeed /= speedBoostMultiplier;
        isUsingSpecial = false;
        RecoverAgent();
        currentState = EnemyState.Chase;
    }

    // Special Power 3: Push Player Back + Boss Backward
    private void SpecialPower3()
    {
        if (Time.time - lastSpecial3Time < special3Cooldown || isUsingSpecial) return;
        lastSpecial3Time = Time.time;

        isUsingSpecial = true;
        agent.isStopped = true;
        RotateTowards(player.position);

        animator.SetTrigger("Speacial Power");
        PlaySound(specialClip);
        Debug.Log($"{name} uses special power to push the player back!");

        // Apply knockback to player
        if (player.TryGetComponent(out PlayerController playerMove))
        {
            Vector3 pushDir = (player.position - transform.position).normalized;
            playerMove.ApplyKnockback(pushDir, 15f); // force value
        }

        // Boss moves backward slightly
        transform.position -= transform.forward * 1.5f;

        Invoke(nameof(EndSpecial), 1.5f);
    }

    private void EndSpecial()
    {
        isUsingSpecial = false;
        agent.isStopped = false;
        RecoverAgent();
        currentState = EnemyState.Chase;
    }


    public override void TakeDamage(float amount)
    {
        currentHealth -= amount;
        animator.SetTrigger("Damage");
        PlaySound(hitClip);

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        animator.SetTrigger("Dead");
        PlaySound(deathClip);
        agent.isStopped = true;
        Destroy(gameObject, 4f);
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
