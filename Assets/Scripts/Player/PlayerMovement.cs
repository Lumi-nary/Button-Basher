using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerMovement : MonoBehaviour
{

    #region Variables
    // Speed
    public float moveSpeed = 0f;
    public float speedIncrease = 1f;
    public float maxSpeed = 15f;
    public float deceleRate = 2f;
    public float wrongPressPenalty = 3f;

    // Jump
    public float jumpForce = 5f;
    public bool isGrounded = true;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    // Lane Switching
    [Header("Switch Lane Key Bindings")]
    [Tooltip("Player 1 Default Q. Player 2 Default Y")]
    public KeyCode leftSwitchKey = KeyCode.None;
    [Tooltip("Player 1 Default t. Player 2 Default P")]
    public KeyCode rightSwithKey = KeyCode.None;
    [Header("")]
    public float laneDistance = 3f; // distance between lanes
    private int currentLane = 1; // 0 = Left, 1 = Center, 2 = Right
    private Vector3 targetPosition;

    public int maxLaneCharges = 3;
    public int currentLaneCharges = 3;
    public float laneSwitchSpeed = 10f;
    private bool isSwitchingLanes = false;


    public PlayerStamina playerStamina;

    [Header("Movement Key Bindings")]
    public KeyCode key1 = KeyCode.None;
    public KeyCode key2 = KeyCode.None;

    private KeyCode lastKeyPressed = KeyCode.None;
    private Rigidbody playerRB;

    #endregion

    public static event Action<KeyCode, bool> OnKeyPressed;

    private void Start()
    {
        playerRB = GetComponent<Rigidbody>();
        playerStamina = GetComponent<PlayerStamina>();
    }

    private void Update()
    {
        HandleInput();
        CheckGrounded();

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }

        HandleLaneSwitchInput();
    }

    private void FixedUpdate()
    {
        ApplyDeceleration();
        ApplyMovement();

        if (isSwitchingLanes)
            SmoothLaneMovement();
    }

    #region Jump
    private void CheckGrounded()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void Jump()
    {
        playerRB.velocity = new Vector3(playerRB.velocity.x, 0, playerRB.velocity.z); // reset Y
        playerRB.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
    #endregion
    #region Movement
    // soon to get randomized buttons from randomizer
    private void HandleInput()
    {
        // Check for key1 press
        if (Input.GetKeyDown(key1))
        {
            if (lastKeyPressed != key1)
            {
                IncreaseSpeed();
                lastKeyPressed = key1;
                OnKeyPressed?.Invoke(key1, true);
            }
            else // Wrong: key1 pressed consecutively
            {
                OnKeyPressed?.Invoke(key1, false);
                moveSpeed = Mathf.Max(moveSpeed - wrongPressPenalty, 0);
            }
        }
        // Check for key2 press
        else if (Input.GetKeyDown(key2))
        {
            if (lastKeyPressed != key2)
            {
                IncreaseSpeed();
                lastKeyPressed = key2;
                OnKeyPressed?.Invoke(key2, true);
            }
            else // Wrong: key2 pressed consecutively
            // i have no idea what the fuck AI did here i should fucking rewrite this whole code for fucksake
            {
                OnKeyPressed?.Invoke(key2, false);
                moveSpeed = Mathf.Max(moveSpeed - wrongPressPenalty, 0);
            }
        }
        // <summary>
        // what the fuck?
        // </summary>

        // Optionally, handle other keys if neede
    }
    private void IncreaseSpeed()
    {
        moveSpeed += speedIncrease;

        if (!playerStamina.isExhausted)
        {
            playerStamina.ConsumeStamina(5f);
        }

        // Use 50% maxSpeed when exhausted, full maxSpeed otherwise.
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
    private void ApplyMovement()
    {
        playerRB.velocity = new Vector3(0, playerRB.velocity.y, moveSpeed);
    }
    #endregion

    private void HandleLaneSwitchInput()
    {
        if (currentLaneCharges <= 0) return;

        if (Input.GetKeyDown(leftSwitchKey) && currentLane > 0)
        {
            currentLane--;
            currentLaneCharges--;
            isSwitchingLanes = true;
        }
        else if (Input.GetKeyDown(rightSwithKey) && currentLane < 2)
        {
            currentLane++;
            currentLaneCharges--;
            isSwitchingLanes = true;
        }

        targetPosition = new Vector3((currentLane - 1) * laneDistance, transform.position.y, transform.position.z);
    }

    private void SmoothLaneMovement()
    {
        Vector3 newPosition = Vector3.Lerp(transform.position, targetPosition, laneSwitchSpeed * Time.deltaTime);
        playerRB.MovePosition(newPosition);

        // Stop Lerp if close enough
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            isSwitchingLanes = false;
    }

    public void ApplyObstaclePenalty()
    {
        moveSpeed = Mathf.Max(moveSpeed - 5f, 0);
        //Debug.Log("Obstacle hit! Speed penalty applied.");
    }
}
