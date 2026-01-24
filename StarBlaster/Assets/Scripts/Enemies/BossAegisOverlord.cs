using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAegisOverlord : BossController
{
    [Header("Aegis References")]
    [SerializeField] BossShield shield;

    protected override void EnterPhase(BossPhase phase)
    {
        base.EnterPhase(phase);

        // Shield Logic (Moved from Base)
        if (shield != null)
        {
            if (currentConfig.shieldHealth > 0)
            {
                shield.gameObject.SetActive(true);
                shield.Init(currentConfig.shieldHealth);
                GetComponent<Health>().RegisterShield(shield);
            }
            else
            {
                shield.gameObject.SetActive(false);
            }
        }
    }

    protected override void HandleAttacks()
    {
        if (player == null || isAttacking) return;

        volleyTimer += Time.deltaTime;
        pulseTimer += Time.deltaTime;
        laserTimer += Time.deltaTime;
        barrageTimer += Time.deltaTime;

        // 1. SHIELD PULSE
        if (currentConfig.pulseDamage > 0 && shield != null && shield.IsActive)
        {
            if (pulseTimer >= currentConfig.pulseInterval)
            {
                StartCoroutine(DoShieldPulse());
                pulseTimer = 0;
                return;
            }
        }

        // 2. TARGETED VOLLEY
        if (currentConfig.targetedVolleyCount > 0)
        {
            if (volleyTimer >= currentConfig.targetedVolleyCooldown)
            {
                StartCoroutine(FireTargetedVolley());
                volleyTimer = 0;
                return;
            }
        }

        // 3. LASER (Inherited)
        if (laserTimer >= currentConfig.laserCooldown)
        {
            StartCoroutine(FireSweepLaser());
            laserTimer = 0;
            return;
        }

        // 4. BARRAGE (Inherited)
        if (barrageTimer >= currentConfig.barrageCooldown)
        {
            StartCoroutine(FireArcBarrage());
            barrageTimer = 0;
            return;
        }
    }

    // --- Specific Attack Coroutines ---

    IEnumerator DoShieldPulse()
    {
        isAttacking = true;
        yield return StartCoroutine(shield.TriggerPulse(currentConfig.pulseDamage, currentConfig.pulsePushForce));
        isAttacking = false;
    }

    IEnumerator FireTargetedVolley()
    {
        isAttacking = true;
        
        // Rapid shots aimed at player
        for (int i=0; i<currentConfig.targetedVolleyCount; i++)
        {
            if (currentConfig.projectilePrefab == null) break;
            
            // Aim
            Vector2 dir = Vector2.down;
            if (player) dir = (player.transform.position - transform.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            Quaternion rot = Quaternion.Euler(0,0,angle);

            // Fire
            GameObject bullet = Instantiate(currentConfig.projectilePrefab, transform.position, rot);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb) rb.linearVelocity = dir * currentConfig.targetedVolleySpeed;
             
            DamageDealer dd = bullet.GetComponent<DamageDealer>();
            if (dd) dd.SetDamage(currentConfig.projectileDamage);

            yield return new WaitForSeconds(0.2f); // Rapid fire delay
        }

        isAttacking = false;
    }

    // Override Spawning if needed (Base already handles Complex Spawning well enough)
}
