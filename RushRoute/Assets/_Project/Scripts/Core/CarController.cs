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

        public void SetCarData(CarData newData)
        {
            carData = newData;
        }

        private float GetWeightModifier()
        {
            // If we are carrying a HEAVY package, we are slower
            if (DeliveryManager.Instance != null && DeliveryManager.Instance.HasPackage)
            {
                var pkg = DeliveryManager.Instance.CurrentPackage;
                if (pkg != null && pkg.Type == PackageType.Heavy)
                {
                    // Strength 2.0 = Half Speed. Strength 1.0 = Normal.
                    return 1f / pkg.ModifierStrength;
                }
            }
            return 1f;
        }

        private void ApplyEngineForce()
        {
            float weightMod = GetWeightModifier();

            float velocityDot = Vector2.Dot(transform.up, _rb.linearVelocity);
            float acceleration = _moveInput.y;

            // Apply Weight to Acceleration Limits
            float currentMaxSpeed = carData.MaxSpeed * weightMod;

            if (acceleration > 0 && velocityDot < currentMaxSpeed)
            {
                // Force is also reduced by weight
                _rb.AddForce(transform.up * acceleration * carData.Acceleration * weightMod);
            }
            else if (acceleration < 0 && velocityDot > -currentMaxSpeed / 2)
            {
                _rb.AddForce(transform.up * acceleration * carData.Acceleration * weightMod);
            }

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
            float minSpeedToTurn = 0.5f;
            if (_rb.linearVelocity.magnitude < minSpeedToTurn)
            {
                _rb.angularVelocity = 0;
                return;
            }

            float weightMod = GetWeightModifier();
            
            // Turn speed reduced by weight too
            float turnSpeed = -_moveInput.x * carData.TurnSpeed * weightMod;
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

        private void OnCollisionEnter2D(Collision2D collision)
        {
            float impactSpeed = collision.relativeVelocity.magnitude;
            
            if (DeliveryManager.Instance != null && DeliveryManager.Instance.HasPackage)
            {
                var pkg = DeliveryManager.Instance.CurrentPackage;

                if (pkg != null && pkg.Type == PackageType.Fragile)
                {
                    float breakThreshold = 5f; 
                    if (impactSpeed > breakThreshold)
                    {
                        Debug.Log($"CRASH! Impact: {impactSpeed}. Package Destroyed!");
                        DeliveryManager.Instance.ReportPackageBroken();
                    }
                }
            }
        }
        
    }
}
