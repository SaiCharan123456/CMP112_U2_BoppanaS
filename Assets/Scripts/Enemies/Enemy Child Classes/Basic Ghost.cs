using UnityEngine;
using UnityEngine.AI;

public class BasicGhost : Ghost
{
    [Header("Basic Ghost Settings")]
    [SerializeField] private float soundMemoryTime = 3f;

    private Vector3 lastHeardPosition;
    private float lastHeardTime;
    private bool heardSound;

    protected override void Start()
    {
        moveSpeed = 1.5f; // slow
        base.Start();

        // Always visible
        if (ghostRenderer != null)
        {
            Material mat = ghostRenderer.material;
            mat.SetFloat("_Dissolve", visibleAlpha);
        }

        // Ensure NavMeshAgent exists
        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.updateRotation = true;
            agent.isStopped = true;
        }
    }

    protected override void Update()
    {
        if (isDead) return;

        HandleHover();

        Decide();
        Act();
    }


    // ========================= DECISION (SOUND BASED ONLY) ========================
    protected override void Decide()
    {
        if (heardSound)
        {
            float distToSound = Vector3.Distance(transform.position, lastHeardPosition);

            if (distToSound <= attackRange)
            {
                currentState = EnemyState.Attack;
                return;
            }

            currentState = EnemyState.Chase;
            return;
        }

        currentState = EnemyState.Idle;
    }

    // ========================= Movement =========================
    protected override void ChasePlayer()
    {
        if (agent == null || !heardSound) return;

        animator.SetBool("IsWalking", true);

        agent.isStopped = false;
        agent.speed = moveSpeed;
        agent.SetDestination(lastHeardPosition);

        LookAtTarget(lastHeardPosition);

        // Forget sound after memory expires
        if (Time.time - lastHeardTime > soundMemoryTime)
            heardSound = false;
    }

    protected override void Idle()
    {
        animator.SetBool("IsWalking", false);
        if (agent != null)
            agent.isStopped = true;
    }

    // ========================= ATTACK =========================
    protected override void OnGhostAttack()
    {
        // Damage handled by animation event or overlap
        Debug.Log($"{name} attacks with claws!");
    }

    // ========================= SOUND DETECTION =========================
    public override void HearSound(Vector3 soundPos)
    {
        float dist = Vector3.Distance(transform.position, soundPos);

        if (dist > hearingRange) return;

        lastHeardPosition = soundPos;
        lastHeardTime = Time.time;
        heardSound = true;
    }

    // ========================= HELPER =========================
    private void LookAtTarget(Vector3 target)
    {
        Vector3 dir = target - transform.position;
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
}
