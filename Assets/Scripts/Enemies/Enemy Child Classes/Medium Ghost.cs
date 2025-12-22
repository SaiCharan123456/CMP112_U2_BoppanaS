using UnityEngine;

public class MediumGhost : Ghost
{
    [Header("Vision")]
    [SerializeField] private float visionRange = 12f;
    [SerializeField] private float visionAngle = 180f;

    [Header("Visibility Control")]
    [SerializeField] private float fullyVisibleDistance = 4f;
    [SerializeField] private float damageRevealTime = 1.2f;

    [Header("Special Power - Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float projectileCooldown = 4f;
    [SerializeField] private float projectileRange = 8f;

    private float lastProjectileTime;


    private Vector3 lastHeardPosition;
    private bool heardSound;
    private float revealTimer;

    protected override void Start()
    {
        base.Start();
        moveSpeed *= 1.3f; // faster than basic ghost

        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.updateRotation = true;
        }
    }

    protected override void Update()
    {
        base.Update();

        if (revealTimer > 0f)
            revealTimer -= Time.deltaTime;
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
            case EnemyState.Special1:
                ThrowProjectile();
                break;
        }
    }


 
    protected override void Decide()
    {
        if (player == null)
        {
            currentState = EnemyState.Idle;
            return;
        }

        float dist = Vector3.Distance(transform.position, player.position);

        // Vision check
        if (CanSeePlayer())
        {
            if (dist <= attackRange)
                currentState = EnemyState.Attack;
            // Use special if in range and off cooldown
            else if (dist <= projectileRange && Time.time - lastProjectileTime >= projectileCooldown)
                currentState = EnemyState.Special1;
            else
                currentState = EnemyState.Chase;
        }
        else if (heardSound)
        {
            // Only chase if the last heard position is valid and not too far
            if (Vector3.Distance(transform.position, lastHeardPosition) <= visionRange * 2)
                currentState = EnemyState.Chase;
            else
                heardSound = false; // stop chasing if last heard position is too far
        }
        else
        {
            currentState = EnemyState.Idle;
        }
    }

    // ========================= SPECIAL ATTACK ========================
    private void ThrowProjectile()
    {
        if (Time.time - lastProjectileTime < projectileCooldown)
            return;

        lastProjectileTime = Time.time;

        animator.SetTrigger("Attack");

        // Reveal on attack
        revealTimer = damageRevealTime;

        // Launch projectile toward player
        Vector3 dir = (player.position - projectileSpawnPoint.position).normalized;

        GameObject proj = Instantiate(
            projectilePrefab,
            projectileSpawnPoint.position,
            Quaternion.LookRotation(dir)
        );

        currentState = EnemyState.Chase;

        //if (proj.TryGetComponent(out GhostProjectile gp))
        //{
        //    gp.Init(dir);
        //}
    }


   
    protected override void ChasePlayer()
    {
        animator.SetBool("IsWalking", true);

        Vector3 target;

        // Prioritize seeing player over sound
        if (CanSeePlayer())
        {
            target = player.position;
        }
        else if (heardSound)
        {
            target = lastHeardPosition;

            // Stop chasing if reached the last heard position
            if (Vector3.Distance(transform.position, lastHeardPosition) < 1.2f)
            {
                heardSound = false;
                currentState = EnemyState.Idle;
                return;
            }
        }
        else
        {
            currentState = EnemyState.Idle;
            return;
        }

        if (agent != null)
        {
            agent.isStopped = false;
            agent.speed = moveSpeed;
            agent.SetDestination(target);
        }
    }


    
    protected override void OnGhostAttack()
    {
        revealTimer = damageRevealTime;

        // Damage player 
        if (Vector3.Distance(transform.position, player.position) <= attackRange)
        {
            if (player.TryGetComponent(out PlayerController pc))
            {
                //pc.TakeDamage(damage);
            }
        }
    }

    // ========================= SOUND DETECTION =========================
    public override void HearSound(Vector3 soundPos)
    {
        if (Vector3.Distance(transform.position, soundPos) > hearingRange)
            return;

        lastHeardPosition = soundPos;
        heardSound = true;
    }

    // ========================= VISIBILITY =========================
    protected override void HandleVisibility()
    {
        if (ghostRenderer == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        float targetAlpha;
        // Become fully visible when close, taking damage, or attacking

        if (dist <= fullyVisibleDistance || revealTimer > 0f || currentState == EnemyState.Attack)
            targetAlpha = visibleAlpha;
        else
            targetAlpha = invisibleAlpha;

        // Smoothly interpolate current alpha toward target alpha
        Material mat = ghostRenderer.material;
        mat.SetFloat("_Dissolve", targetAlpha);
    }

    // ========================= VISION CHECK =========================
    private bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 dir = player.position - transform.position;
        float dist = dir.magnitude;

        if (dist > visionRange) return false;
        float angle = Vector3.Angle(transform.forward, dir);

        return angle <= visionAngle * 0.5f;
    }

    
    public override void TakeDamage(float amount)
    {
        base.TakeDamage(amount);
        revealTimer = damageRevealTime;
    }
}
