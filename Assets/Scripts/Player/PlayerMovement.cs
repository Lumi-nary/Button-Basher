using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using BezierSolution;
using MoreMountains.Feedbacks;

public class PlayerMovement : MonoBehaviour
{

    #region Variables
    // Speed
    public float moveSpeed = 0f;
    public float speedIncrease = 1f;
    public float maxSpeed = 20f;
    public float deceleRate = 2f;
    public float wrongPressPenalty = 3f;

    // Jump
    [Header("Jump Settings")]
    public KeyCode jumpKey = KeyCode.Space;
    public float jumpForce = 5f;
    public bool isGrounded = true;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    // Lane Switching
    [Header("Lane Switching Key Bindings")]
    [Tooltip("Player 1 Default Q. Player 2 Default Y")]
    public KeyCode leftSwitchKey = KeyCode.None;
    [Tooltip("Player 1 Default T. Player 2 Default P")]
    public KeyCode rightSwitchKey = KeyCode.None; // Renamed from rightSwithKey for consistent naming
    [Header("Lane Switching Settings")]
    [SerializeField]
    private int currentLane = 1; // 0 = Left, 1 = Center, 2 = Right
    public int maxLaneCharges = 3;
    public int currentLaneCharges = 3;
    public float laneSwitchSpeed = 10f;
    private bool isSwitchingLanes = false; // Used to control rapid charge consumption

    // Bezier Spline Movement
    [Header("Spline Settings")]
    [SerializeField] private BezierSpline spline; // Assign your race track spline here
    private float cachedSplineLength;
    private float normalizedT = 0f; // Current progress along the spline (0 to 1)
    private float lateralOffset = 0f; // Current lateral offset from the spline center
    private float targetLateralOffset = 0f; // Target lateral offset for lane switching
    public float laneOffsetAmount = 5f; // Distance for each lane from the center
    public float trapDeployOffset = -2f; // How far behind the player to spawn the trap along the spline

    public PlayerStamina playerStamina;

    [Header("Movement Key Bindings")]
    public KeyCode key1 = KeyCode.None;
    public KeyCode key2 = KeyCode.None;

    private KeyCode lastKeyPressed = KeyCode.None;
    private Rigidbody playerRB;
    private Animator animator;
    private PlayerPowerUp playerPowerUp;

    [Header("Audio Feedbacks (Feel)")]
    public MMF_Player keyPressFeedback;
    public MMF_Player wrongKeyPressFeedback;
    public MMF_Player jumpFeedback;
    public MMF_Player laneSwitchFeedback;
    public MMF_Player obstacleHitFeedback;

    #endregion

    public event Action<PlayerMovement, KeyCode, bool> OnKeyPressed;
    public event Action<int, int> OnLaneChargesChanged;

    private void Start()
    {
        playerRB = GetComponent<Rigidbody>();
        playerStamina = GetComponent<PlayerStamina>();
        animator = GetComponentInChildren<Animator>();
        playerPowerUp = GetComponent<PlayerPowerUp>();
        currentLaneCharges = maxLaneCharges;
        OnLaneChargesChanged?.Invoke(currentLaneCharges, maxLaneCharges);

        if (spline == null)
        {
            Debug.LogError("Spline not assigned to PlayerMovement on " + gameObject.name + ". Movement will not work.");
            enabled = false; // Disable script if spline is missing
            return;
        }
        else
        {
            cachedSplineLength = spline.GetLengthApproximately(0f, 1f, 0.1f);
            Debug.Log(gameObject.name + " - Estimated Spline Length: " + cachedSplineLength);
            if (cachedSplineLength <= 0)
            {
                Debug.LogError("Spline length is zero or negative for " + gameObject.name + ". Movement will not work correctly.");
                // Consider disabling script or handling this case
            }
        }

        if (groundCheck == null)
        {
            Debug.LogWarning("GroundCheck transform not assigned to PlayerMovement on " + gameObject.name + ". Ground detection will not work.");
        }
        if (playerStamina == null)
        {
            Debug.LogWarning("PlayerStamina component not found on " + gameObject.name + ". Stamina system will not work.");
        }
        if (animator == null)
        {
            Debug.LogWarning("Animator component not found on children or self for " + gameObject.name + ". Animations will not play.");
        }
        if (playerPowerUp == null)
        {
            Debug.LogWarning("PlayerMovement: PlayerPowerUp component not found on " + gameObject.name + ". Stun effect might not work correctly.");
        }

        targetLateralOffset = (currentLane - 1) * laneOffsetAmount;
        lateralOffset = targetLateralOffset;
        currentLaneCharges = maxLaneCharges;
    }

    private void Update()
    {
        HandleMovementInput(); // Renamed from HandleInput for clarity
        CheckGrounded();

        if (Input.GetKeyDown(jumpKey) && isGrounded) // Use the assignable jumpKey
        {
            Jump();
            jumpFeedback?.PlayFeedbacks(transform.position);
        }

        HandleLaneSwitchInput();
    }

