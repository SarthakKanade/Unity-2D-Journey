using System.Collections;
using UnityEngine;

public class LaserBeam : MonoBehaviour
{
    [SerializeField] LineRenderer warningLine;
    [SerializeField] LineRenderer activeLaser;
    [SerializeField] BoxCollider2D laserCollider;
    
    int damagePerSec;
    bool isFiring;

    void Awake()
    {
        // Ensure everything starts off
        if (warningLine) warningLine.enabled = false;
        if (activeLaser) activeLaser.enabled = false;
        if (laserCollider) laserCollider.enabled = false;
    }

    public void Setup(int damage)
    {
        damagePerSec = damage;
    }

    public IEnumerator FireRoutine(float warningDuration, float fireDuration)
    {
        // 1. Warning Phase
        if (warningLine) warningLine.enabled = true;
        yield return new WaitForSeconds(warningDuration);
        if (warningLine) warningLine.enabled = false;

        // 2. Fire Phase
        isFiring = true;
        if (activeLaser) activeLaser.enabled = true;
        if (laserCollider) laserCollider.enabled = true;

        yield return new WaitForSeconds(fireDuration);

        // 3. Cooldown / End
        StopFiring();
    }

    public void StopFiring()
    {
        isFiring = false;
        if (activeLaser) activeLaser.enabled = false;
        if (laserCollider) laserCollider.enabled = false;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!isFiring) return;
        
        // Use Health.TakeDamage? Or continuous?
        // Let's deal damage per tick (Frame rate independent?)
        // Easiest is to just deal discrete damage periodically or small amounts every frame.
        // Given Health is int, we accumulate float damage? Or just dealing 1 damage often?
        
        // Let's verify if other has Health
        Health health = other.GetComponent<Health>();
        if (health != null)
        {
            // Simple approach: Deal damage every frame scaled by time? 
            // Warning: Int damage means we need accumulator.
            // For now, let's just deal damage if we haven't this frame?
            // Actually, "15 damage / sec" -> 15 * Time.deltaTime.
            // If delta is small, damage is 0.
            
            // Allow integer arithmetic:
            // Chance to take 1 damage = (DamagePerSec * dt)
            // Or use a timer per target? Too complex.
            
            // Simplest: `damageDealer` approach.
            // But this is OnTriggerStay.
            
            // Let's try: Health.TakeDamage expects int.
            // Let's implement a 'Tick' system in player health? No.
            
            // I'll just use a randomized approach for now to simulate float damage over time with ints.
            float expectedDamage = damagePerSec * Time.deltaTime;
            if (Random.value < expectedDamage) 
            {
                health.TakeDamage(1); 
            }
        }
    }
}
