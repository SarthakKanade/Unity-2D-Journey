using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("Configuration")]
    [SerializeField] List<WaveRangeConfigSO> rangeConfigs;
    [SerializeField] float initialWaveDelay = 2.0f;
    [SerializeField] float waveBreakTime = 3.0f;
    [SerializeField] int startingWave = 1; // NEW: Easy testing!

    [Header("Boss Settings")]
    [SerializeField] GameObject bossPrefab;
    [SerializeField] int bossWaveNumber = 10;
    
    [Header("Debug / Read-Only")]
    [SerializeField] int currentWaveIndex = 1;
    [SerializeField] WaveState currentState = WaveState.WaitingToStart;
    [SerializeField] bool isBossWave = false; 

    [SerializeField] int currentQuota;
    [SerializeField] int spawnedCount;
    [SerializeField] int killedCount;
    [SerializeField] int enemiesOnScreen;
    [SerializeField] float currentSpawnIntervalMin;
    [SerializeField] float currentSpawnIntervalMax;
    [SerializeField] int currentMaxOnScreen;

    // Escalation
    float escalationTimer;
    const float ESCALATION_INTERVAL = 35f;

    EnemySpawner spawner;
    WaveRangeConfigSO currentConfig;
    float nextSpawnTime;

    public enum WaveState
    {
        WaitingToStart,
        Spawning,
        BossBattle, // NEW
        Cleanup,
        WaveComplete,
        GameOver
    }

    // UI Helpers
    public int GetCurrentWave() => currentWaveIndex;
    public int GetRemainingQuota() => currentQuota - killedCount;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner == null) Debug.LogError("WaveManager: No EnemySpawner found!");

        // Assuming Enemy has static event, or we subscribe via spawner
        Enemy.OnEnemyDeath += HandleEnemyDeath;

        StartCoroutine(StartFirstWaveRoutine());
    }

    void OnDestroy()
    {
        Enemy.OnEnemyDeath -= HandleEnemyDeath;
    }

    IEnumerator StartFirstWaveRoutine()
    {
        yield return new WaitForSeconds(initialWaveDelay);
        StartWave(startingWave);
    }

    void Update()
    {
        if (currentState == WaveState.Spawning)
        {
            HandleSpawning();
            HandleEscalation();
        }
        else if (currentState == WaveState.Cleanup)
        {
            // Just waiting for kills, maybe special cleanup logic here
        }
    }

    void StartWave(int waveIndex)
    {
        currentWaveIndex = waveIndex;
        
        // BOSS TRIGGER
        if (currentWaveIndex == bossWaveNumber)
        {
            StartBossBattle();
            return;
        }

        currentState = WaveState.Spawning;
        isBossWave = false;

        // Reset Counters
        spawnedCount = 0;
        killedCount = 0;
        enemiesOnScreen = 0;
        escalationTimer = 0f;

        // Load Config
        currentConfig = GetConfigForWave(currentWaveIndex);
        if (currentConfig == null)
        {
            Debug.LogError($"No Config found for Wave {currentWaveIndex}!");
            return;
        }

        // Calculate Stats
        CalculateQuota();
        currentSpawnIntervalMin = currentConfig.spawnIntervalMin;
        currentSpawnIntervalMax = currentConfig.spawnIntervalMax;
        currentMaxOnScreen = currentConfig.maxEnemiesOnScreen;

        Debug.Log($"Wave {currentWaveIndex} Started! Quota: {currentQuota}");
    }

    void StartBossBattle()
    {
        currentState = WaveState.BossBattle;
        isBossWave = true;
        
        // Spawn Boss
        Debug.Log("⚠️ WARNING: BOSS APPROACHING! ⚠️");
        
        if (bossPrefab != null)
        {
            // Spawn at default location (Entrance handled by BossController)
            Instantiate(bossPrefab, new Vector3(0, 12, 0), Quaternion.identity);
            enemiesOnScreen = 1; // Track boss
        }
    }

    WaveRangeConfigSO GetConfigForWave(int wave)
    {
        foreach (var config in rangeConfigs)
        {
            if (wave >= config.minWave && wave <= config.maxWave)
                return config;
        }
        return null;
    }

    void CalculateQuota()
    {
        // Formula: Base 8 + (Wave * 2)
        int baseQuota = 8 + (currentWaveIndex * 2);
        currentQuota = Mathf.RoundToInt(baseQuota * currentConfig.quotaMultiplier);
    }

    void HandleSpawning()
    {
        if (spawnedCount >= currentQuota)
        {
            SetState(WaveState.Cleanup);
            TriggerDangerMode();
            return;
        }

        if (enemiesOnScreen >= currentMaxOnScreen) return;

        if (Time.time >= nextSpawnTime)
        {
            SpawnEnemy();
            float interval = UnityEngine.Random.Range(currentSpawnIntervalMin, currentSpawnIntervalMax);
            nextSpawnTime = Time.time + interval;
        }
    }

    void SpawnEnemy()
    {
        if (currentConfig == null) return;
        
        GameObject prefab = currentConfig.GetRandomEnemyPrefab();
        if (prefab != null)
        {
            spawner.SpawnSpecificEnemy(prefab);
            spawnedCount++;
            enemiesOnScreen++;
        }
    }

    void HandleEscalation()
    {
        escalationTimer += Time.deltaTime;
        if (escalationTimer >= ESCALATION_INTERVAL)
        {
            escalationTimer = 0;
            ApplyEscalation();
        }
    }

    void ApplyEscalation()
    {
        // Simple alternation logic or random pick
        // For now: Decrease interval by 10%
        currentSpawnIntervalMin *= 0.9f;
        currentSpawnIntervalMax *= 0.9f;
        Debug.Log("Escalation Applied! Faster Spawns.");
    }

    void HandleEnemyDeath(Enemy enemy)
    {
        enemiesOnScreen--;
        killedCount++;

        if (enemiesOnScreen < 0) enemiesOnScreen = 0;

        // BOSS DEATH CHECK
        if (isBossWave && enemy is BossController)
        {
            HandleBossDefeated();
            return;
        }

        CheckWaveCompletion();
    }

    void HandleBossDefeated()
    {
        Debug.Log("🎉 BOSS DEFEATED! 🎉");
        
        // 1. Rewards
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            // Full Heal
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null) playerHealth.HealPercent(1.0f);
            
            // Heat Reset
            HeatSystem heat = player.GetComponent<HeatSystem>();
            if (heat != null) heat.ReduceHeatPercent(1.0f);
            
            // Score Bonus
            ScoreKeeper score = FindFirstObjectByType<ScoreKeeper>();
            if (score != null) score.ModifyScore(2000);
        }

        // 2. Clear Minions?
        Enemy[] remaining = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach(var e in remaining)
        {
            Destroy(e.gameObject); // Clean sweep
        }

        // 3. Complete Wave
        StartCoroutine(WaveCompleteRoutine());
    }

    void CheckWaveCompletion()
    {
        // Allow completion if Spawning OR Cleanup
        if (currentState != WaveState.Spawning && currentState != WaveState.Cleanup) return;

        // Wave is done if we spawned everyone AND killed everyone
        if (spawnedCount >= currentQuota && killedCount >= currentQuota)
        {
            StartCoroutine(WaveCompleteRoutine());
        }
    }

    IEnumerator WaveCompleteRoutine()
    {
        SetState(WaveState.WaveComplete);
        Debug.Log("Wave Complete!");
        
        PerkManager.Instance.HandleWaveComplete(); // Trigger OnWaveEnd perks

        yield return new WaitForSeconds(waveBreakTime);

        // Find and Show UI (Include inactive because the panel is likely hidden)
        PerkSelectionUI selectionUI = FindFirstObjectByType<PerkSelectionUI>(FindObjectsInactive.Include);
        if (selectionUI != null)
        {
            selectionUI.Show(); // This will pause the game
        }
        else
        {
            Debug.LogWarning("No PerkSelectionUI found! Skipping perk selection.");
            StartNextWave();
        }
    }

    public void StartNextWave()
    {
        StartWave(currentWaveIndex + 1);
    }

    void SetState(WaveState newState)
    {
        currentState = newState;
    }

    void TriggerDangerMode()
    {
        Debug.Log("DANGER MODE: Cleanup Phase Started!");
        // Notify all active enemies to charge
        Enemy[] activeEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
                enemy.EnableDangerMode();
        }
    }
}
