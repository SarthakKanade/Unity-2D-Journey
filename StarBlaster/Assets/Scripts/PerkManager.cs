using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PerkManager : MonoBehaviour
{
    public static PerkManager Instance { get; private set; }

    [Header("Configuration")]
    [SerializeField] List<PerkDataSO> allPerks; // Assign all 15 SOs here

    // State: PerkID -> Current Level
    Dictionary<PerkID, int> ownedPerks = new Dictionary<PerkID, int>();

    // References to Systems we modify
    HeatSystem playerHeat;
    Health playerHealth;
    Shooter playerShooter;

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
        // Find Player Components
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null)
        {
            playerHeat = pc.GetComponent<HeatSystem>();
            playerHealth = pc.GetComponent<Health>();
            playerShooter = pc.GetComponentInChildren<Shooter>();
        }

        // Subscribe to Global Events
        Enemy.OnEnemyDeath += HandleEnemyDeath;
        // WaveManager will call HandleWaveEnd directly or we subscribe if event exists
    }

    void OnDestroy()
    {
        Enemy.OnEnemyDeath -= HandleEnemyDeath;
    }

    // Called by UI Selection
    public void AcquirePerk(PerkID id)
    {
        PerkDataSO data = GetPerkData(id);
        if (data == null) return;

        if (ownedPerks.ContainsKey(id))
        {
            ownedPerks[id]++;
        }
        else
        {
            ownedPerks.Add(id, 1);
        }

        ApplyPerkEffect(id, ownedPerks[id], data);
        Debug.Log($"Acquired Perk: {id} (Level {ownedPerks[id]})");
    }

    void ApplyPerkEffect(PerkID id, int level, PerkDataSO data)
    {
        float value = data.GetValueAtLevel(level);

        switch (id)
        {
            // --- COMMON ---
            case PerkID.ReinforcedHull:
                playerHealth.IncreaseMaxHealth((int)value); 
                break;
            case PerkID.WeaponCalibration:
                playerShooter.damageMultiplier = 1 + (value / 100f);
                break;
            case PerkID.HeatVenting:
                playerHeat.coolingMultiplier = 1 + (value / 100f);
                break;
            case PerkID.HeatDiscipline:
                playerHeat.heatGainMultiplier = 1 - (value / 100f);
                break;
            case PerkID.RapidCycling:
                playerShooter.fireRateMultiplier = 1 + (value / 100f);
                break;
            case PerkID.StabilityCore:
                playerHeat.maxHeatBonus += value; // Fixed: using maxHeatBonus
                break;

            // --- RARE ---
            case PerkID.LifeLeech:
                // Logic handled in OnHit (needs hook)
                break;
            case PerkID.EmergencyCooling:
                playerHeat.overheatDurationReduction = value;
                break;
            
            // --- EPIC ---
            case PerkID.OverdriveCore:
                playerHeat.allowOverdrive = true;
                break;
        }
    }

    // --- Event Handlers ---

    void HandleEnemyDeath(Enemy enemy)
    {
        // Check for OnKill perks
        if (ownedPerks.ContainsKey(PerkID.KillMomentum))
        {
            // Trigger temporary fire rate buff
            // StartCoroutine(KillMomentumRoutine());
        }
    }

    public void HandleWaveComplete()
    {
        // Check for OnWaveEnd perks
        if (ownedPerks.ContainsKey(PerkID.FieldRepair))
        {
            float percent = GetPerkValue(PerkID.FieldRepair);
            // Value is e.g. 6 (6%), convert to 0.06
            playerHealth.HealPercent(percent / 100f);
        }
        
        if (ownedPerks.ContainsKey(PerkID.CoolingSurge))
        {
            // Reset instant heat
            playerHeat.ReduceHeatPercent(1f); // ToDo: Implement ReduceHeatPercent
        }
    }

    // --- RNG Section ---

    public List<PerkDataSO> GeneratePerkOptions(int count = 3)
    {
        List<PerkDataSO> options = new List<PerkDataSO>();
        if (allPerks == null || allPerks.Count == 0) Debug.LogError("PerkManager: All Perks List is EMPTY!");
        
        List<PerkDataSO> available = new List<PerkDataSO>(allPerks);
        Debug.Log($"PerkManager: Initial Available Count: {available.Count}");

        // Filter out Maxed Perks
        available.RemoveAll(p => ownedPerks.ContainsKey(p.id) && ownedPerks[p.id] >= p.maxLevel);
        Debug.Log($"PerkManager: Available After Max Filter: {available.Count}");
        
        // Filter out Unique/One-Time Perks if already owned (e.g. Second Chance)
        // (Logic handled by MaxLevel=1 in SO usually, but double check)

        for (int i = 0; i < count; i++)
        {
            if (available.Count == 0) break;

            PerkRarity targetRarity = RollRarity();
            
            // Try to find perks of this rarity
            List<PerkDataSO> pool = available.Where(p => p.rarity == targetRarity).ToList();
            
            // If none found (e.g. no Epics left), fallback to any
            if (pool.Count == 0) pool = available;

            if (pool.Count > 0)
            {
                PerkDataSO selected = pool[Random.Range(0, pool.Count)];
                options.Add(selected);
                available.Remove(selected); // Don't pick same perk twice in one hand
            }
        }
        
        Debug.Log($"PerkManager: Generated {options.Count} Options.");
        return options;
    }

    PerkRarity RollRarity()
    {
        // Spec: Common 70%, Rare 25%, Epic 5%
        // Bonus: Dynamic scaling based on Wave (ToDo: Pass wave index)
        float roll = Random.value;
        if (roll < 0.7f) return PerkRarity.Common;
        if (roll < 0.95f) return PerkRarity.Rare;
        return PerkRarity.Epic;
    }

    // Helper
    PerkDataSO GetPerkData(PerkID id)
    {
        return allPerks.FirstOrDefault(p => p.id == id);
    }
    
    float GetPerkValue(PerkID id)
    {
        if (!ownedPerks.ContainsKey(id)) return 0;
        return GetPerkData(id).GetValueAtLevel(ownedPerks[id]);
    }

    public int GetPerkLevel(PerkID id)
    {
        return ownedPerks.ContainsKey(id) ? ownedPerks[id] : 0;
    }
}
