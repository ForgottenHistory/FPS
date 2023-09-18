using System.ComponentModel;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour, IInitialize
{
    enum WallState
    {
        None,
        Left,
        Right
    }

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

    [Header("Wall Jumping")]
    public float wallJumpHeight = 1f;
    public float wallJumpForce = 6f;
    public float wallJumpAngle = 45f;
    public float wallJumpDisableDirectionalControlDuration = 0.5f;
    public float wallJumpApplyForceDuration = 1f;
    public float wallJumpBunnyHopMultiplier = 0.2f;
    public float wallJumpGravityOnWall = 0.5f;
    public float wallJumpDetectionRadius = 0.5f;
    public LayerMask wallLayerMask;
    private WallState onWallState = WallState.None;
    private Vector3 wallJumpDirection;
    private Vector3 wallJumpNormal;

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
    private float wallJumpTimer = 10f; // Timer to keep track of time since last walljump
    private float verticalVelocity = 0f; // Keeping track of vertical velocity separately
    private float originalSpeed;
    private float originalWallJumpForce;
    private float originalGroundFriction;
    private bool canBunnyHop = false;
    private bool inAir = false;
    private bool isMoving = false;

    // Store the last frame's mouse position
    private Vector3 lastPosition;

    public bool isActive { get; set; }
    public bool isSprinting { get; set; } = false;
    public float rightInputMultiplier { get; set; } = 1.0f;
    public float leftInputMultiplier { get; set; } = 1.0f;

    /////////////////////////////////////////////////////////////////////////////////////
    // Initialize & Deinitialize
    /////////////////////////////////////////////////////////////////////////////////////

    public void Initialize()
    {
        characterController = GetComponent<CharacterController>();
        playerUI = GetComponent<PlayerUI>();

        originalSpeed = speed;
        originalGroundFriction = groundFriction;
        originalWallJumpForce = wallJumpForce;

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
        IncrementWallJumpTimer();

        // Apply movement effects
        ApplyForces();

        onWallState = WallDetection();

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
        // Clamp x with the input multipliers
        x = Mathf.Clamp(x, -leftInputMultiplier, rightInputMultiplier);

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

        // Check if on a wall in the move direction
        // If yes, nullify the move direction
        //move = CheckOnWall(move, x);
        playerUI.SetDebugText("X", x.ToString("F2"));
        playerUI.SetDebugText("Z", z.ToString("F2"));
        playerUI.SetDebugText("Move", move.ToString("F2"));
        return move;
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

        // Apply walljump effect if applicable
        if (wallJumpTimer < wallJumpApplyForceDuration)
        {
            // Move the character in the direction opposite of the wall
            characterController.Move(wallJumpDirection * wallJumpForce * Time.deltaTime);
            wallJumpForce -= originalWallJumpForce / wallJumpApplyForceDuration * Time.deltaTime;
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
        if (isSprinting)
        {
            move *= sprintMultiplier;
        }

        return move;
    }

    /////////////////////////////////////////////////////////////////////////////////////

    /**
    Vector3 CheckOnWall(Vector3 move, float x)
    {
        // Check if on a wall in the move direction
        // If yes, nullify the move direction
        if (OnWall())
        {
            if (onWallState == WallState.Left && x > 0)
            {
                move -= transform.right * x; // nullify the rightward movement
            }
            else if (onWallState == WallState.Right && x < 0)
            {
                move += transform.right * x; // nullify the leftward movement
            }
        }
        else if (wallJumpTimer < wallJumpDisableDirectionalControlDuration)
        {
            // If the player is in the wall jump state, disable movement in the direction of the wall
            if (x > 0 && !canMoveRight)
            {
                move -= transform.right * x; // nullify the rightward movement
            }
            else if (x < 0 && !canMoveLeft)
            {
                move += transform.right * x; // nullify the leftward movement
            }
        }
        return move;
    }
    */

    /////////////////////////////////////////////////////////////////////////////////////

    void CheckGravity()
    {
        if (inAir && verticalVelocity <= 0)
        {
            float gravityMultiplier = gravity;
            if (OnWall() && isMoving)
            {
                gravityMultiplier = wallJumpGravityOnWall;
            }
            verticalVelocity -= Time.deltaTime * gravityMultiplier;
        }
        else if (inAir && verticalVelocity > 0)
        {
            verticalVelocity -= Time.deltaTime * gravity;
        }
    }

    /////////////////////////////////////////////////////////////////////////////////////

    public void Jump()
    {
        if (OnWall())
        {
            // Jump off the wall to the left at an angle based on the side the wall is from the player
            bool onLeftSide = onWallState == WallState.Left ? true : false;

            // Jump direction is x angle from the wall normal
            Vector3 outwardsDirection = wallJumpNormal;

            Vector3 jumpDirection = Quaternion.AngleAxis(onLeftSide ? -wallJumpAngle : wallJumpAngle, Vector3.up) * outwardsDirection;
            WallJump(jumpDirection);
        }
        else
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
    }

    /////////////////////////////////////////////////////////////////////////////////////

    void WallJump(Vector3 jumpDirection)
    {
        playerUI.SetDebugText("Jump Direction", jumpDirection.ToString("F2"));
        verticalVelocity = Mathf.Sqrt(wallJumpHeight * 2f * gravity);
        
        // Disable movement in the direction of the wall for a short duration
        if (onWallState == WallState.Left)
        {
            leftInputMultiplier = 0.0f;
            rightInputMultiplier = 1.0f;
        }
        else if (onWallState == WallState.Right)
        {
            leftInputMultiplier = 1.0f;
            rightInputMultiplier = 0.0f;
        }

        if (canBunnyHop && isMoving)
        {
            speed = Mathf.Min(speed * bunnyHopMultiplier, maxSpeed);
        }
        canBunnyHop = true;
        bunnyHopTimer = 0f; // Reset the timer on each jump

        // Reset the timer
        wallJumpTimer = 0f;
        wallJumpDirection = jumpDirection;
        wallJumpForce = originalWallJumpForce;
    }

    /////////////////////////////////////////////////////////////////////////////////////

    WallState WallDetection()
    {
        if (IsGrounded())
        {
            return WallState.None;
        }

        // If player is going backwards do not stick to a wall
        if (Vector3.Dot(transform.forward, characterController.velocity) < 0)
        {
            return WallState.None;
        }

        RaycastHit hit;
        bool onWallLeft = false;
        bool onWallRight = false;

        onWallLeft = Physics.Raycast(transform.position, -transform.right, out hit, wallJumpDetectionRadius, wallLayerMask);

        if (onWallLeft == false)
            onWallRight = Physics.Raycast(transform.position, transform.right, out hit, wallJumpDetectionRadius, wallLayerMask);

        wallJumpNormal = hit.normal;
        return onWallLeft ? WallState.Left : onWallRight ? WallState.Right : WallState.None;
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

    /////////////////////////////////////////////////////////////////////////////////////

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
        if (OnWall())
        {
            return true;
        }
        return CheckDistanceDown(jumpHeightOffset);
    }

    /////////////////////////////////////////////////////////////////////////////////////

    bool OnWall()
    {
        return onWallState != WallState.None;
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

    void IncrementWallJumpTimer()
    {
        wallJumpTimer += Time.deltaTime; // Increment the timer each frame
        CheckWallJumpTimer();
    }

    public void CheckWallJumpTimer()
    {
        if (wallJumpTimer > wallJumpDisableDirectionalControlDuration)
        {
            if (wallJumpTimer > wallJumpApplyForceDuration)
            {
                wallJumpForce = originalWallJumpForce;
            }
        }
        else
        {
            leftInputMultiplier += Time.deltaTime / wallJumpDisableDirectionalControlDuration;
            rightInputMultiplier += Time.deltaTime / wallJumpDisableDirectionalControlDuration;

            leftInputMultiplier = Mathf.Clamp(leftInputMultiplier, 0.0f, 1.0f);
            rightInputMultiplier = Mathf.Clamp(rightInputMultiplier, 0.0f, 1.0f);
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
        playerUI.SetDebugText("Speed", speed.ToString("F2"));
        playerUI.SetDebugText("Velocity", characterController.velocity.ToString("F2"));
        playerUI.SetDebugText("Vertical Velocity", verticalVelocity.ToString("F2"));
        playerUI.SetDebugText("Bunny Hop Timer", bunnyHopTimer.ToString("F2"));
        playerUI.SetDebugText("Wall Jump Timer", wallJumpTimer.ToString("F2"));
        playerUI.SetDebugText("In air", inAir.ToString());
        playerUI.SetDebugText("On ground", IsGrounded().ToString());
        playerUI.SetDebugText("On wall", onWallState.ToString());
        playerUI.SetDebugText("Moving", isMoving.ToString());
        playerUI.SetDebugText("Can jump", CanJump().ToString());
        playerUI.SetDebugText("Left", leftInputMultiplier.ToString("F2"));
        playerUI.SetDebugText("Right", rightInputMultiplier.ToString("F2"));
    }

    /////////////////////////////////////////////////////////////////////////////////////
}
