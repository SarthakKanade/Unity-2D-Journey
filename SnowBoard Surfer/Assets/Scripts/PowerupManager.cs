using UnityEngine;

public class PowerupManager : MonoBehaviour
{
    [SerializeField] PowerupSO powerup;

    PlayerStateMachine player;
    SpriteRenderer spriteRenderer;
    float timeLeft;

    void Start()
    {
        player = FindFirstObjectByType<PlayerStateMachine>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        timeLeft = powerup.GetTime();
    }

    void Update()
    {
        CountdownTimer();
    }

    void CountdownTimer()
    {
        if (spriteRenderer.enabled == false)
        {
            if (timeLeft > 0)
            {
                timeLeft -= Time.deltaTime;

                if (timeLeft <= 0)
                {
                    DeactivatePowerup();
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        int layerIndex = LayerMask.NameToLayer("Player");

        if (collision.gameObject.layer == layerIndex && spriteRenderer.enabled)
        {
            spriteRenderer.enabled = false;
            ActivatePowerup();
        }
    }

    private void ActivatePowerup()
    {
        if (powerup.GetPowerupType() == "speed")
        {
            player.ApplySpeedModifier(powerup.GetValueChange());
        }
        else if (powerup.GetPowerupType() == "torque")
        {
            player.ApplyTorqueModifier(powerup.GetValueChange());
        }
    }

    private void DeactivatePowerup() 
    {
        if (powerup.GetPowerupType() == "speed")
        {
            player.ApplySpeedModifier(-powerup.GetValueChange());
        }
        else if (powerup.GetPowerupType() == "torque")
        {
            player.ApplyTorqueModifier(-powerup.GetValueChange());
        }
    }
}