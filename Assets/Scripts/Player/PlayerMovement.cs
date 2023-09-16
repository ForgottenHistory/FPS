using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour, IInitialize
{
    /////////////////////////////////////////////////////////////////////////////////////
    // PUBLIC FIELDS
    /////////////////////////////////////////////////////////////////////////////////////

    [Header("Movement")]
    public float speed = 6f;
    public float gravity = -9.81f;
    public float sprintMultiplier = 1.5f;
    public float groundFriction = 1f;
    public float groundFrictionMultipier = 1f;  // Adjust this value as needed
    public float maxSpeed = 12f; // Maximum speed achievable through bunny hopping

    [Header("Crouching")]
    public float crouchHeight = 0.5f; // The height of the character while crouching
    public float crouchSpeedMultiplier = 0.5f; // Speed multiplier while crouching
    public float crouchTransitionDuration = 0.5f; // Duration of the transition.
    private float standingHeight; // To store the original height of the character
    private bool isCrouching = false; // To keep track of the crouching state

    [Header("Jumping")]
    public float jumpHeight = 1f;
    public float jumpHeightOffset = 0.1f;
    public float groundOffset = 0.1f;
    public float airControlMultiplier = 1.0f;
    public float airFriction = 0.1f;

    [Header("Bunny Hopping")]
    public float bunnyHopMultiplier = 1.02f; // Multiplier for each successful bunny hop
    public float bunnyHopGracePeriod = 0.2f; // Time in seconds to allow for next bunny hop
    public float strafeJumpVelocityMultiplier = 0.1f; // Multiplier for strafe jump velocity

    public CameraController cameraController;

    /////////////////////////////////////////////////////////////////////////////////////
    // PRIVATE FIELDS
    /////////////////////////////////////////////////////////////////////////////////////

    private CharacterController characterController;
    private PlayerUI playerUI;

    private float bunnyHopTimer = 0f; // Timer to keep track of time since last jump
    private float verticalVelocity = 0f; // Keeping track of vertical velocity separately
    private float originalSpeed;
    private float originalGroundFriction;
    private bool canBunnyHop = false;
    private bool inAir = false;
    private bool isMoving = false;

    // Store the last frame's mouse position
    private Vector2 lastMousePosition;
    private Vector3 lastPosition;

    public bool isActive { get; set; }
    public bool isSprinting { get; set; } = false;

    /////////////////////////////////////////////////////////////////////////////////////
    // Initialize & Deinitialize
    /////////////////////////////////////////////////////////////////////////////////////

    public void Initialize()
    {
        characterController = GetComponent<CharacterController>();
        playerUI = GetComponent<PlayerUI>();

        originalSpeed = speed;
        originalGroundFriction = groundFriction;

        lastMousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        lastPosition = transform.position;
        standingHeight = transform.localScale.y;

        isActive = true;
    }

    public void Deinitialize()
    {
        isActive = false;
    }

    /////////////////////////////////////////////////////////////////////////////////////'
    // UPDATES
    /////////////////////////////////////////////////////////////////////////////////////

    void Update()
    {
        if (isActive == false) return;

        // Increment timers
        IncrementBunnyHopTimer();

        // Apply movement effects
        ApplyForces();

        Vector2 mouseDelta = GetMouseDifferenceSinceLastFrame();

        // Apply strafe jumping logic if the player is in the air
        if (inAir)
        {
            //ApplyStrafeJumping(mouseDelta);
        }

        // Apply vertical movement
        characterController.Move(new Vector3(0f, verticalVelocity, 0f) * Time.deltaTime);

        DebugMode();
    }

    /////////////////////////////////////////////////////////////////////////////////////

    void FixedUpdate()
    {
        if (isActive == false) return;

        // Check if the player is in the air
        inAir = !IsGrounded();

        SetIsMoving();
    }

    /////////////////////////////////////////////////////////////////////////////////////

    public void MovePlayer(float x, float z)
    {
        // Apply horizontal movement
        Vector3 move = DirectionalMovement(x, z);

        // Put movement on the character controller
        characterController.Move(move * speed * Time.deltaTime);
    }

    /////////////////////////////////////////////////////////////////////////////////////

    Vector3 DirectionalMovement(float x, float z)
    {
        Vector3 move = transform.right * x + transform.forward * z;

        // Normalize the direction vector to prevent faster diagonal movement
        if (move.magnitude > 1f)
        {
            move.Normalize();
        }

        // Adjusting air control multiplier application
        if (inAir)
        {
            move *= airControlMultiplier; // Apply different control while in the air
        }

        // Apply sprinting
        move = CheckSprint(move);

        return move;
    }

    /////////////////////////////////////////////////////////////////////////////////////

    Vector2 GetMouseDifferenceSinceLastFrame()
    {
        // Get the current mouse position
        Vector2 currentMousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

        // Get the difference in mouse position since the last frame
        Vector2 mouseDelta = currentMousePosition - lastMousePosition;

        // Update the last mouse position for the next frame
        lastMousePosition = currentMousePosition;

        return mouseDelta;
    }

    /////////////////////////////////////////////////////////////////////////////////////

    void ApplyStrafeJumping(Vector2 mouseDelta)
    {
        // Calculate the desired strafe direction based on the mouse movement direction
        Vector3 strafeDirection = (transform.right * mouseDelta.x + transform.up * mouseDelta.y).normalized;

        // Get the player's current velocity
        Vector3 velocity = characterController.velocity;

        // Add the strafe direction to the player's velocity, scaled by a factor (tweak this value for the desired effect)
        velocity += strafeDirection * strafeJumpVelocityMultiplier;

        // Apply the new velocity to the character controller
        characterController.Move(velocity * Time.deltaTime);
    }

    /////////////////////////////////////////////////////////////////////////////////////

    void ApplyForces()
    {
        if (inAir == false && speed >= originalSpeed + 1)
        {
            groundFriction *= groundFrictionMultipier;
        }
        else
        {
            groundFriction = originalGroundFriction;
        }

        // Apply ground friction
        ApplyGroundFriction();

        // Apply gravity if applicable
        CheckGravity();
    }

    /////////////////////////////////////////////////////////////////////////////////////

    void ApplyGroundFriction()
    {
        if (!inAir)
        {
            speed -= groundFriction * Time.deltaTime;
            if (speed < originalSpeed)
            {
                speed = originalSpeed;
            }
        }
        else
        {
            speed = Mathf.Lerp(speed, originalSpeed, airFriction * Time.deltaTime);
        }
    }

    /////////////////////////////////////////////////////////////////////////////////////

    Vector3 CheckSprint(Vector3 move)
    {
        // Check if the player is sprinting by holding a key

        if (isSprinting)
        {
            move *= sprintMultiplier;
        }

        return move;
    }

    /////////////////////////////////////////////////////////////////////////////////////

    void CheckGravity()
    {
        if (inAir)
        {
            // Apply gravity
            verticalVelocity -= gravity * Time.deltaTime;
        }
    }

    /////////////////////////////////////////////////////////////////////////////////////

    public void Jump()
    {
        verticalVelocity = Mathf.Sqrt(jumpHeight * 2f * gravity);
        characterController.Move(new Vector3(0f, verticalVelocity, 0f) * Time.deltaTime);
        if (canBunnyHop && isMoving)
        {
            speed = Mathf.Min(speed * bunnyHopMultiplier, maxSpeed);
        }
        canBunnyHop = true;
        bunnyHopTimer = 0f; // Reset the timer on each jump
    }

    /////////////////////////////////////////////////////////////////////////////////////

    public void Crouch()
    {
        if (isCrouching)
        {
            // Stand up (only if there's enough space to stand)
            if (!Physics.Raycast(transform.position, Vector3.up, standingHeight))
            {
                StartCoroutine(ChangeHeight(standingHeight, crouchHeight, crouchTransitionDuration));
                isCrouching = false;
                originalSpeed /= crouchSpeedMultiplier; // Reset speed to normal
            }
        }
        else
        {
            // Crouch
            StartCoroutine(ChangeHeight(crouchHeight, standingHeight, crouchTransitionDuration));
            isCrouching = true;
            originalSpeed *= crouchSpeedMultiplier; // Reduce speed while crouching
        }
    }

    IEnumerator ChangeHeight(float targetHeight, float originalHeight, float duration)
    {
        float elapsedTime = 0f;
        float previousHeight = originalHeight;

        while (elapsedTime < duration)
        {
            float newHeight = Mathf.Lerp(originalHeight, targetHeight, (elapsedTime / duration));

            // Calculate the yOffset based on the difference between the new and previous height
            float yOffset = (newHeight - previousHeight) / 2.0f;

            characterController.height = newHeight;

            // Move the player up or down based on half the change in height since the last frame
            characterController.Move(new Vector3(0f, yOffset, 0f));

            // Update the previous height to the new height for the next iteration
            previousHeight = newHeight;

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        characterController.height = targetHeight;
    }


    /////////////////////////////////////////////////////////////////////////////////////

    public bool IsGrounded()
    {
        bool result = false;
        if (isCrouching)
        {
            result = CheckDistanceDown(crouchHeight);
        }
        else
        {
            result = CheckDistanceDown(groundOffset);
        }
        if (result)
            verticalVelocity = 0f;
        return result;
    }

    /////////////////////////////////////////////////////////////////////////////////////

    public bool CanJump()
    {
        return CheckDistanceDown(jumpHeightOffset);
    }

    /////////////////////////////////////////////////////////////////////////////////////

    bool CheckDistanceDown(float offset)
    {
        // Use a sphere to check if the player is grounded
        // Offset the sphere down a bit to have the center at the feet
        bool grounded = Physics.CheckSphere(transform.position - new Vector3(0f, offset, 0f), characterController.radius, LayerMask.GetMask("Ground"));
        return grounded;
    }

    /////////////////////////////////////////////////////////////////////////////////////

    void IncrementBunnyHopTimer()
    {
        bunnyHopTimer += Time.deltaTime; // Increment the timer each frame
        if (bunnyHopTimer > bunnyHopGracePeriod)
        {
            canBunnyHop = false; // Reset the bunny hop state if grace period is exceeded
        }
    }

    /////////////////////////////////////////////////////////////////////////////////////

    void SetIsMoving()
    {
        Vector3 currentPos = transform.position;
        currentPos.y = lastPosition.y; // Ignore the y component of the position
        float positionDelta = Vector3.Distance(currentPos, lastPosition);
        lastPosition = transform.position;

        // Check if the player is moving, at least by a small margin
        if (positionDelta > 0.01f)
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }
    }

    /////////////////////////////////////////////////////////////////////////////////////

    public void Reset()
    {
        // Reset the player's position
        characterController.enabled = false;
        transform.position = Vector3.up;
        speed = originalSpeed;
        verticalVelocity = 0f;
        characterController.enabled = true;
    }

    /////////////////////////////////////////////////////////////////////////////////////

    void DebugMode()
    {
        if (playerUI.isActive == false) return;

        // Update the debug UI
        playerUI.SetDebugSpeedText(speed);
        playerUI.SetDebugVelocityText(characterController.velocity);
        playerUI.SetDebugVerticalVelocityText(verticalVelocity);
    }

    /////////////////////////////////////////////////////////////////////////////////////
}
