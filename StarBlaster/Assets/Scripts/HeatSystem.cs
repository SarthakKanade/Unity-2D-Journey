using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatSystem : MonoBehaviour
{
    [Header("Heat Settings")]
    [SerializeField] float maxHeat = 100f;
    [SerializeField] float heatGainPerShot = 4f; // User Req: 4
    [SerializeField] float coolingRate = 10f;    // User Req: 10
    [SerializeField] float cooldownDelay = 0.5f; // Time before cooling starts after shooting
    
    [Tooltip("How long to disable weapons after hitting 100% heat")]
    [SerializeField] float overheatDuration = 2f; // User Req: 2s

    // State
    public float CurrentHeat { get; private set; }
    public bool IsOverheated { get; private set; }
    
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
                CurrentHeat -= coolingRate * Time.deltaTime;
                CurrentHeat = Mathf.Max(CurrentHeat, 0);
            }
        }
    }

    public bool TryFire()
    {
        if (IsOverheated) return false;

        AddHeat();
        return true;
    }

    void AddHeat()
    {
        lastFireTime = Time.time; // Reset cooldown timer
        CurrentHeat += heatGainPerShot;

        if (CurrentHeat >= maxHeat)
        {
            CurrentHeat = maxHeat;
            StartCoroutine(OverheatRoutine());
        }
    }

    IEnumerator OverheatRoutine()
    {
        IsOverheated = true;
        OnOverheat?.Invoke(); // UI Flash

        // User Req: "Shooting stops for 2 seconds"
        // And implied: No cooling during this time (ProcessCooling checks IsOverheated)
        yield return new WaitForSeconds(overheatDuration);

        // User Req: "Starts lowering again... does not directly go to zero"
        // So we just unlock the gun. ProcessCooling will kick in next frame (since Time > lastFireTime).
        IsOverheated = false;
        OnCooled?.Invoke();
    }
    
    // Helper for Damage calculation (GDD: "High Heat = High Damage")
    public float GetHeatPercentage()
    {
        return CurrentHeat / maxHeat;
    }
}
