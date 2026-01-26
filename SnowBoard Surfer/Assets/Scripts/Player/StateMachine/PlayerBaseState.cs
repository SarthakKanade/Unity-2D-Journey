using UnityEngine;

public abstract class PlayerBaseState
{
    protected PlayerStateMachine player;
    protected PlayerStateMachine.PlayerStateFactory factory;

    public PlayerBaseState(PlayerStateMachine currentContext, PlayerStateMachine.PlayerStateFactory playerStateFactory)
    {
        player = currentContext;
        factory = playerStateFactory;
    }

    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void FixedUpdateState();
    public abstract void ExitState();
    public abstract void CheckSwitchStates();
    public abstract void InitializeSubState();

    public void UpdateStates()
    {
        UpdateState();
        if (_currentSubState != null)
        {
            _currentSubState.UpdateStates();
        }
    }

    public void FixedUpdateStates() 
    {
        FixedUpdateState();
        if (_currentSubState != null)
        {
            _currentSubState.FixedUpdateStates();
        }
    }

    public void ExitStates()
    {
        ExitState();
        if (_currentSubState != null)
        {
            _currentSubState.ExitStates();
        }
    }

    protected void SwitchState(PlayerBaseState newState)
    {
        // Exit current state logic
        ExitState();

        // Enter new state logic
        newState.EnterState();

        if (_isRootState) 
        {
            player.CurrentState = newState;
        }
        else if (_currentSuperState != null)
        {
            _currentSuperState.SetSubState(newState);
        }
    }

    protected void SetSuperState(PlayerBaseState newSuperState)
    {
        _currentSuperState = newSuperState;
    }

    protected void SetSubState(PlayerBaseState newSubState)
    {
        _currentSubState = newSubState;
        newSubState.SetSuperState(this);
    }

    private PlayerBaseState _currentSuperState; 
    private PlayerBaseState _currentSubState; 
    protected bool _isRootState = false; 
}
