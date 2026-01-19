using UnityEngine;

public class EnemyBomber : Enemy
{
    [Header("Bomber DDO Settings")]
    [SerializeField] float driftDurationMin = 1.8f;
    [SerializeField] float driftDurationMax = 2.8f;
    [SerializeField] float telegraphDuration = 0.75f;
    [SerializeField] float rushDuration = 1.0f;
    [SerializeField] float rushSpeed = 15f;
    [SerializeField] Color telegraphColor = Color.red;

    SpriteRenderer spriteRenderer;
    Color originalColor;
    
    // State Data
    Vector2 targetDriftPos;
    float currentDriftDuration;
    Vector2 lockedRushVector;

    protected override void Awake()
    {
        base.Awake();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    protected override void Start()
    {
        base.Start();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
    }

    protected override void OnStateEnter(EnemyState newState)
    {
        switch (newState)
        {
            case EnemyState.Drift:
                currentDriftDuration = Random.Range(driftDurationMin, driftDurationMax);
                PickNewDriftPoint();
                break;
            
            case EnemyState.Telegraph:
                rb.linearVelocity = Vector2.zero;
                if (spriteRenderer != null) spriteRenderer.color = telegraphColor;
                // Add Audio Cue here later
                break;

            case EnemyState.Attack: // The Rush
                if (spriteRenderer != null) spriteRenderer.color = originalColor; // Or keep red? DDO doesn't specify
                // Lock Direction once
                if (player != null)
                {
                    lockedRushVector = (player.transform.position - transform.position).normalized;
                }
                else
                {
                    lockedRushVector = Vector2.down;
                }
                rb.linearVelocity = lockedRushVector * rushSpeed;
                break;
        }
    }

    protected override void UpdateEnemyState()
    {
        switch(currentState)
        {
            case EnemyState.Drift:
                HandleDrift();
                break;
            case EnemyState.Telegraph:
                HandleTelegraph();
                break;
            case EnemyState.Attack:
                HandleRush();
                break;
        }
    }

    void HandleDrift()
    {
        transform.position = Vector2.MoveTowards(transform.position, targetDriftPos, 2f * Time.deltaTime); // Slow drift

        if (Vector2.Distance(transform.position, targetDriftPos) < 0.1f)
        {
            PickNewDriftPoint();
        }

        if (stateTimer >= currentDriftDuration)
        {
            ChangeState(EnemyState.Telegraph);
        }
    }

    void HandleTelegraph()
    {
        FacePlayer(); // Track player until last second
        
        if (stateTimer >= telegraphDuration)
        {
            ChangeState(EnemyState.Attack);
        }
    }

    void HandleRush()
    {
        // Movement handled by Physics (velocity set in OnStateEnter)
        // Just check timer for self-destruct if missed
        if (stateTimer >= rushDuration)
        {
            // Missed check? Or just destroy based on DDO "Self-destruct shortly after rush"
            // Let's add the small 0.25s delay mentioned in DDO
            // stateTimer includes the rush duration. 
            // So @ rushDuration, we stop?
             Destroy(gameObject, 0.25f);
        }
    }

    void PickNewDriftPoint()
    {
        if (GameArea.Instance != null)
        {
            targetDriftPos = GameArea.Instance.GetRandomPointInZone(PathType.Any); // Mid Zone
        }
        else
        {
            targetDriftPos = transform.position;
        }
    }
}
