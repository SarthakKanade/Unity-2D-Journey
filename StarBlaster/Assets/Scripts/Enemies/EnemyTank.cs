using UnityEngine;

public class EnemyTank : Enemy
{
    [Header("Tank DDO Settings")]
    [SerializeField] float xTrackingIntervalMin = 0.4f;
    [SerializeField] float xTrackingIntervalMax = 0.6f;
    [SerializeField] float trackDurationMin = 2.0f;
    [SerializeField] float trackDurationMax = 2.5f;
    [SerializeField] float trackingSpeed = 1f;
    [SerializeField] float crawlSpeed = 0.5f;

    // State Data
    float trackTimer; // How long have we been in Tracking Phase?
    float nextXUpdateTimer; // When do we update our target X?
    float targetX;
    float totalTrackTimeAssigned;

    protected override void Start()
    {
        base.Start();
        // Tanks override standard FacePlayer, so let's lock rotation here just in case
        transform.rotation = Quaternion.Euler(0, 0, 180); 
    }

    protected override void OnStateEnter(EnemyState newState)
    {
        if (newState == EnemyState.Drift) // Tank uses Drift state as "Tracking Phase"
        {
            totalTrackTimeAssigned = Random.Range(trackDurationMin, trackDurationMax);
            targetX = transform.position.x;
            nextXUpdateTimer = 0f;
            if (enemyShooter != null) enemyShooter.isFiring = true;
        }
        else if (newState == EnemyState.Attack) // Tank uses Attack state as "Crawl Phase"
        {
            if (enemyShooter != null) enemyShooter.isFiring = true;
        }
    }

    protected override void UpdateEnemyState()
    {
        // Tank always faces down (180)
        transform.rotation = Quaternion.Euler(0, 0, 180);

        switch (currentState)
        {
            case EnemyState.Drift: // Tracking Phase
                HandleTracking();
                break;

            case EnemyState.Attack: // Crawl Phase
                HandleCrawl();
                break;
        }
    }

    void HandleTracking()
    {
        trackTimer += Time.deltaTime;
        nextXUpdateTimer -= Time.deltaTime;

        // 1. Update Target X at intervals (imperfect tracking)
        if (nextXUpdateTimer <= 0)
        {
            if (player != null)
            {
                targetX = player.transform.position.x;
            }
            nextXUpdateTimer = Random.Range(xTrackingIntervalMin, xTrackingIntervalMax);
        }

        // 2. Move towards Target X (Maintain Y)
        // We use MoveTowards on X axis only
        float newX = Mathf.MoveTowards(transform.position.x, targetX, trackingSpeed * Time.deltaTime);
        transform.position = new Vector2(newX, transform.position.y);

        // 3. Transition to Crawl
        if (trackTimer >= totalTrackTimeAssigned)
        {
            ChangeState(EnemyState.Attack);
        }
    }

    void HandleCrawl()
    {
        // "Never Retreats... Slow constant forward"
        transform.Translate(Vector2.up * crawlSpeed * Time.deltaTime); 
        // Note: transform.Translate(Vector2.up) moves in LOCAL Up.
        // Since Tank is rotated 180 (facing down), Local Up is World Down.
        // So this correctly moves it down the screen.
    }
}
