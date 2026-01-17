using UnityEngine;
using RushRoute.Inputs;

namespace RushRoute.Core
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class CarController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private CarData carData;

        private Rigidbody2D _rb;
        private Vector2 _moveInput;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            // Safety Check: If InputManager is missing, don't crash, just warn (once per second ideally, but here just return)
            if (InputManager.Instance == null)
            {
                Debug.LogWarning("InputManager is missing from the Scene! Please create a GameObject and attach InputManager.cs");
                return;
            }

            // 1. Read Input from our decoupled Manager
            _moveInput = InputManager.Instance.MoveInput;
        }

        private void FixedUpdate()
        {
            if (carData == null) return;

            // 2. Handle Physics
            ApplyEngineForce();
            ApplySteering();
            ApplyDrift();
        }

        private void ApplyEngineForce()
        {
            // Acceleration: Only add force if below max speed
            // Dot product checks if we are already moving fast in the forward direction
            float velocityDot = Vector2.Dot(transform.up, _rb.linearVelocity);

            // Simple arcade acceleration
            // Pressing W (y=1) -> Add force forward
            // Pressing S (y=-1) -> Add force backward (Braking/Reversing)
            float acceleration = _moveInput.y;

            if (acceleration > 0 && velocityDot < carData.MaxSpeed)
            {
                _rb.AddForce(transform.up * acceleration * carData.Acceleration);
            }
            else if (acceleration < 0 && velocityDot > -carData.MaxSpeed / 2) // Reverse is slower
            {
                _rb.AddForce(transform.up * acceleration * carData.Acceleration);
            }

            // Drag: Apply linear drag if no input, so we roll to a stop
            if (acceleration == 0)
            {
                _rb.linearDamping = carData.Drag;
            }
            else
            {
                _rb.linearDamping = 0;
            }
        }

        private void ApplySteering()
        {
            // Don't turn if we aren't moving (Arcade feel)
            float minSpeedToTurn = 0.5f;
            if (_rb.linearVelocity.magnitude < minSpeedToTurn)
            {
                _rb.angularVelocity = 0; // Stop spinning if we stop moving!
                return;
            }

            // New Physics Steering: Directly set the Rotation Speed (Angular Velocity)
            // This is snappier and stops spinning immediately when you let go.
            float turnSpeed = -_moveInput.x * carData.TurnSpeed;
            _rb.angularVelocity = turnSpeed;
        }

        private void ApplyDrift()
        {
            // Orthogonal Velocity: The speed we are sliding sideways
            Vector2 forwardVelocity = transform.up * Vector2.Dot(_rb.linearVelocity, transform.up);
            Vector2 rightVelocity = transform.right * Vector2.Dot(_rb.linearVelocity, transform.right);

            // Kill the sideways velocity based on DriftFactor (Grip)
            // 0.0 = Ice (No grip, full slide)
            // 1.0 = Train (Full grip, no slide)
            _rb.linearVelocity = forwardVelocity + (rightVelocity * carData.DriftFactor);
        }
        
        public void SetCarData(CarData newData)
        {
            carData = newData;
        }
    }
}
