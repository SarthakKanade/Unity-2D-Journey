using UnityEngine;

public class PlayerAirborneState : PlayerBaseState
{
    public PlayerAirborneState(PlayerStateMachine currentContext, PlayerStateMachine.PlayerStateFactory playerStateFactory)
        : base(currentContext, playerStateFactory)
    {
        _isRootState = true;
    }

    private float _moveX; // Changed back to float for Vector2.x
    private float _previousRotation;
    private float _totalRotation;

    public override void EnterState()
    {
        Debug.Log("Entered Airborne State");
        // Revert to Manual Rotation (A/D or Arrows)
        player.InputReader.MoveEvent += OnMove;
        
        _previousRotation = player.transform.rotation.eulerAngles.z;
        _totalRotation = 0f;
    }

    public override void ExitState()
    {
        player.InputReader.MoveEvent -= OnMove;
    }

    public override void UpdateState()
    {
        CheckSwitchStates();
        CalculateFlips();
    }
    
    public override void FixedUpdateState()
    {
        HandleRotation();
    }

    public override void CheckSwitchStates()
    {
         if (player.IsGrounded)
        {
             SwitchState(factory.Grounded());
        }
    }

    public override void InitializeSubState()
    {
    }

    private void CalculateFlips()
    {
        float currentRotation = player.transform.rotation.eulerAngles.z;
        
        _totalRotation += Mathf.DeltaAngle(_previousRotation, currentRotation);

        // Check for 360 degree flip (allow some leeway, e.g. 340)
        if (_totalRotation > 340 || _totalRotation < -340)
        {
            _totalRotation = 0;
            GameEvents.ReportScore(100); 
            Debug.Log("Flip Detected! Score +100");
        }

        _previousRotation = currentRotation;
    }

    private void OnMove(Vector2 input)
    {
        _moveX = input.x;
    }

    private void HandleRotation()
    {
        // Manual Rotation Control
        if (_moveX < 0)
        {
            player.Rb.AddTorque(player.TorqueAmount);
        }
        else if (_moveX > 0)
        {
            player.Rb.AddTorque(-player.TorqueAmount);
        }
    }
}
