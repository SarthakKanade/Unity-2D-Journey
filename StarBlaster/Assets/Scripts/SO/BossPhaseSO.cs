using UnityEngine;

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
    
    [Header("Spawning (Phase 2)")]
    public bool enableAdds = false;
    public float spawnInterval = 12f;
    public int maxAddsAlive = 2; // Default for Phase 1
    public GameObject addsPrefab;
}
