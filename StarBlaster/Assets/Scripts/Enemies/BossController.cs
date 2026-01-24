using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : Enemy
{
    [Header("Boss Config")]
    [SerializeField] protected BossPhaseSO phase1Config;
    [SerializeField] protected BossPhaseSO phase2Config;
    
    [Header("State")]
    [SerializeField] protected BossPhase currentPhase = BossPhase.Phase1;
    protected BossPhaseSO currentConfig;

    [Header("Shared References")]
    [SerializeField] protected LaserBeam laserBeam;

    // Timers
    protected float volleyTimer;
    protected float pulseTimer;
    protected float barrageTimer;
    protected float laserTimer;

    // Movement
    protected Vector2 startPos;
    protected float timeAlive;

    // Shared State
    protected bool isAttacking = false;
    protected float spawnTimer;

    public enum BossPhase { Phase1, Phase2 }

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        
        Health healthComp = GetComponent<Health>();
        if (healthComp != null)
        {
            healthComp.OnHealthChanged += HandleHealthChanged;
        }

        StartCoroutine(BossEntranceRoutine());
    }

    protected virtual IEnumerator BossEntranceRoutine()
    {
        currentState = EnemyState.Entrance;
        
        float targetY = 7f; 
        if (GameArea.Instance != null)
        {
            targetY = GameArea.Instance.BacklineY - 1f;
        }

        Vector3 targetPos = new Vector3(0, targetY, 0);
        
        while (Vector3.Distance(transform.position, targetPos) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, 2f * Time.deltaTime);
            yield return null;
        }
        
        startPos = transform.position;
        EnterPhase(BossPhase.Phase1);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        Health healthComp = GetComponent<Health>();
        if (healthComp != null)
        {
            healthComp.OnHealthChanged -= HandleHealthChanged;
        }
    }

    protected virtual void HandleHealthChanged(float percent)
    {
        if (currentPhase == BossPhase.Phase1 && percent <= 0.5f)
        {
            EnterPhase(BossPhase.Phase2);
        }
    }

    protected virtual void EnterPhase(BossPhase phase)
    {
        currentState = EnemyState.Attack;
        currentPhase = phase;
        currentConfig = (phase == BossPhase.Phase1) ? phase1Config : phase2Config;
    }

    protected override void UpdateEnemyState()
    {
        timeAlive += Time.deltaTime;

        MovePattern();
        HandleAttacks();
        
        if (currentConfig != null && currentConfig.enableAdds)
        {
            HandleSpawning();
        }
    }

    protected virtual void MovePattern()
    {
        if (currentConfig == null) return;
        float newX = startPos.x + Mathf.Sin(timeAlive * currentConfig.flowSpeed) * currentConfig.driftAmplitude;
        
        Vector3 pos = transform.position;
        pos.x = newX;
        transform.position = pos;
    }
    
    protected override void HandlePathComplete()
    {
        startPos = transform.position;
        base.HandlePathComplete();
    }

    protected virtual void HandleAttacks()
    {
        // Override in Subclasses (Sentinel, Aegis)
    }

    protected virtual void HandleSpawning()
    {
        if (isAttacking || currentConfig == null) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= currentConfig.spawnInterval)
        {
            if (currentConfig.complexAdds != null && currentConfig.complexAdds.Count > 0)
            {
                SpawnComplexAdd();
            }
            else
            {
                int currentAddCount = CountActiveAdds(null); 
                if (currentAddCount < currentConfig.maxAddsAlive)
                {
                    StartCoroutine(SpawnAddAtEdge(currentConfig.addsPrefab));
                }
            }
            spawnTimer = 0;
        }
    }

    protected void SpawnComplexAdd()
    {
        int totalAdds = CountActiveAdds(null);
        if (totalAdds >= currentConfig.maxAddsAlive) return;

        List<BossPhaseSO.AddConfig> candidates = new List<BossPhaseSO.AddConfig>();
        foreach(var cfg in currentConfig.complexAdds)
        {
            int typeCount = CountActiveAdds(cfg.prefab);
            if (typeCount < cfg.maxCount)
            {
                candidates.Add(cfg);
            }
        }

        if (candidates.Count == 0) return;
        var chosen = candidates[Random.Range(0, candidates.Count)];
        StartCoroutine(SpawnAddAtEdge(chosen.prefab));
    }

    protected int CountActiveAdds(GameObject specificPrefab)
    {
        Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        int count = 0;
        foreach(var e in allEnemies)
        {
            if (e == this) continue;
            
            if (specificPrefab == null) 
            {
                count++;
            }
            else
            {
                 if (e.name.Contains(specificPrefab.name))
                 {
                     count++;
                 }
            }
        }
        return count;
    }

    protected IEnumerator SpawnAddAtEdge(GameObject prefabToSpawn)
    {
        if (prefabToSpawn == null) yield break;

        bool leftSide = Random.value > 0.5f;
        float spawnX = 0f;
        float spawnY = 6f; 

        if (GameArea.Instance != null)
        {
            float padding = 1.0f;
            spawnX = leftSide ? GameArea.Instance.MinBounds.x + padding 
                              : GameArea.Instance.MaxBounds.x - padding;
            spawnY = Random.Range(GameArea.Instance.FrontlineY, GameArea.Instance.BacklineY);
        }
        else
        {
            spawnX = leftSide ? -8f : 8f;
        }

        Vector2 spawnPos = new Vector2(spawnX, spawnY);
        Instantiate(prefabToSpawn, spawnPos, Quaternion.Euler(0,0,180));
        yield return null;
    }

    protected IEnumerator FireSweepLaser()
    {
        isAttacking = true;

        if (laserBeam == null) 
        {
            Debug.LogError($"BOSS ERROR: LaserBeam reference is MISSING on {name}!");
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
        
        // 4. Unlock
        LockAim(false);
        isAttacking = false;
    }

    protected IEnumerator FireArcBarrage()
    {
        isAttacking = true;
        
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
        
        // 2. UNLOCK AIM
        LockAim(false);
        isAttacking = false;
    }
}
