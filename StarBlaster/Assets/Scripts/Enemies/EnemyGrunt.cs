using UnityEngine;

public class EnemyGrunt : Enemy
{
    [Header("Grunt DDO Settings")]
    [SerializeField] float driftDurationMin = 3.5f;
    [SerializeField] float driftDurationMax = 4.5f;
    [SerializeField] float driftSpeed = 2f;
    [SerializeField] float advanceSpeed = 3f;

    Vector2 targetDriftPos;
    float currentDriftDuration;

    protected override void OnStateEnter(EnemyState newState)
    {
        switch (newState)
        {
            case EnemyState.Drift:
                // Setup Drift Phase
                currentDriftDuration = Random.Range(driftDurationMin, driftDurationMax);
                PickNewDriftPoint();
                if (enemyShooter != null) enemyShooter.isFiring = true; // Always fire
                break;

            case EnemyState.Attack:
                // Setup Advance Phase
                if (enemyShooter != null) enemyShooter.isFiring = true;
                break;
        }
    }

    protected override void UpdateEnemyState()
    {
        switch (currentState)
        {
            case EnemyState.Drift:
                HandleDriftBehavior();
                break;

            case EnemyState.Attack:
                HandleAdvanceBehavior();
                break;
        }
    }

    void HandleDriftBehavior()
    {
        // 1. Move towards random point in Mid Zone
        transform.position = Vector2.MoveTowards(transform.position, targetDriftPos, driftSpeed * Time.deltaTime);

        // 2. If reached point, pick new one
        if (Vector2.Distance(transform.position, targetDriftPos) < 0.1f)
        {
            PickNewDriftPoint();
        }

        // 3. Check Timer -> Transition to Advance
        if (stateTimer >= currentDriftDuration)
        {
            ChangeState(EnemyState.Attack);
        }
    }

    void HandleAdvanceBehavior()
    {
        // Advance towards player (or just down?)
        // DDO: "Advance toward player until destroyed"
        // Simple implementation: Move towards Player's current position
        
        if (player != null)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.transform.position, advanceSpeed * Time.deltaTime);
        }
        else
        {
            // Fallback if player dead
            transform.Translate(Vector2.down * advanceSpeed * Time.deltaTime);
        }
    }

    void PickNewDriftPoint()
    {
        if (GameArea.Instance != null)
        {
            targetDriftPos = GameArea.Instance.GetRandomPointInZone(PathType.Any); // 'Any' defaults to Mid Zone in my GameArea logic
        }
        else
        {
            targetDriftPos = transform.position; // Stay put if no GameArea
        }
    }
}
