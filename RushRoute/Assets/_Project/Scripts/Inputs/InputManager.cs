using UnityEngine;

namespace RushRoute.Inputs
{
    /// <summary>
    /// Handles all Input reading from the New Input System.
    /// Acts as a wrapper so other scripts via Singleton doesn't need to know about "DeliveryInput".
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        private DeliveryInput _inputActions;

        public Vector2 MoveInput { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _inputActions = new DeliveryInput();
        }

        private void OnEnable()
        {
            _inputActions.Enable();
        }

        private void OnDisable()
        {
            _inputActions.Disable();
        }

        private void Update()
        {
            // Read value frame-by-frame
            MoveInput = _inputActions.Driving.MOVE.ReadValue<Vector2>();
        }
    }
}
