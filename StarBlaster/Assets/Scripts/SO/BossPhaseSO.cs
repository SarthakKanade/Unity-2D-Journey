using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "StarBlaster/Boss Phase Config", fileName = "NewBossPhase")]
public class BossPhaseSO : ScriptableObject
{
    [Header("Movement")]
    public float flowSpeed = 1.0f;
    public float driftAmplitude = 2.0f; // Side to side sway

    [Header("Attacks")]
    public float barrageCooldown = 3.5f;
    public float laserCooldown = 6.0f;
    
    [Header("Ordnance")]
    public GameObject projectilePrefab;
    public int projectileDamage = 10;
    public float projectileSpeed = 6f;
    
    [Header("Laser")]
    public int laserDamagePerSec = 15;
    public float laserDuration = 2.0f;

    
    [Header("Visuals")]
    public Color phaseColor = Color.white;
    
    [Header("Targeted Volley (Boss 2)")]
    public float targetedVolleyCooldown = 4.5f;
    public int targetedVolleyCount = 3;
    public float targetedVolleySpeed = 12f;

    [Header("Shield Pulse (Boss 2)")]
    public float pulseInterval = 5f;
    public int pulseDamage = 10;
    public float pulsePushForce = 5f;
    
    [Header("Shield Logic")]
    public int shieldHealth = 0; // If > 0, Shield Active

    [Header("Spawning (Phase 2)")]
    public bool enableAdds = false;
    public float spawnInterval = 12f;
    public int maxAddsAlive = 2; 
    public GameObject addsPrefab;
    
    // Complex Adds Support
    [Header("Complex Spawning")]
    public List<AddConfig> complexAdds;

    [System.Serializable]
    public struct AddConfig
    {
        public GameObject prefab;
        public int maxCount;
        public float spawnWeight;
    }
}
