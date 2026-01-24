using System.Collections;
using UnityEngine;

public class BossSentinel : BossController
{
    protected override void HandleAttacks()
    {
        if (player == null || isAttacking) return;

        barrageTimer += Time.deltaTime;
        laserTimer += Time.deltaTime;

        // 1. LASER
        if (laserTimer >= currentConfig.laserCooldown)
        {
            StartCoroutine(FireSweepLaser());
            laserTimer = 0;
            return;
        }

        // 2. BARRAGE
        if (barrageTimer >= currentConfig.barrageCooldown)
        {
            StartCoroutine(FireArcBarrage());
            barrageTimer = 0;
        }
    }


}
