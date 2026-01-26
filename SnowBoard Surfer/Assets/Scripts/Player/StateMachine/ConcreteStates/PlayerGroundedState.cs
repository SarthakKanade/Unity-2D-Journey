using UnityEngine;

public class PlayerGroundedState : PlayerBaseState
{
    public PlayerGroundedState(PlayerStateMachine currentContext, PlayerStateMachine.PlayerStateFactory playerStateFactory)
        : base(currentContext, playerStateFactory)
    {
        _isRootState = true;
        InitializeSubState();
    }

    private bool _isBoosting;

    public override void EnterState()
    {
        Debug.Log("Entered Grounded State");
        player.InputReader.BoostEvent += OnBoost;
        player.InputReader.JumpEvent += OnJump;
    }

    public override void ExitState()
    {
        player.InputReader.BoostEvent -= OnBoost;
        player.InputReader.JumpEvent -= OnJump;
    }

    public override void UpdateState()
    {
        CheckSwitchStates();
    }

    public override void FixedUpdateState()
    {
        HandleSpeed();
    }

    public override void CheckSwitchStates()
    {
        if (!player.IsGrounded) 
        {
             SwitchState(factory.Airborne());
        }
    }

    public override void InitializeSubState()
    {
    }

    private void OnJump(bool pressed)
    {
        if (pressed)
        {
            // Apply Immediate Force
            player.Rb.AddForce(Vector2.up * player.JumpForce);
        }
    }

    private void OnBoost(bool boosting)
    {
        _isBoosting = boosting;
    }

    private void HandleSpeed()
    {
        if (_isBoosting)
        {
            player.SurfaceEffector.speed = player.BoostSpeed;
        }
        else
        {
            player.SurfaceEffector.speed = player.BaseSpeed;
        }
    }
}
