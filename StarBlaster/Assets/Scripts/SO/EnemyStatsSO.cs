using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyStats", menuName = "Stats/Enemy Stats")]
public class EnemyStatsSO : ScriptableObject
{
    [Header("Defense Stats")]
    public int health = 50;
    public int scoreValue = 100;
    
    [Header("Movement Stats")]
    public float moveSpeed = 5f;
    public PathType preferredPath; // NEW: Determines which paths this enemy can use

    [Header("Weapon Stats")]
    public GameObject projectilePrefab; // Assign different sprites (Blue/Red/Big) here
    public int damage = 10;
    public float projectileSpeed = 10f;
    public float fireRate = 0.5f;     // For Grunt/Tank spam
    public float fireVariance = 0.1f;
}
