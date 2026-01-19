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
        if (spawnPoints.Length > 0 && enemyPrefabs.Length > 0)
        {
            StartCoroutine(SpawnEnemies());
        }
        else
        {
            Debug.LogWarning("EnemySpawner missing Prefabs or SpawnPoints!");
        }
    }

    IEnumerator SpawnEnemies()
    {
        // Initial delay
        yield return new WaitForSeconds(1f);

        do
        {
            // 1. Pick Random Enemy
            int enemyIndex = Random.Range(0, enemyPrefabs.Length);
            GameObject enemyToSpawn = enemyPrefabs[enemyIndex];

            // 2. Pick Random Spawn Point (Off-screen)
            int spawnIndex = Random.Range(0, spawnPoints.Length);
            Transform point = spawnPoints[spawnIndex];

            // 3. Spawn
            GameObject newEnemy = Instantiate(enemyToSpawn, point.position, Quaternion.Euler(0, 0, 180));
            newEnemy.transform.SetParent(transform);

            // 4. Wait
            yield return new WaitForSeconds(spawnRate);

        } while (isLooping);
    }
}
