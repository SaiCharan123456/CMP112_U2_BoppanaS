using UnityEngine;

public class BossGhost : Ghost
{
    [Header("Boss Specials")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawn;
    [SerializeField] private float projectileRange = 10f;

    [SerializeField] private float teleportRange = 7f; 
    [SerializeField] private float teleportDistance = 3f;
    [SerializeField] private float phaseDuration = 3f;
    [SerializeField] private float phaseSpeedMultiplier = 2f;

    [SerializeField] private float specialCooldown = 5f;

    [Header("Dodge Settings")]
    [SerializeField] private float dodgeDistance = 3f;
    [SerializeField] private float dodgeCooldown = 5f;
    [SerializeField] private float dodgePredictionFactor = 0.5f;

    private float lastDodgeTime;
    private Vector3 lastPlayerPosition;
    private bool isDodging;


    private float lastSpecialTime;
    private bool isPhasing;


    protected override void Start()
    {
        moveSpeed *= 1.5f; // Boss is faster
        base.Start();
        lastPlayerPosition = player.position;

        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.updateRotation = true;
        }

    }

    protected override void Update()
    {
        base.Update();
        lastPlayerPosition = player.position;
    }

    protected override void Decide()
    {
        if (player == null)
        {
            currentState = EnemyState.Idle;
            return;
        }
        float dist = Vector3.Distance(transform.position, player.position);

        if (!isDodging && Time.time - lastDodgeTime >= dodgeCooldown)
        {
            float playerSpeed = (player.position - lastPlayerPosition).magnitude / Time.deltaTime;
            if (playerSpeed > 2f)
            {
                currentState = EnemyState.Dodge;
                Debug.Log("Boss Ghost decides to dodge!");
                return;
            }
        }

        // If phasing, just chase normally
        if (isPhasing)
        {
            currentState = EnemyState.Chase;
            return;
        }

        // Check if player is within sight
        if (dist > sightRange)
        {
            currentState = EnemyState.Idle; // don’t chase if too far
            //Debug.Log("Boss Ghost can't see the player, going idle");
            return;
        }

        // Normal attack/special decisions
        if (dist <= attackRange)
            currentState = EnemyState.Attack;
        // Teleport if in range and off cooldown
        else if (dist <= teleportRange && CanUseSpecial())
        {
            Debug.Log("boss ghost teleports");
            currentState = EnemyState.Special1;
        }
        // Multi projectile if in range and off cooldown
        else if (dist <= projectileRange && CanUseSpecial())
            currentState = EnemyState.Special2;
        else
            currentState = EnemyState.Chase;

        //Debug.Log($"Boss Ghost decides to {currentState} (dist: {dist})");

    }


    protected override void Act()
    {
        //Debug.Log($"Boss Ghost acting in state: {currentState}");
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
            case EnemyState.Dodge:
                Dodge();
                break;
            case EnemyState.Special1:
                Teleport();
                break;
            case EnemyState.Special2:
                MultiProjectile();
                break;
            case EnemyState.Special3:
                PhaseWalk();
                break;
        }
    }

    protected override void ChasePlayer()
    {
        animator.SetBool("IsWalking", true);

        if (agent != null)
        {
            agent.isStopped = false;
            agent.speed = moveSpeed;
            agent.SetDestination(player.position);
        }
    }

    // ====================== SPECIAL 1 — TELEPORT BEHIND PLAYER ======================
    private void Teleport()
    {
        if (!CanUseSpecial()) return;

        lastSpecialTime = Time.time;

        // Calculate position behind player
        Vector3 behindPlayer =
            player.position - player.forward * teleportDistance;

        
        if (agent != null)
            agent.Warp(behindPlayer);
        else
            transform.position = behindPlayer;

        LookAtPlayer();

        Debug.Log("Boss Ghost teleported behind player!");

        animator.SetTrigger("Attack");
        PlaySound(attackClip);

        currentState = EnemyState.Chase;
    }

    // ====================== SPECIAL 2 — MULTI-PROJECTILE ATTACK ======================
    private void MultiProjectile()
    {
        if (!CanUseSpecial()) return;

        lastSpecialTime = Time.time;

        animator.SetTrigger("Attack");

        // Fire 3 projectiles in a spread
        for (int i = -1; i <= 1; i++)
        {
            // Calculate direction with spread
            Vector3 dir = (player.position - projectileSpawn.position).normalized;
            dir = Quaternion.Euler(0f, i * 15f, 0f) * dir;
            Debug.Log("Boss Ghost fires projectile!");

            GameObject proj = Instantiate(
                projectilePrefab,
                projectileSpawn.position,
                Quaternion.LookRotation(dir)
            );

            

            //if (proj.TryGetComponent(out GhostProjectile gp))
            //{
            //    gp.Init(dir);
            //}
        }
    }

    // ====================== SPECIAL 3 — PHASE WALK ======================
    private void PhaseWalk()
    {
        if (isPhasing) return;

        isPhasing = true;
        lastSpecialTime = Time.time;

        // Increase speed and become semi-transparent
        moveSpeed *= phaseSpeedMultiplier;
        if (agent != null) agent.speed = moveSpeed;
        SetGhostAlpha(0.7f);

        Debug.Log("Boss Ghost enters phase walk!");

        Invoke(nameof(EndPhase), phaseDuration);
    }

    // ====================== END PHASE WALK ======================
    private void EndPhase()
    {
        moveSpeed /= phaseSpeedMultiplier;
        if (agent != null) agent.speed = moveSpeed;
        SetGhostAlpha(visibleAlpha);
        isPhasing = false;
    }

    private void Dodge()
    {
        if (isDodging) return;

        isDodging = true;
        lastDodgeTime = Time.time;

        Vector3 playerMovement = (player.position - lastPlayerPosition) / Time.deltaTime;
        Vector3 predictedPos = player.position + playerMovement * dodgePredictionFactor;

        // Dodge perpendicular to player movement
        Vector3 dodgeDir = Vector3.Cross(Vector3.up, predictedPos - transform.position).normalized;

        // Randomize left/right
        if (Random.value > 0.5f) dodgeDir = -dodgeDir;

        if (agent != null)
        {
            Vector3 dodgeTarget = transform.position + dodgeDir * dodgeDistance;
            agent.Warp(dodgeTarget); // instant dodge
        }

        animator.SetTrigger("Attack"); 
        PlaySound(attackClip);

        Debug.Log("Boss Ghost dodges!");

        // End dodge after short time
        Invoke(nameof(EndDodge), 1f);
    }

    private void EndDodge()
    {
        isDodging = false;
        currentState = EnemyState.Chase;
    }


    // ====================== OVERRIDE TAKE DAMAGE ======================
    public override void TakeDamage(float amount)
    {
        if (isPhasing) return;

        base.TakeDamage(amount);

        // 30% chance to immediately use Special3 (Phase Walk) when taking damage
        if (Random.value > 0.7f)
        {
            currentState = EnemyState.Special3;
        }
    }

    // ====================== CHECK SPECIAL COOLDOWN ======================
    private bool CanUseSpecial()
    {
        if (Time.time - lastSpecialTime < specialCooldown)
            return false;

        return true;
    }

    // ====================== SET GHOST ALPHA ======================
    private void SetGhostAlpha(float alpha)
    {
        if (ghostRenderer == null) return;

        Material mat = ghostRenderer.material;
        mat.SetFloat("_Dissolve", alpha);
    }

    protected override void OnGhostAttack()
    {

        Debug.Log("Boss Ghost attacks the player!");
    }
}
