using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class Enemy : MonoBehaviour, IDamageable
{
    [Header("Detection")]
    [SerializeField] protected float hearingRange = 15f;
    [SerializeField] protected float sightRange = 10f;
    [SerializeField] protected float attackRange = 2f;
    [SerializeField] protected float fieldOfView = 120f;
    [SerializeField] protected LayerMask obstacleMask;

    [Header("Movement")]
    [SerializeField] protected float walkSpeed = 2f;
    [SerializeField] protected float runSpeed = 4f;

    [Header("References")]
    [SerializeField] protected Transform player;
    [SerializeField] protected NavMeshAgent agent;

    // AI State
    protected EnemyState currentState;

    protected bool playerDetected;
    protected bool playerInSightRange;
    protected bool playerInAttackRange;
    protected bool soundDetected;

    // Sound memory
    protected List<Vector3> activeSounds = new List<Vector3>();

    protected virtual void Start()
    {
        currentState = EnemyState.Idle;

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    protected virtual void Update()
    {
        Sense();
        Decide();
        Act();
    }

    // ===================== SENSE =====================
    protected virtual void Sense()
    {
        DetectPlayer();
    }

    public abstract void TakeDamage(float amount);

    // ===================== PLAYER DETECTION =====================
    protected virtual void DetectPlayer()
    {
        playerDetected = false;
        playerInSightRange = false;
        playerInAttackRange = false;

        if (player == null) return;

        Vector3 playerTarget = (player.position - transform.position).normalized;
        

        if (Vector3.Angle(transform.forward, playerTarget) < fieldOfView / 2)
        {
            float distanceToTarget = Vector3.Distance(transform.position, player.position);
            if (distanceToTarget <= sightRange &&
                Physics.Raycast(transform.position, playerTarget, distanceToTarget, obstacleMask) == false)
            {
                playerDetected = true;
                playerInSightRange = true;
                playerInAttackRange = distanceToTarget <= attackRange;
            }
        }
    }

    // ===================== SOUND =====================
    public virtual void HearSound(Vector3 soundPos)
    {
        if (Vector3.Distance(transform.position, soundPos) > hearingRange)
            return;

        if (!activeSounds.Contains(soundPos))
            activeSounds.Add(soundPos);

        soundDetected = activeSounds.Count > 0;

        if (currentState == EnemyState.Idle)
            currentState = EnemyState.Investigate;
    }

    // ===================== DECISION =====================
    protected virtual void Decide()
    {
        switch (currentState)
        {
            case EnemyState.Investigate:
                if (playerInAttackRange)
                    currentState = EnemyState.Attack;
                else if (playerInSightRange)
                    currentState = EnemyState.Chase;
                break;

            case EnemyState.Chase:
            case EnemyState.Attack:
                if (!playerInSightRange && !playerInAttackRange)
                {
                    if (soundDetected)
                    {
                        currentState = EnemyState.Investigate;
                    }
                    else
                    {
                        currentState = EnemyState.Idle;
                    }
                }
                break;

            case EnemyState.Idle:
                if (soundDetected)
                    currentState = EnemyState.Investigate;
                break;
        }
    }

    // ===================== ACTION =====================
    protected virtual void Act()
    {
        switch (currentState)
        {
            case EnemyState.Idle:
                Idle();
                break;

            case EnemyState.Investigate:
                MoveToSound();
                break;

            case EnemyState.Chase:
                ChasePlayer();
                break;

            case EnemyState.Attack:
                Attack();
                break;
        }
    }

    protected virtual void Idle()
    {
        if (agent != null)
            agent.isStopped = true;
    }

    // ===================== INVESTIGATE =====================
    protected virtual void MoveToSound()
    {
        if (activeSounds.Count == 0)
        {
            soundDetected = false;
            currentState = EnemyState.Idle;
            return;
        }

        Vector3 closest = activeSounds[0];
        float minDist = Vector3.Distance(transform.position, closest);

        foreach (var sound in activeSounds)
        {
            float d = Vector3.Distance(transform.position, sound);
            if (d < minDist)
            {
                minDist = d;
                closest = sound;
            }
        }

        MoveTowards(closest, walkSpeed);

        DetectPlayer();
        if (playerInAttackRange)
        {
            currentState = EnemyState.Attack;
            return;
        }
        if (playerInSightRange)
        {
            currentState = EnemyState.Chase;
            return;
        }

        if (minDist < 0.5f)
        {
            activeSounds.Remove(closest);
            soundDetected = activeSounds.Count > 0;
        }
    }

    protected virtual void ChasePlayer()
    {
        if (player == null) return;
        MoveTowards(player.position, runSpeed);
    }

    // ===================== MOVEMENT =====================
    protected virtual void MoveTowards(Vector3 target, float speed)
    {
        if (agent == null) return;

        agent.isStopped = false;
        agent.speed = speed;
        agent.SetDestination(target);

        RotateTowards(target);
    }

    protected void RotateTowards(Vector3 target, float rotationSpeed = 5f)
    {
        Vector3 direction = (target - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);
    }

    // ===================== COMBAT =====================
    protected abstract void Attack();

    // ===================== DEBUG =====================
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, hearingRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Vector3 fov1 = Quaternion.Euler(0, fieldOfView / 2, 0) * transform.forward * sightRange;
        Vector3 fov2 = Quaternion.Euler(0, -fieldOfView / 2, 0) * transform.forward * sightRange;

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, fov1);
        Gizmos.DrawRay(transform.position, fov2);
    }
}
