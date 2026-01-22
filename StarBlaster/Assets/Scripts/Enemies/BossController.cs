using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : Enemy
{
    [Header("Boss Config")]
    [SerializeField] BossPhaseSO phase1Config;
    [SerializeField] BossPhaseSO phase2Config;
    [SerializeField] Transform[] firePoints; // 0 for Barrage, 1 for Laser?

    [Header("State")]
    [SerializeField] BossPhase currentPhase = BossPhase.Phase1;
    BossPhaseSO currentConfig;

    // Attack Timers
    float barrageTimer;
    float laserTimer;
    float spawnTimer;

    // Movement
    Vector2 startPos;
    float timeAlive;

    public enum BossPhase { Phase1, Phase2 }

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        
        // Listen to Health
        Health healthComp = GetComponent<Health>();
        if (healthComp != null)
        {
            healthComp.OnHealthChanged += HandleHealthChanged;
        }

        // Start Entrance
        StartCoroutine(BossEntranceRoutine());
    }

    IEnumerator BossEntranceRoutine()
    {
        currentState = EnemyState.Entrance;
        
        // Target: Top of Enemy Zone (Backline)
        float targetY = 7f; // Default high
        if (GameArea.Instance != null)
        {
            targetY = GameArea.Instance.BacklineY - 1f; // Slightly below top edge
        }

        Vector3 targetPos = new Vector3(0, targetY, 0);
        
        while (Vector3.Distance(transform.position, targetPos) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, 2f * Time.deltaTime);
            yield return null;
        }
        
        // Arrived
        startPos = transform.position;
        EnterPhase(BossPhase.Phase1);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy(); // IMPORTANT: Notify WaveManager!

        Health healthComp = GetComponent<Health>();
        if (healthComp != null)
        {
            healthComp.OnHealthChanged -= HandleHealthChanged;
        }

        // BOSS REWARD handled in WaveManager
    }

    void HandleHealthChanged(float percent)
    {
        if (currentPhase == BossPhase.Phase1 && percent <= 0.5f)
        {
            EnterPhase(BossPhase.Phase2);
        }
    }

    void EnterPhase(BossPhase phase)
    {
        currentState = EnemyState.Attack; // UNFREEZE LOGIC
        currentPhase = phase;
        currentConfig = (phase == BossPhase.Phase1) ? phase1Config : phase2Config;
        
        // Reset Timers? 
        // Maybe keep them but clamp to new cooldowns so they don't instant fire
        
        Debug.Log($"BOSS ENTERING {phase}");

        if (phase == BossPhase.Phase2)
        {
            // Flash / Sound
        }
    }

    protected override void UpdateEnemyState()
    {
        timeAlive += Time.deltaTime;

        MovePattern();
        HandleAttacks();
        
        if (currentConfig.enableAdds)
        {
            HandleSpawning();
        }
    }

    void MovePattern()
    {
        // Drifts forward slowly + Sine wave X
        // Y Position: Slowly move from Spawn (Back) towards Mid
        // We shouldn't cross Player Line
        
        // For now, simpler Drift:
        float newX = startPos.x + Mathf.Sin(timeAlive * currentConfig.flowSpeed) * currentConfig.driftAmplitude;
        
        // Slow advance?
        // float speed = (currentPhase == BossPhase.Phase2) ? 0.3f : 0.2f;
        // transform.Translate(Vector3.down * speed * Time.deltaTime);
        
        // Just Clamp X
        Vector3 pos = transform.position;
        pos.x = newX;
        transform.position = pos;
    }
    
    // Override Start to capture startPos after Entrance
    protected override void HandlePathComplete()
    {
        startPos = transform.position;
        base.HandlePathComplete();
    }

    bool isAttacking = false;

    void HandleAttacks()
    {
        if (player == null || isAttacking) return; // Block new attacks

        barrageTimer += Time.deltaTime;
        laserTimer += Time.deltaTime;

        // Prioritize Laser if both ready? Or random?
        // Let's prioritize Laser because it has a longer cooldown
        if (laserTimer >= currentConfig.laserCooldown)
        {
            StartCoroutine(FireSweepLaser());
            laserTimer = 0;
            return;
        }

        if (barrageTimer >= currentConfig.barrageCooldown)
        {
            StartCoroutine(FireArcBarrage());
            barrageTimer = 0;
        }
    }

    void HandleSpawning()
    {
        // RULES:
        // 1. Never spawn while attacking (Laser/Barrage active)
        if (isAttacking) 
        {
            // Debug.Log("BOSS SPAWN BLOCKED: Attacking"); // Uncomment if curious
            return;
        }

        spawnTimer += Time.deltaTime;
        
        // Debug every second to ensure timer is running
        // if (Time.frameCount % 60 == 0) Debug.Log($"BOSS SPAWN TIMER: {spawnTimer}/{currentConfig.spawnInterval}");

        if (spawnTimer >= currentConfig.spawnInterval)
        {
            // 2. Cap Boss Adds
            int currentAddCount = CountActiveAdds();
            Debug.Log($"BOSS ATTEMPT SPAWN: Timer Ready. Adds Active: {currentAddCount}/{currentConfig.maxAddsAlive}");
            
            if (currentAddCount < currentConfig.maxAddsAlive)
            {
                StartCoroutine(SpawnAddAtEdge());
            }
            else
            {
                Debug.Log("BOSS SPAWN SKIPPED: Max Adds Reached.");
            }
            spawnTimer = 0;
        }
    }

    int CountActiveAdds()
    {
        // Simple tag check or counting Component
        // Since we only have Boss and Adds, counts all enemies except self
        Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        int count = 0;
        foreach(var e in allEnemies)
        {
            if (e != this) count++;
        }
        return count;
    }

    IEnumerator SpawnAddAtEdge()
    {
        if (currentConfig.addsPrefab == null) yield break;

        // 3. Edge Spawning
        // Left (-X) or Right (+X)
        // Y: Random height in Enemy Zone
        bool leftSide = Random.value > 0.5f;
        float spawnX = 0f;
        float spawnY = 6f; // Fallback

        if (GameArea.Instance != null)
        {
            float padding = 1.0f;
            spawnX = leftSide ? GameArea.Instance.MinBounds.x + padding 
                              : GameArea.Instance.MaxBounds.x - padding;
            
            // Random Y in Mid/Front
            spawnY = Random.Range(GameArea.Instance.FrontlineY, GameArea.Instance.BacklineY);
        }
        else
        {
            spawnX = leftSide ? -8f : 8f;
        }

        Vector2 spawnPos = new Vector2(spawnX, spawnY);

        Instantiate(currentConfig.addsPrefab, spawnPos, Quaternion.Euler(0,0,180));
        
        Debug.Log($"BOSS: Spawning Add at {spawnPos}");
        yield return null;
    }


    [Header("Laser Reference")]
    [SerializeField] LaserBeam laserBeam;

    // --- ATTACKS ---

    IEnumerator FireArcBarrage()
    {
        isAttacking = true;
        
        // Visual Telegraph
        if (currentConfig.projectilePrefab == null)
        {
            isAttacking = false;
            yield break;
        }

        // Phase 1: Tracking (0.4s)
        yield return new WaitForSeconds(0.4f);

        // Phase 2: Locked Warning (0.2s)
        LockAim(true); 
        yield return new WaitForSeconds(0.2f); // Total 0.6s telegraph

        int bulletCount = 6;
        float spreadAngle = 45f;
        
        // Use current locked rotation as base
        float baseAngle = transform.rotation.eulerAngles.z;

        float startAngle = baseAngle - (spreadAngle / 2f);
        float angleStep = spreadAngle / (bulletCount - 1);
        
        // Fire logic
        for (int i = 0; i < bulletCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            Quaternion rotation = Quaternion.Euler(0, 0, currentAngle);
            
            GameObject bullet = Instantiate(currentConfig.projectilePrefab, transform.position, rotation);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = bullet.transform.up * currentConfig.projectileSpeed;
            }
            
            DamageDealer dd = bullet.GetComponent<DamageDealer>();
            if (dd != null)
            {
                dd.SetDamage(currentConfig.projectileDamage);
            }
            
            Destroy(bullet, 5f);
        }
        
        Debug.Log("BOSS: Arc Barrage Fired!");
        
        // 2. UNLOCK AIM
        LockAim(false);
        isAttacking = false;
    }

    IEnumerator FireSweepLaser()
    {
        isAttacking = true;

        if (laserBeam == null) 
        {
            Debug.LogError("BOSS ERROR: LaserBeam reference is MISSING in Inspector!");
            isAttacking = false;
            yield break;
        }

        // Setup Damage
        laserBeam.Setup(currentConfig.laserDamagePerSec);

        // Start Visuals (Warning Line appears NOW)
        StartCoroutine(laserBeam.FireRoutine(1.2f, currentConfig.laserDuration));

        // Sync Lock Logic:
        // 1. Tracking Phase (0.8s) - Warning line moves with boss
        yield return new WaitForSeconds(0.8f);

        // 2. Lock Aim (0.4s before fire) - Final commitment
        LockAim(true);
        yield return new WaitForSeconds(0.4f);

        // 3. Fire Phase (Laser is active now)
        // Keep locked while firing + small buffer to ensure visuals are gone
        yield return new WaitForSeconds(currentConfig.laserDuration + 0.25f);
        
        Debug.Log("BOSS: Sweep Laser Channeling...");
        
        // 4. Unlock
        LockAim(false);
        isAttacking = false;
    }
}
