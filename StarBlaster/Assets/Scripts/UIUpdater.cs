using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIUpdater : MonoBehaviour
{
    [Header("Player References")]
    [SerializeField] Health playerHealth;
    [SerializeField] HeatSystem playerHeat; // NEW

    [Header("Health Bar Visuals")]
    [SerializeField] Slider healthSlider;
    [SerializeField] Image healthFillImage;
    [SerializeField] Gradient healthGradient; // Pro: Use Gradient instead of hardcoded colors

    [Header("Heat Bar Visuals")]
    [SerializeField] Slider heatSlider;
    [SerializeField] Image heatFillImage;
    [SerializeField] Gradient heatGradient; 
    [SerializeField] float pulseSpeed = 5f;

    [Header("Score")]
    [SerializeField] TextMeshProUGUI scoreText;
    ScoreKeeper scoreKeeper;

    void Start()
    {
        scoreKeeper = FindFirstObjectByType<ScoreKeeper>();
        
        // AUTO-SETUP: If user forgot to drag references, find them automatically.
        if (playerHealth == null) playerHealth = FindFirstObjectByType<Health>();
        if (playerHeat == null) playerHeat = FindFirstObjectByType<HeatSystem>();

        // Setup Sliders
        if (healthSlider != null) { healthSlider.minValue = 0; healthSlider.maxValue = 1; }
        if (heatSlider != null) { heatSlider.minValue = 0; heatSlider.maxValue = 1; }
    }

    void Update()
    {
        UpdateScore();
        UpdateHealth();
        UpdateHeat();
    }

    void UpdateScore()
    {
        if (scoreKeeper != null)
        {
           scoreText.text = scoreKeeper.GetScore().ToString("000000000");
        }
    }

    void UpdateHealth()
    {
        // FIX: If player is destroyed (Dead), set Health Bar to 0 immediately.
        if(playerHealth == null) 
        {
            if (healthSlider != null) healthSlider.value = 0;
            return;
        }

        // 1. Get Percentage
        float healthPct = playerHealth.GetHealthPercentage();
        
        // 2. Update Slider
        if (healthSlider != null) healthSlider.value = healthPct;

        // 3. Update Color based on Gradient
        if (healthFillImage != null)
        {
            healthFillImage.color = healthGradient.Evaluate(healthPct);
        }
    }

    void UpdateHeat()
    {
        // Graceful fail if no heat system found
        if(playerHeat == null) 
        {
            // Debugging: Warn if we haven't found the heat system yet
            // Debug.LogWarning("UIUpdater: No HeatSystem found on Player!"); 
            return;
        }

        float heatPct = playerHeat.GetHeatPercentage();
        // Debug.Log($"UIUpdater Heat Pct: {heatPct}"); // Uncomment to see values in Console

        if (heatSlider != null) heatSlider.value = heatPct;

        if (heatFillImage != null)
        {
            Color targetColor = heatGradient.Evaluate(heatPct);

            // JUICE: Pulse effect at high heat (> 80%)
            if (heatPct > 0.8f)
            {
                float emission = Mathf.PingPong(Time.time * pulseSpeed, 0.5f) + 0.5f; 
                targetColor = targetColor * emission; 
            }

            heatFillImage.color = targetColor;
        }
    }
}
