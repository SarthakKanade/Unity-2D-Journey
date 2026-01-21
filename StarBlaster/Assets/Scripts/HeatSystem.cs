using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatSystem : MonoBehaviour
{
    [Header("Heat Settings")]
    [SerializeField] float maxHeat = 100f;
    [SerializeField] float heatGainPerShot = 3f; // User Req: 3
    [SerializeField] float coolingRate = 12f;    // User Req: 12
    [SerializeField] float cooldownDelay = 0.5f; // Time before cooling starts after shooting
    
    [Tooltip("How long to disable weapons after hitting 100% heat")]
    [SerializeField] float overheatDuration = 2f; // User Req: 2s

    // State
    public float CurrentHeat { get; private set; }
    public bool IsOverheated { get; private set; }
    
    // Perk Modifiers
    public float coolingMultiplier = 1f;
    public float heatGainMultiplier = 1f;
    public float maxHeatBonus = 0f;
    public float overheatDurationReduction = 0f;
    public bool allowOverdrive = false; // Epic Perk

    float lastFireTime;

    // Events
    public event System.Action OnOverheat;
    public event System.Action OnCooled;

    void Update()
    {
        ProcessCooling();
    }

    void ProcessCooling()
    {
        // Rule: Cooling only happens if NOT overheated AND we haven't fired recently.
        if (IsOverheated) return;

        if (Time.time > lastFireTime + cooldownDelay)
        {
            if (CurrentHeat > 0)
            {
                // PERK: Apply Multiplier
                float effectiveCooling = coolingRate * coolingMultiplier;
                CurrentHeat -= effectiveCooling * Time.deltaTime;
                CurrentHeat = Mathf.Max(CurrentHeat, 0);
            }
        }
    }

    public bool TryFire()
    {
        // PERK: Overdrive Core allows firing while overheated (at health cost, handled in Shooter)
        if (IsOverheated && !allowOverdrive) return false;

        AddHeat();
        return true;
    }

    void AddHeat()
    {
        lastFireTime = Time.time; // Reset cooldown timer
        
        // PERK: Apply Multiplier
        float effectiveHeat = heatGainPerShot * heatGainMultiplier;
        
        // PERK: Overdrive logic (Don't add heat if already maxed, just stay maxed)
        if (IsOverheated && allowOverdrive) return;

        CurrentHeat += effectiveHeat;

        // PERK: Max Heat Bonus
        float actualMaxHeat = maxHeat + maxHeatBonus;

        if (CurrentHeat >= actualMaxHeat)
        {
            CurrentHeat = actualMaxHeat;
            StartCoroutine(OverheatRoutine());
        }
    }

    IEnumerator OverheatRoutine()
    {
        IsOverheated = true;
        OnOverheat?.Invoke(); // UI Flash

        // PERK: Reduce duration
        float actualDuration = Mathf.Max(0.5f, overheatDuration - overheatDurationReduction);

        yield return new WaitForSeconds(actualDuration);

        IsOverheated = false;
        OnCooled?.Invoke();
    }
    
    // Helper for Damage calculation (GDD: "High Heat = High Damage")
    public float GetHeatPercentage()
    {
        return CurrentHeat / maxHeat;
    }

    public void ReduceHeatPercent(float percent)
    {
        CurrentHeat -= maxHeat * percent;
        CurrentHeat = Mathf.Max(0, CurrentHeat);
    }
}
