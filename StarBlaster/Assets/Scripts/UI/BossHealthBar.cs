using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    [SerializeField] Slider healthSlider;
    [SerializeField] GameObject visualContainer; // Parent object to hide/show

    BossController currentBoss;

    void Start()
    {
        // Start hidden
        if (visualContainer != null) visualContainer.SetActive(false);
    }

    void Update()
    {
        // Poll for boss if none
        if (currentBoss == null)
        {
            // Expensive? Do it every 1s? For prototype, Update is fine or use WaveManager event.
            // Optimized: WaveManager could trigger this. But finding is robust.
            currentBoss = FindFirstObjectByType<BossController>();
            
            if (currentBoss != null)
            {
                // Found boss!
                ConnectToBoss();
            }
        }
    }

    void ConnectToBoss()
    {
        if (visualContainer != null) visualContainer.SetActive(true);
        
        Health bossHealth = currentBoss.GetComponent<Health>();
        if (bossHealth != null)
        {
            // Initial Set
            UpdateBar(bossHealth.GetHealthPercentage());
            // Subscribe
            bossHealth.OnHealthChanged += UpdateBar;
        }
        
        // Listen for death to hide? BossController destroys itself on death.
        // We can check null in Update or rely on OnDestroy logic?
    }

    void UpdateBar(float percent)
    {
        if (healthSlider != null)
        {
            healthSlider.value = percent;
        }
        
        // Hide if dead
        if (percent <= 0 && visualContainer != null)
        {
            visualContainer.SetActive(false);
            currentBoss = null; // Search again for next boss
        }
    }
}
