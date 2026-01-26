using UnityEngine;
using UnityEngine.InputSystem;
using System;

[CreateAssetMenu(fileName = "InputReader", menuName = "SnowBoard/InputReader")]
public class InputReader : ScriptableObject, InputSystem_Actions.IPlayerActions
{
    public event Action<Vector2> MoveEvent;
    public event Action<bool> BoostEvent; // True = Pressed, False = Released
    public event Action<bool> JumpEvent;  // True = Pressed, False = Released
    
    private InputSystem_Actions gameInput;

    private void OnEnable()
    {
        if (gameInput == null)
        {
            gameInput = new InputSystem_Actions();
            gameInput.Player.SetCallbacks(this);
        }
        
        gameInput.Player.Enable();
    }

    private void OnDisable()
    {
        gameInput?.Player.Disable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        MoveEvent?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnBoost(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            BoostEvent?.Invoke(true);
        else if (context.phase == InputActionPhase.Canceled)
            BoostEvent?.Invoke(false);
    }

    public void OnJump(InputAction.CallbackContext context) 
    {
        if (context.phase == InputActionPhase.Performed)
            JumpEvent?.Invoke(true);
        else if (context.phase == InputActionPhase.Canceled)
            JumpEvent?.Invoke(false);
    }
}
