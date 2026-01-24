using System.Collections;
using UnityEngine;

public class BossShield : MonoBehaviour
{
    [SerializeField] int maxHealth = 1500;
    [SerializeField] int currentHealth;
    [SerializeField] float damageAbsorption = 0.7f; // 70% to shield
    
    [Header("Visuals")]
    [SerializeField] SpriteRenderer shieldVisual;
    [SerializeField] Color healthyColor = new Color(0, 0.5f, 1f, 0.4f);
    [SerializeField] Color damagedColor = new Color(1f, 0, 0, 0.4f);
    
    [Header("Pulse")]
    [SerializeField] float pulseRadius = 5f;

    public bool IsActive => currentHealth > 0;
    public int CurrentHealth => currentHealth;

    public event System.Action OnShieldBroken;

    public void Init(int hp)
    {
        maxHealth = hp;
        currentHealth = hp;
        UpdateVisuals();
    }

    public int ProcessDamage(int incomingDamage)
    {
        if (currentHealth <= 0) return incomingDamage;

        int absorbed = Mathf.RoundToInt(incomingDamage * damageAbsorption);
        int passthrough = incomingDamage - absorbed;

        currentHealth -= absorbed;
        
        // Visual Feedback (Flash?)
        StartCoroutine(FlashShield());

        if (currentHealth <= 0)
        {
            BreakShield();
            return passthrough;
        }

        UpdateVisuals();
        return passthrough;
    }

    void UpdateVisuals()
    {
        if (shieldVisual == null) return;
        
        float pct = (float)currentHealth / maxHealth;
        shieldVisual.color = Color.Lerp(damagedColor, healthyColor, pct);
    }
    
    IEnumerator FlashShield()
    {
        if (shieldVisual) shieldVisual.enabled = false;
        yield return new WaitForSeconds(0.05f);
        if (shieldVisual && currentHealth > 0) shieldVisual.enabled = true;
    }

    void BreakShield()
    {
        currentHealth = 0;
        if (shieldVisual) shieldVisual.enabled = false;
        
        // FX
        // Play Sound?
        
        Debug.Log("BOSS SHIELD BROKEN!");
        OnShieldBroken?.Invoke();
    }

    // Pulse Logic called by BossController
    public IEnumerator TriggerPulse(int damage, float pushForce)
    {
        if (currentHealth <= 0) yield break;

        // Visual: Procedural Pulse
        StartCoroutine(AnimatePulseVisual());
        
        // Logic: OverlapCircle
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pulseRadius);
        foreach(var hit in hits)
        {
            // Support child colliders by checking Parent
            PlayerController pc = hit.GetComponent<PlayerController>();
            if (pc == null) pc = hit.GetComponentInParent<PlayerController>();

            if (pc != null)
            {
                // Push (Knockback via Controller)
                // Multiply by 5 to make it feel like a heavy impact relative to moveSpeed
                Vector2 dir = (hit.transform.position - transform.position).normalized;
                pc.ApplyKnockback(dir * pushForce * 5f);
                
                // Damage (Apply to Health)
                Health h = hit.GetComponent<Health>();
                if (h == null) h = hit.GetComponentInParent<Health>();
                
                if (h) h.TakeDamage(damage);
            }
        }

        yield return null;
    }

    IEnumerator AnimatePulseVisual()
    {
        if (shieldVisual == null) yield break;

        // 1. Create Temp Object
        GameObject pulseObj = new GameObject("BossShieldPulse");
        pulseObj.transform.position = transform.position;
        pulseObj.transform.rotation = transform.rotation;
        pulseObj.transform.SetParent(transform); 
        pulseObj.transform.localScale = Vector3.one; // Starts matching shield size currently

        // 2. Setup Sprite
        SpriteRenderer sr = pulseObj.AddComponent<SpriteRenderer>();
        sr.sprite = shieldVisual.sprite;
        sr.color = shieldVisual.color; // Start with same color
        sr.sortingOrder = shieldVisual.sortingOrder - 1; 

        // 3. Animate
        float duration = 0.5f;
        float elapsed = 0f;
        
        Vector3 startScale = Vector3.one * 0.1f; // Start small
        // Use LOSSY SCALE (Global) to ensure we match World Radius regardless of Boss Scale
        // worldDiameter = pulseRadius * 2
        // parentWorldScale = transform.lossyScale.x
        // targetLocalScale = worldDiameter / parentWorldScale
        
        float parentWorldScale = Mathf.Abs(transform.lossyScale.x);
        if (parentWorldScale < 0.001f) parentWorldScale = 1f;

        float targetScaleFactor = (pulseRadius * 2f) / parentWorldScale;
        Vector3 targetScale = Vector3.one * targetScaleFactor;
        
        Color startColor = sr.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, 0f); // Fade to alpha 0

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            // Ease Out Quad
            t = t * (2 - t);

            if (pulseObj != null)
            {
                pulseObj.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                sr.color = Color.Lerp(startColor, targetColor, t);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (pulseObj != null) Destroy(pulseObj);
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pulseRadius);
    }
}
