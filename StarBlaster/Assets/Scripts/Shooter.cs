using System.Collections;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    [Header("Base Variables")]
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] float projectileSpeed = 10f;
    [SerializeField] float projectileLifetime = 5f;
    [SerializeField] float baseFireRate = 0.2f;

    [Header("AI Variables")]
    [SerializeField] bool useAI;
    [SerializeField] float minimumFireRate = 0.2f;
    [SerializeField] float fireRateVariance = 0f;

    [HideInInspector] public bool isFiring;
    Coroutine fireCoroutine;
    AudioManager audioManager;

    HeatSystem heatSystem;

    void Awake()
    {
        // Professional Practice: Get dependency in Awake
        heatSystem = GetComponent<HeatSystem>();
    }

    void Start()
    {
        audioManager = FindFirstObjectByType<AudioManager>();

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
            fireCoroutine = StartCoroutine(FireContinuously());
        }
        else if (!isFiring && fireCoroutine != null)
        {
            StopCoroutine(fireCoroutine);
            fireCoroutine = null;
        }
    }

    IEnumerator FireContinuously()
    {
        while (true)
        {
            // NEW: Check Heat System before firing
            // If we have a HeatSystem and it fails to fire (overheated), we wait.
            if (heatSystem != null)
            {
                if (!heatSystem.TryFire()) 
                {
                    // If failed to fire due to overheat, we just yield and try again next frame/loop
                    yield return null; 
                    continue;
                }
            }

            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

            projectile.transform.rotation = transform.rotation;

            Rigidbody2D projectileRB = projectile.GetComponent<Rigidbody2D>();
            projectileRB.linearVelocity = transform.up * projectileSpeed;

            Destroy(projectile, projectileLifetime);

            float waitTime = Random.Range(baseFireRate - fireRateVariance, baseFireRate + fireRateVariance);
            waitTime = Mathf.Clamp(waitTime, minimumFireRate, float.MaxValue);

            audioManager.PlayShootingSFX();

            yield return new WaitForSeconds(waitTime);
        }
    }
}
