using UnityEngine;

public enum EnemyState 
{ 
    Entrance,   // Following Path
    Drift,      // Idle movement (Mid zone)
    Attack,     // Firing / Rushing
    Telegraph,  // Charging up (Red flash)
    Cooldown,   // Waiting
    Special     // Tank Crawl / Unique
}

public abstract class Enemy : MonoBehaviour
{
    public static System.Action<Enemy> OnEnemyDeath;

    // Existing fields...

    // ...

    void OnDestroy()
    {
       // Notify Manager
       OnEnemyDeath?.Invoke(this); 
    }




    [Header("Enemy Stats")]
    [SerializeField] protected EnemyStatsSO stats;

    // Fallbacks if SO is missing (These are kept as per original, but will be overridden by stats if present)
    [SerializeField] protected float moveSpeed = 5f;
    [SerializeField] protected int scoreValue = 100;

    // State Machine
    protected EnemyState currentState = EnemyState.Entrance;
    protected float stateTimer; // General purpose timer for states

    // References
    protected PlayerController player;
    protected Rigidbody2D rb;
    protected Shooter enemyShooter;
    
    // Components
    // Pathfinding removed

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyShooter = GetComponentInChildren<Shooter>();
    }

    protected virtual void Start()
    {
        player = FindFirstObjectByType<PlayerController>();

        // NEW: Apply Stats from SO
        if (stats != null)
        {
            ApplyStats();
        }
        
        // NEW: Entrance Logic (Spawn & Seek)
        // Pick a target spot in my allowed zone
        if (GameArea.Instance != null && stats != null)
        {
            Vector2 targetZonePos = GameArea.Instance.GetRandomPointInZone(stats.preferredPath);
            // Debug.Log($"{name} spawning. Moving from {transform.position} to {targetZonePos}");
            StartCoroutine(EntranceRoutine(targetZonePos));
        }
        else
        {
            Debug.LogWarning($"{name}: Missing GameArea Instance or Stats! Defaulting to Drift immediately.");
            // Fallback if no GameArea or Stats
            ChangeState(EnemyState.Drift);
        }
    }

    // Coroutine to handle the initial "Fly In"
    System.Collections.IEnumerator EntranceRoutine(Vector2 targetPos)
    {
        currentState = EnemyState.Entrance;
        
        // While far from target
        while (Vector2.Distance(transform.position, targetPos) > 0.5f)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            FacePlayer(); // Or face travel direction? User likes "Face Player".
            yield return null;
        }

        // Arrived!
        HandlePathComplete();
    }

    void ApplyStats()
    {
        // 1. Body Stats
        moveSpeed = stats.moveSpeed;
        scoreValue = stats.scoreValue;
        
        Health healthComp = GetComponent<Health>();
        if (healthComp != null)
        {
            healthComp.SetMaxHealth(stats.health);
        }

        // 2. Weapon Stats
        if (enemyShooter != null && stats.projectilePrefab != null)
        {
            // IMPORTANT: Disable Shooter's internal AI so our State Machine controls firing
            enemyShooter.useAI = false; 

            enemyShooter.InitializeWeapon(
                stats.projectilePrefab,
                stats.projectileSpeed,
                stats.damage,
                stats.fireRate
            );
        }
    }


    protected virtual void Update()
    {
        stateTimer += Time.deltaTime;

        // Global Rule: Enemies usually face player (except Tanks, handled in subclass)
        // Subclasses can override FacePlayer if needed, or we check state.
        FacePlayer();

        // State Machine
        if (currentState == EnemyState.Entrance) return; // Coroutine handles movement, we wait.

        UpdateEnemyState(); // Subclasses implement specific behavior
    }

    // NEW: Core State Machine Logic
    protected virtual void UpdateEnemyState() 
    { 
        // Subclasses override this switch statement
    }

    public void ChangeState(EnemyState newState)
    {
        currentState = newState;
        stateTimer = 0f;
        OnStateEnter(newState);
    }

    protected virtual void OnStateEnter(EnemyState newState) { }

    // Callback when entrance path finishes
    protected virtual void HandlePathComplete()
    {
        // Transition from Entrance to Default Combat State
        ChangeState(EnemyState.Drift); 
    }

    // NEW: Cleanup Phase Aggression
    public virtual void EnableDangerMode()
    {
        // Base behavior: Just move faster?
        // Specifics: Subclasses should override this.
        // e.g., Grunt: ChangeState(EnemyState.Attack) -> Rush Player
        moveSpeed *= 1.5f;
    }
    
    // Common Helper Methods
    protected void FacePlayer()
    {
        if (player == null) return;
        
        Vector2 direction = player.transform.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
