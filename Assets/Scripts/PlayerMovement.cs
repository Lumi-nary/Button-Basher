using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 0f;
    public float speedIncrease = 1f;
    public float maxSpeed = 10f;
    public float deceleRate = 2f;
    public float wrongPressPenalty = 3f;

    public KeyCode key1 = KeyCode.None;
    public KeyCode key2 = KeyCode.None;

    private KeyCode lastKeyPressed = KeyCode.None;
    private Rigidbody playerRB;

    public static event Action<KeyCode, bool> OnKeyPressed;

    private void Start()
    {
        playerRB = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        HandleInput();
    }

    private void FixedUpdate()
    {
        ApplyDeceleration();
        ApplyMovement();
    }

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
            {
                OnKeyPressed?.Invoke(key2, false);
                moveSpeed = Mathf.Max(moveSpeed - wrongPressPenalty, 0);
            }
        }
        // Optionally, handle other keys if needed
    }
    private void IncreaseSpeed()
    {
        moveSpeed += speedIncrease;
        moveSpeed = Mathf.Clamp(moveSpeed, 0, maxSpeed); // Ensure maxSpeed is applied
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
}
