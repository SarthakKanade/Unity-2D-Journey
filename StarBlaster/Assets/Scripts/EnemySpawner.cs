using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Simple Wave Settings")]
    [SerializeField] GameObject[] enemyPrefabs;
    [SerializeField] Transform[] spawnPoints;
    [SerializeField] float spawnRate = 2f;
    [SerializeField] bool isLooping = true;

    void Start()
    {
        // Manager Controlled - No auto start
    }

    public void SpawnSpecificEnemy(GameObject enemyPrefab)
    {
        if (enemyPrefab == null) return;

        // 2. Pick Random Spawn Point (Off-screen)
        int spawnIndex = Random.Range(0, spawnPoints.Length);
        Transform point = spawnPoints[spawnIndex];

        // 3. Spawn
        GameObject newEnemy = Instantiate(enemyPrefab, point.position, Quaternion.Euler(0, 0, 180));
        newEnemy.transform.SetParent(transform);
    }
}