    private void FixedUpdate()
    {
        ApplyDeceleration();
        ApplyMovementAlongSpline(); // Calculates normalizedT based on moveSpeed

        if (playerRB != null)
        {
            // Preserve Y velocity for gravity/jump, nullify XZ velocity
            // as LateUpdate will handle precise XZ positioning on the spline.
            playerRB.velocity = new Vector3(0f, playerRB.velocity.y, 0f);
        }
    }

    private void LateUpdate()
    {
        if (spline == null) // Should have been caught in Start, but good for safety
            return;

        lateralOffset = Mathf.Lerp(lateralOffset, targetLateralOffset, laneSwitchSpeed * Time.deltaTime);

        Vector3 pointOnSpline = spline.GetPoint(normalizedT);
        Vector3 tangent = spline.GetTangent(normalizedT); // No need to normalize here, GetTangent usually is. If not, normalize.

        Vector3 rightDirection;
        if (Mathf.Abs(Vector3.Dot(tangent.normalized, Vector3.up)) > 0.99f) // Checks if tangent is nearly vertical
        {
            // If tangent is vertical, need a robust way to get 'right'.
            // Using the player's current transform.right might be okay if player is mostly upright.
            // Or, if spline has a general 'forward' direction even when parts are vertical, use that.
            // For a typical race track, this might be rare unless you have perfectly vertical sections.
            rightDirection = transform.right; // Fallback for vertical tangents
        }
        else
        {
            rightDirection = Vector3.Cross(tangent, Vector3.up).normalized;
        }

        Vector3 finalPos = pointOnSpline + rightDirection * lateralOffset;
        finalPos.y = transform.position.y; // Preserve Rigidbody's Y (jump/gravity)

        transform.position = finalPos;

        if (tangent != Vector3.zero)
        {
            Vector3 lookDirection = tangent;
            lookDirection.y = 0;
            if (lookDirection.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
            }
        }
    }

    #region Jump
    private void CheckGrounded()
    {
        if (groundCheck == null) { isGrounded = false; return; } // Assume not grounded if no groundCheck
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
        //animator?.SetBool("isGrounded", isGrounded); no animation yet
    }

    private void Jump()
    {
        if (playerRB == null) return;
        playerRB.velocity = new Vector3(playerRB.velocity.x, 0f, playerRB.velocity.z); // Reset Y for consistent jump
        playerRB.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isGrounded = false;
        //animator?.SetTrigger("Jump"); no animation yet
    }
    #endregion

    #region Movement
    private void HandleMovementInput() // Renamed from HandleInput
    {
        if (playerPowerUp != null && playerPowerUp.IsPlayerStunned()) // Add IsPlayerStunned() to PlayerPowerUp
        {
            // If stunned, don't process movement input, maybe even decelerate quickly
            // moveSpeed = Mathf.Max(0, moveSpeed - someLargeDeceleration * Time.deltaTime);
            return;
        }

        // Check for key1 press
        if (Input.GetKeyDown(key1))
        {
            if (lastKeyPressed != key1)
            {
                IncreaseSpeed();
                lastKeyPressed = key1;
                OnKeyPressed?.Invoke(this, key1, true);

                keyPressFeedback?.PlayFeedbacks(transform.position);
            }
            else // Wrong: key1 pressed consecutively
            {
                OnKeyPressed?.Invoke(this, key1, false); // Incorrect press
                moveSpeed = Mathf.Max(moveSpeed - wrongPressPenalty, 0);

                wrongKeyPressFeedback?.PlayFeedbacks(transform.position);
            }
        }
        // Check for key2 press
        else if (Input.GetKeyDown(key2))
        {
            if (lastKeyPressed != key2)
            {
                IncreaseSpeed();
                lastKeyPressed = key2;
                OnKeyPressed?.Invoke(this, key2, true); // Correct press

                keyPressFeedback?.PlayFeedbacks(transform.position);
            }
            else // Wrong: key2 pressed consecutively
            {
                OnKeyPressed?.Invoke(this, key2, false); // Incorrect press
                moveSpeed = Mathf.Max(moveSpeed - wrongPressPenalty, 0);

                wrongKeyPressFeedback?.PlayFeedbacks(transform.position);
            }
        }
    }
    private void IncreaseSpeed()
    {
        if (playerStamina == null) return;

        moveSpeed += speedIncrease;

        if (!playerStamina.isExhausted)
        {
            playerStamina.ConsumeStamina(5f); // Make sure this value is balanced
        }

        float effectiveMaxSpeed = playerStamina.isExhausted ? maxSpeed * 0.5f : maxSpeed;
        moveSpeed = Mathf.Clamp(moveSpeed, 0, effectiveMaxSpeed);
    }
    private void ApplyDeceleration()
    {
        if (moveSpeed > 0)
        {
            moveSpeed -= deceleRate * Time.deltaTime;
            moveSpeed = Mathf.Max(moveSpeed, 0); // Prevent negative speed
        }
    }
    private void ApplyMovementAlongSpline()
    {
        if (spline == null || cachedSplineLength <= 0) // Check cachedSplineLength too
            return;

        if (moveSpeed <= 0.01f)
        {
            UpdateAnimationStates();
            return;
        }

        normalizedT += (moveSpeed / cachedSplineLength) * Time.fixedDeltaTime;
        normalizedT = Mathf.Clamp01(normalizedT); // Clamp since no laps

        UpdateAnimationStates();
    }

