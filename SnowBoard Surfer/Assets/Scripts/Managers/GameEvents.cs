using System;
using UnityEngine;

public static class GameEvents
{
    // Gameplay Events
    public static event Action<int> OnScoreGained;
    public static event Action OnPlayerCrash;
    public static event Action OnPlayerFinished;

    // Methods to Trigger Events (Safety Wrappers)
    public static void ReportScore(int amount) => OnScoreGained?.Invoke(amount);
    public static void ReportCrash() => OnPlayerCrash?.Invoke();
    public static void ReportFinish() => OnPlayerFinished?.Invoke();
}
