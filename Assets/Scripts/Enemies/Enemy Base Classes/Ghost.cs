using UnityEngine;

public abstract class Ghost : Enemy
{
    [Header("Ghost Movement")]
    [SerializeField] protected float moveSpeed = 3f;
    [SerializeField] protected float hoverAmplitude = 0.4f;
    [SerializeField] protected float hoverFrequency = 2f;

    [Header("Combat")]
    [SerializeField] protected float damage = 5f;
    [SerializeField] protected float attackCooldown = 2f;

    [Header("Visual Fade")]
    [SerializeField] protected Renderer ghostRenderer;
    [SerializeField] protected float visibleAlpha = 1f;
    [SerializeField] protected float invisibleAlpha = 0.5f;
    [SerializeField] protected float fadeSpeed = 4f;

    [Header("Animation")]
    [SerializeField] protected Animator animator;

    [Header("Audio")]
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected AudioClip attackClip;
    [SerializeField] protected AudioClip damageClip;
    [SerializeField] protected AudioClip deathClip;
    [SerializeField] protected AudioClip idleClip;

    protected float lastAttackTime;
    protected Vector3 startPos;
    protected float hoverOffset;
    protected bool isDead;


    protected override void Start()
    {
        base.Start();
        startPos = transform.position;
        hoverOffset = Random.Range(0f, 10f);
        PlayIdleSound();
    }

    protected override void Update()
    {
        if (isDead || player == null) return;

        HandleHover();
        HandleVisibility();
        Decide();
        Act();
    }

    // ========================= DECISION ========================
    protected override void Decide()
    {
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist > sightRange)
        {
            currentState = EnemyState.Idle;
            return;
        }

        if (dist <= attackRange)
        {
            currentState = EnemyState.Attack;
            return;
        }

        currentState = EnemyState.Chase;
    }

    
    protected override void Idle()
    {
        animator.SetBool("IsWalking", false);
    }

    protected override void ChasePlayer()
    {
        animator.SetBool("IsWalking", true);

        // Move toward player
        Vector3 dir = (player.position - transform.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;

        LookAtPlayer();
    }

    // ========================= ATTACK =========================
    protected override void Attack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;

        lastAttackTime = Time.time;
        animator.SetTrigger("Attack");
        PlaySound(attackClip);

        Debug.Log($"{name} attacks the player for {damage} damage!");

        OnGhostAttack();
    }


    // ========================= MOVEMENT & VISUALS =========================
    protected void HandleHover()
    {
        // Simple hover effect
        float hover = Mathf.Sin(Time.time * hoverFrequency + hoverOffset) * hoverAmplitude;
        // Update vertical position
        transform.position = new Vector3(
            transform.position.x,
            startPos.y + hover,
            transform.position.z
        );
    }

    // Make the ghost face the player smoothly
    protected void LookAtPlayer()
    {
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(dir),
                Time.deltaTime * 5f
            );
        }
    }

    // Adjust ghost visibility based on distance to player
    protected virtual void HandleVisibility()
    {
        if (ghostRenderer == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        float targetAlpha;

        // Determine target alpha based on distance
        if (dist <= sightRange * 0.9f)
        {
            targetAlpha = visibleAlpha;
        }
        else
        {
            targetAlpha = invisibleAlpha;
        }

        // Smoothly interpolate to target alpha
        Material mat = ghostRenderer.material;
        mat.SetFloat("_Dissolve", targetAlpha);
    }


    protected void PlaySound(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;
        audioSource.PlayOneShot(clip);
    }

    protected void PlayIdleSound()
    {
        if (audioSource == null || idleClip == null) return;
        audioSource.loop = true;
        audioSource.clip = idleClip;
        audioSource.Play();
    }

    // ========================= DAMAGE & DEATH =========================
    public override void TakeDamage(float amount)
    {
        if (isDead) return;

        animator.SetTrigger("Damage");
        PlaySound(damageClip);
    }

    protected virtual void Die()
    {
        isDead = true;

        animator.SetTrigger("Dead");
        PlaySound(deathClip);

        Destroy(gameObject, 3f);
    }

    protected abstract void OnGhostAttack();
}
