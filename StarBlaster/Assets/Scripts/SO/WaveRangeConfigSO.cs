using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWaveRangeConfig", menuName = "Wave System/Wave Range Config")]
public class WaveRangeConfigSO : ScriptableObject
{
    [Header("Wave Range Definition")]
    [Tooltip("Start of this configuration range (inclusive)")]
    public int minWave;
    [Tooltip("End of this configuration range (inclusive)")]
    public int maxWave;

    [Header("Wave Logic Overrides")]
    [Tooltip("Multiplier for the base quota formula: 8 + (Wave * 2)")]
    public float quotaMultiplier = 1.0f;
    [Tooltip("Maximum enemies allowed on screen simultaneously")]
    public int maxEnemiesOnScreen = 6;

    [Header("Spawn Cadence")]
    public float spawnIntervalMin = 1.2f;
    public float spawnIntervalMax = 1.6f;

    [Header("Enemy Probabilities")]
    public List<EnemyWeight> enemyWeights;

    public GameObject GetRandomEnemyPrefab()
    {
        float totalWeight = 0f;
        foreach (var ew in enemyWeights)
        {
            totalWeight += ew.weight;
        }

        float randomValue = Random.Range(0, totalWeight);
        float cumulativeWeight = 0f;

        foreach (var ew in enemyWeights)
        {
            cumulativeWeight += ew.weight;
            if (randomValue <= cumulativeWeight)
            {
                return ew.enemyPrefab;
            }
        }

        // Fallback
        if (enemyWeights.Count > 0) return enemyWeights[0].enemyPrefab;
        return null;
    }
}

[System.Serializable]
public struct EnemyWeight
{
    public GameObject enemyPrefab;
    [Tooltip("Relative probability weight (e.g., 80 for 80%)")]
    public float weight;
}
