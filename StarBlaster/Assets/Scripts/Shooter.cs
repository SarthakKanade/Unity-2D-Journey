using System.Collections;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    [Header("Base Variables")]
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] float projectileSpeed = 10f;
    [SerializeField] float projectileLifetime = 5f;
    [SerializeField] float baseFireRate = 0.15f;
    [SerializeField] int defaultDamage = 12; // BASE damage for Player or default enemies

    [Header("AI Variables")]
    [SerializeField] public bool useAI; 
    [SerializeField] float minimumFireRate = 0.2f;
    [SerializeField] float fireRateVariance = 0f;

    [HideInInspector] public bool isFiring;
    Coroutine fireCoroutine;
    AudioManager audioManager;
    HeatSystem heatSystem;

    // Runtime variables (can be overridden by SO)
    int currentDamage;
    
    // Perk Modifiers
    public float damageMultiplier = 1f;
    public float fireRateMultiplier = 1f; // Higher is faster (WaitTime / Multiplier)

    void Awake()
    {
        heatSystem = GetComponent<HeatSystem>();
        currentDamage = defaultDamage; // Start with default
    }

    // NEW: Called by Enemy.cs to inject stats from SO
    public void InitializeWeapon(GameObject prefab, float speed, int damage, float fireRate)
    {
        this.projectilePrefab = prefab;
        this.projectileSpeed = speed;
        this.currentDamage = damage;
        this.baseFireRate = fireRate;
    }

    void Start()
    {
        audioManager = FindFirstObjectByType<AudioManager>();

        // If useAI is checked in inspector, we start firing immediately.
        // Enemy.cs will TURN THIS OFF if it wants to control it manually.
        if (useAI)
        {
            isFiring = true;
        }
    }

    void Update()
    {
        Fire();
    }

    void Fire()
    {
        if (isFiring && fireCoroutine == null)
        {
            // Debug.Log($"{name} starting fire coroutine.");
            fireCoroutine = StartCoroutine(FireContinuously());
        }
        else if (!isFiring && fireCoroutine != null)
        {
            // Debug.Log($"{name} stopping fire coroutine.");
            StopCoroutine(fireCoroutine);
            fireCoroutine = null;
        }
    }

    IEnumerator FireContinuously()
    {
        while (true)
        {
            if (heatSystem != null)
            {
                if (!heatSystem.TryFire()) 
                {
                    yield return null; 
                    continue;
                }
            }

            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

            projectile.transform.rotation = transform.rotation;

            Rigidbody2D projectileRB = projectile.GetComponent<Rigidbody2D>();
            if (projectileRB != null)
            {
                projectileRB.linearVelocity = transform.up * projectileSpeed;
            }

            // NEW: Apply Damage from Config + Perks
            DamageDealer damageDealer = projectile.GetComponent<DamageDealer>();
            if (damageDealer != null)
            {
                int finalDamage = Mathf.RoundToInt(currentDamage * damageMultiplier);
                damageDealer.SetDamage(finalDamage);
            }

            Destroy(projectile, projectileLifetime);

            float waitTime = Random.Range(baseFireRate - fireRateVariance, baseFireRate + fireRateVariance);
            // Apply Perk Multiplier (Higher multiplier = Lower wait time)
            if (fireRateMultiplier > 0) waitTime /= fireRateMultiplier;
            
            waitTime = Mathf.Clamp(waitTime, minimumFireRate, float.MaxValue);

            audioManager.PlayShootingSFX();

            yield return new WaitForSeconds(waitTime);
        }
    }
}