    private void UpdateAnimationStates()
    {
        if (animator == null) return;

        bool isCurrentlyWalking = moveSpeed > 0.1f && moveSpeed < (maxSpeed * 0.5f) && isGrounded;
        bool isCurrentlyRunning = moveSpeed >= (maxSpeed * 0.5f) && isGrounded;

        animator.SetBool("isWalking", isCurrentlyWalking);
        animator.SetBool("isRunning", isCurrentlyRunning);

        // You might want separate parameters for idle, jump, fall
        // e.g., animator.SetBool("isIdle", moveSpeed <= 0.1f && isGrounded);
    }
    #endregion

    #region Lane Switching
    private void HandleLaneSwitchInput()
    {
        bool switchedThisFrame = false; // To ensure sound plays only once per successful switch
        if (Input.GetKeyDown(rightSwitchKey) && currentLane > 0 && currentLaneCharges > 0 && !isSwitchingLanes)
        {
            currentLane--;
            currentLaneCharges--;
            targetLateralOffset = (currentLane - 1) * laneOffsetAmount;
            isSwitchingLanes = true;
            switchedThisFrame = true;
            OnLaneChargesChanged?.Invoke(currentLaneCharges, maxLaneCharges);
        }
        else if (Input.GetKeyDown(leftSwitchKey) && currentLane < 2 && currentLaneCharges > 0 && !isSwitchingLanes)
        {
            currentLane++;
            currentLaneCharges--;
            targetLateralOffset = (currentLane - 1) * laneOffsetAmount;
            isSwitchingLanes = true;
            switchedThisFrame = true;
            OnLaneChargesChanged?.Invoke(currentLaneCharges, maxLaneCharges);
        }

        if (switchedThisFrame)
        {
            laneSwitchFeedback?.PlayFeedbacks(transform.position); // Play lane switch sound
        }

        if (Mathf.Abs(lateralOffset - targetLateralOffset) < 0.1f)
        {
            isSwitchingLanes = false;
        }
    }

    public void RechargeLaneSwitch(int amount = 1)
    {
        int oldCharges = currentLaneCharges;
        currentLaneCharges = Mathf.Min(currentLaneCharges + amount, maxLaneCharges);
        if (currentLaneCharges != oldCharges || amount > 0)
        {
            OnLaneChargesChanged?.Invoke(currentLaneCharges, maxLaneCharges);
        }
        // Debug.Log(gameObject.name + " lane charges. Current: " + currentLaneCharges);
    }
    #endregion

    public void ApplyObstaclePenalty(float penaltyAmount = 5f) // Added parameter for flexibility
    {
        moveSpeed = Mathf.Max(moveSpeed - penaltyAmount, 0);
        //Debug.Log("Obstacle hit! Speed penalty applied.");
        obstacleHitFeedback?.PlayFeedbacks(transform.position);
        // Optionally, add a visual/audio feedback for penalty
    }

    public Vector3 GetTrapSpawnPoint(out Quaternion spawnRotation)
    {
        if (spline == null)
        {
            spawnRotation = transform.rotation;
            return transform.position - transform.forward * Mathf.Abs(trapDeployOffset); // Fallback
        }

        float distanceToMoveBack = Mathf.Abs(trapDeployOffset);
        float tOffset = distanceToMoveBack / cachedSplineLength; // Convert world distance to normalizedT offset
        float trapNormalizedT = normalizedT - tOffset;

        // Handle wrapping for looped splines if trapNormalizedT < 0
        if (spline.loop && trapNormalizedT < 0f)
        {
            trapNormalizedT += 1f;
        }
        trapNormalizedT = Mathf.Clamp01(trapNormalizedT); // Clamp if not looping or to be safe

        Vector3 pointOnSpline = spline.GetPoint(trapNormalizedT);
        Vector3 tangent = spline.GetTangent(trapNormalizedT);

        Vector3 rightDirection;
        if (Mathf.Abs(Vector3.Dot(tangent.normalized, Vector3.up)) > 0.99f)
        {
            rightDirection = transform.right;
        }
        else
        {
            rightDirection = Vector3.Cross(tangent, Vector3.up).normalized;
        }

        // Spawn in the player's current lane
        Vector3 trapPos = pointOnSpline + rightDirection * lateralOffset;

        // Ensure trap is on the ground - raycast down from a bit above trapPos
        RaycastHit hit;
        Vector3 groundPos = trapPos;
        if (Physics.Raycast(trapPos + Vector3.up * 1f, Vector3.down, out hit, 2f, groundLayer))
        {
            groundPos = hit.point;
        }
        else
        {
            groundPos.y = transform.position.y; // Fallback to player's Y if no ground found (less ideal)
        }


        // Rotation to align with spline
        Vector3 lookDirection = tangent;
        lookDirection.y = 0;
        if (lookDirection.sqrMagnitude > 0.001f)
        {
            spawnRotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
        }
        else
        {
            spawnRotation = transform.rotation; // Fallback
        }

        return groundPos;
    }

    // Gizmo for visualizing ground check
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
