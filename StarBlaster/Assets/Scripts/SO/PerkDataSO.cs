using UnityEngine;

[CreateAssetMenu(fileName = "NewPerkData", menuName = "Perks/Perk Data")]
public class PerkDataSO : ScriptableObject
{
    [Header("Identity")]
    public PerkID id;
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Classification")]
    public PerkCategory category;
    public PerkRarity rarity;
    public PerkTriggerType triggerType;

    [Header("Scaling Stats")]
    public int maxLevel = 3;
    [Tooltip("Value at Level 1")]
    public float baseValue;
    [Tooltip("Amount added per level (Level 2 = Base + Growth)")]
    public float growthValue;

    // Helper calculate value for a specific level
    public float GetValueAtLevel(int level)
    {
        if (level <= 0) return 0;
        // Formula: Base + (Growth * (Level - 1))
        return baseValue + (growthValue * (level - 1));
    }

    // Helper for description formatting (e.g., replaces {0} with current value)
    public string GetDescriptionForLevel(int level)
    {
        float val = GetValueAtLevel(level);
        return string.Format(description, val);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        // 1. Sync Display Name from File Name (Vice Versa case)
        // If display name is empty, grab the file name and make it pretty
        if (string.IsNullOrEmpty(displayName))
        {
            // "Perk_ReinforcedHull" -> "Reinforced Hull"
            string prettyName = name.Replace("Perk_", "");
            prettyName = System.Text.RegularExpressions.Regex.Replace(prettyName, "([a-z])([A-Z])", "$1 $2");
            displayName = prettyName;
        }
    }

    // 2. Button to Rename File (The "Forward" case)
    // We don't do this automatically in OnValidate to avoid infinite loops or typing lag
    // [ContextMenu("Rename File to Display Name")]
    // public void RenameFile()
    // {
    //     if (string.IsNullOrEmpty(displayName)) return;

    //     string assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);
    //     UnityEditor.AssetDatabase.RenameAsset(assetPath, displayName.Replace(" ", "")); // Remove spaces for file name
    //     UnityEditor.AssetDatabase.SaveAssets();
    // }
#endif
}

public enum PerkID
{
    // Common
    FieldRepair,
    ReinforcedHull,
    WeaponCalibration,
    HeatVenting,
    HeatDiscipline,
    RapidCycling,
    StabilityCore,

    // Rare
    LifeLeech,
    EmergencyCooling,
    KillMomentum,
    CoolingSurge,
    AdaptivePlating,

    // Epic
    SecondChance,
    TemporalFreeze,
    OverdriveCore
}

public enum PerkCategory
{
    Survival,
    Offense,
    Heat,
    Control
}

public enum PerkRarity
{
    Common,
    Rare,
    Epic
}

public enum PerkTriggerType
{
    AlwaysOn,       // Stat mod triggers immediately on aquire
    OnWaveEnd,      // Triggered by WaveManager
    OnKill,         // Triggered by Enemy Death
    OnHit,          // Triggered by Projectile Hit
    OnOverheat,     // Triggered by HeatSystem
    Conditional,    // Health threshold checks
    TimedInterval,  // Passive timer
    Special         // Unique behaviors (Second Chance, etc)
}
