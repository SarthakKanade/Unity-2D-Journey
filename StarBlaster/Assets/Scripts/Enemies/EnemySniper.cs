using UnityEngine;

public class EnemySniper : Enemy
{
    [Header("Sniper DDO Settings")]
    [SerializeField] float patrolSpeed = 2f;
    [SerializeField] float telegraphDuration = 0.9f;
    [SerializeField] float cooldownDuration = 1.6f;
    [SerializeField] GameObject aimReticlePrefab;

    Vector2 targetPatrolPos;
    GameObject currentReticle;

    protected override void OnStateEnter(EnemyState newState)
    {
        switch (newState)
        {
            case EnemyState.Drift:
                // Backline Patrol
                PickNewPatrolPoint();
                break;

            case EnemyState.Telegraph:
                // Stop and Aim
                rb.linearVelocity = Vector2.zero; 
                if (aimReticlePrefab != null && player != null)
                {
                    currentReticle = Instantiate(aimReticlePrefab, player.transform.position, Quaternion.identity);
                }
                break;

            case EnemyState.Attack:
                // Fire!
                if (currentReticle != null) Destroy(currentReticle);
                FireOneShot();
                // Instant transition to Cool down after firing
                ChangeState(EnemyState.Cooldown); 
                break;

            case EnemyState.Cooldown:
                // Just wait
                break;
        }
    }

    protected override void UpdateEnemyState()
    {
        switch (currentState)
        {
            case EnemyState.Drift:
                HandlePatrol();
                break;

            case EnemyState.Telegraph:
                HandleTelegraph();
                break;

            case EnemyState.Cooldown:
                if (stateTimer >= cooldownDuration)
                {
                    ChangeState(EnemyState.Drift); // Or Straight to Telegraph? DDO says "Repeat cycle"
                    // Usually Patrol -> Aim -> Fire -> Cooldown -> Patrol
                }
                break;
        }
    }

    void HandlePatrol()
    {
        transform.position = Vector2.MoveTowards(transform.position, targetPatrolPos, patrolSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetPatrolPos) < 0.1f)
        {
            // Reached point, start aiming
            ChangeState(EnemyState.Telegraph);
        }
    }

    void HandleTelegraph()
    {
        // Visuals: Track Player
        if (currentReticle != null && player != null)
        {
            currentReticle.transform.position = player.transform.position;
        }
        
        FacePlayer(); // Inherited helper

        if (stateTimer >= telegraphDuration)
        {
            ChangeState(EnemyState.Attack);
        }
    }

    void FireOneShot()
    {
        if (enemyShooter != null)
        {
            // We force a single shot manually since Shooter is designed for loops
            // For now, let's toggle it briefly or add a helper to Shooter later.
            // Hack for now: Toggle on/off in next frame? 
            // Better: Shooter.FireOnce() method.
            // Current Shooter support:
             StartCoroutine(FireBriefly());
        }
    }
    
    System.Collections.IEnumerator FireBriefly()
    {
        enemyShooter.isFiring = true;
        yield return new WaitForSeconds(0.1f);
        enemyShooter.isFiring = false;
    }

    void PickNewPatrolPoint()
    {
        if (GameArea.Instance != null)
        {
            targetPatrolPos = GameArea.Instance.GetRandomPointInZone(PathType.Backline);
        }
        else
        {
            targetPatrolPos = transform.position;
        }
    }

    void OnDestroy()
    {
        if (currentReticle != null) Destroy(currentReticle);
    }
}
