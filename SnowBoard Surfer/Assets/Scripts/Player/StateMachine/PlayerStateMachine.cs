using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    // State Variables
    public PlayerBaseState CurrentState { get; set; }

    // References
    [Header("References")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SurfaceEffector2D surfaceEffector;

    [Header("Settings")]
    [SerializeField] float torqueAmount = 20f;
    [SerializeField] float baseSpeed = 25f;
    [SerializeField] float boostSpeed = 35f;
    [SerializeField] float jumpForce = 500f; // New Setting
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Collider2D playerCollider;

    // Modifiers
    private float _speedModifier = 0f;
    private float _torqueModifier = 0f;

    // Public Getters for Settings (Calculated)
    public InputReader InputReader => inputReader;
    public Rigidbody2D Rb => rb;
    public SurfaceEffector2D SurfaceEffector => surfaceEffector;
    public float TorqueAmount => torqueAmount + _torqueModifier;
    public float BaseSpeed => baseSpeed + _speedModifier;
    public float BoostSpeed => boostSpeed + _speedModifier;
    public float JumpForce => jumpForce; // Getter

    public void ApplySpeedModifier(float amount) => _speedModifier += amount;
    public void ApplyTorqueModifier(float amount) => _torqueModifier += amount;

    public bool IsGrounded { get; private set; }

    // State Factory responsible for creating instances
    private PlayerStateFactory _states;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        surfaceEffector = FindFirstObjectByType<SurfaceEffector2D>();
        
        _states = new PlayerStateFactory(this);
        
        CurrentState = _states.Grounded();
        CurrentState.EnterState();
    }

    private void Start()
    {
        // Subscribe to Input
        // inputReader.MoveEvent += ... (handled in states?)
        // Actually, states can access inputReader directly via Context.
    }

    private void Update()
    {
        CheckGround();

        if (CurrentState != null)
        {
            CurrentState.UpdateStates();
        }
    }

    private void CheckGround()
    {
        // Simple overlap check
        IsGrounded = playerCollider.IsTouchingLayers(groundLayer);
    }

    private void FixedUpdate()
    {
        if (CurrentState != null)
        {
            CurrentState.FixedUpdateStates();
        }
    }

    // Inner Factory Class to manage state instantiation
    public class PlayerStateFactory
    {
        PlayerStateMachine _context;

        public PlayerStateFactory(PlayerStateMachine currentContext)
        {
            _context = currentContext;
        }

        public PlayerBaseState Grounded() { return new PlayerGroundedState(_context, this); }
        public PlayerBaseState Airborne() { return new PlayerAirborneState(_context, this); }
    }
}
