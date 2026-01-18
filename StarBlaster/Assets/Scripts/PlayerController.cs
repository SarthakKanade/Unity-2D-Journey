using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] float padding = 0.5f;

    [Header("Components")]
    [SerializeField] Rigidbody2D rb;
    Shooter playerShooter;

    // Input Actions (Professional Class Way)
    GameInput gameInput;
    
    // State Variables
    Vector2 moveInput;
    Vector2 minBounds;
    Vector2 maxBounds;
    Vector2 mousePosition;

    void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        playerShooter = GetComponent<Shooter>();

        // Initialize the generated Input Class
        gameInput = new GameInput();
    }

    void OnEnable()
    {
        // Professional Practice: Enable input when the object is enabled.
        gameInput.Player.Enable();
    }

    void OnDisable()
    {
        // Professional Practice: Disable input when the object is disabled (e.g. death, pause).
        gameInput.Player.Disable();
    }

    void Start()
    {
        InitBounds();
    }

    void Update()
    {
        ProcessInput();
        FireShooter();
    }

    void FixedUpdate()
    {
        ProcessMovement();
        ProcessRotation();
    }

    void InitBounds()
    {
        Camera mainCamera = Camera.main;
        minBounds = mainCamera.ViewportToWorldPoint(new Vector2(0, 0));
        maxBounds = mainCamera.ViewportToWorldPoint(new Vector2(1, 1));
    }

    void ProcessInput()
    {
        // Read values directly from the Class
        moveInput = gameInput.Player.Move.ReadValue<Vector2>();
        
        mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    }

    void ProcessMovement()
    {
        Vector2 currentPos = rb.position;
        Vector2 displacement = moveInput * moveSpeed * Time.fixedDeltaTime;
        
        Vector2 targetPos = currentPos + displacement;

        targetPos.x = Mathf.Clamp(targetPos.x, minBounds.x + padding, maxBounds.x - padding);
        targetPos.y = Mathf.Clamp(targetPos.y, minBounds.y + padding, maxBounds.y - padding);

        rb.MovePosition(targetPos);
    }

    void ProcessRotation()
    {
        Vector2 lookDir = mousePosition - rb.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
        rb.rotation = angle;
    }

    void FireShooter()
    {
        if(playerShooter != null)
        {
            // Read button state directly from the Class
            playerShooter.isFiring = gameInput.Player.Fire.IsPressed();
        }
    }
}
