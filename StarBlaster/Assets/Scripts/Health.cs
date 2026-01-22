using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] bool isPlayer;
    [SerializeField] int scoreValue = 50;
    [SerializeField] int health = 100;
    int maxHealth; // NEW: Track max health for percentage bars
    [SerializeField] ParticleSystem hitParticles;

    [SerializeField] bool applyCameraShake;
    CameraShake cameraShake;
    AudioManager audioManager;
    ScoreKeeper scoreKeeper;
    LevelManager levelManager;

    // NEW: Events for UI and Boss Phases
    public event System.Action<float> OnHealthChanged;


    void Awake()
    {
        maxHealth = health;
    }

    void Start()
    {
        cameraShake = Camera.main.GetComponent<CameraShake>();
        audioManager = FindFirstObjectByType<AudioManager>();
        scoreKeeper = FindFirstObjectByType<ScoreKeeper>();
        levelManager = FindFirstObjectByType<LevelManager>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        DamageDealer damageDealer = other.GetComponent<DamageDealer>();

        if (damageDealer != null)
        {
            TakeDamage(damageDealer.GetDamage());
            PlayHitParticles();
            damageDealer.Hit();
            audioManager.PlayDamageSFX();

            if (applyCameraShake)
            {
                cameraShake.Play();
            }
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        OnHealthChanged?.Invoke(GetHealthPercentage());
        
        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isPlayer)
        {
            levelManager.LoadGameOver();
        }
        else
        {
            scoreKeeper.ModifyScore(scoreValue);
        }
        Destroy(gameObject);
    }

    void PlayHitParticles()
    {
        if (hitParticles != null)
        {
            ParticleSystem particles = Instantiate(hitParticles, transform.position, Quaternion.identity);
            Destroy(particles, particles.main.duration + particles.main.startLifetime.constantMax);
        }
    }

    public int GetHealth()
    {
        return health;
    }

    // NEW: Allow initialization from SO
    public void SetMaxHealth(int amount)
    {
        health = amount;
        maxHealth = amount;
    }

    public void IncreaseMaxHealth(int amount)
    {
        maxHealth += amount;
        health += amount; 
        OnHealthChanged?.Invoke(GetHealthPercentage());
    }

    public void HealPercent(float percent)
    {
        int healAmount = Mathf.RoundToInt(maxHealth * percent);
        health += healAmount;
        health = Mathf.Min(health, maxHealth);
        OnHealthChanged?.Invoke(GetHealthPercentage());
    }

    public float GetHealthPercentage()
    {
        return (float)health / maxHealth;
    }
}
