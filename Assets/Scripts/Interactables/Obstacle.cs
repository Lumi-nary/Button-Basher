using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public float speedPenaltyAmount = 5f; // Can be configured per obstacle type

    private void OnTriggerEnter(Collider other) // Or OnCollisionEnter if not a trigger
    {
        // Check if the collided object is a player (e.g., by tag or component)
        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player != null)
        {
            Debug.Log("Obstacle collided with player: " + other.name);
            player.ApplyObstaclePenalty(speedPenaltyAmount);

            // Optional: Destroy the obstacle or disable it after a hit
            Destroy(gameObject);
            // Or if it's a reusable obstacle that just penalizes:
            // gameObject.SetActive(false);
            // StartCoroutine(ReactivateAfterDelay(5f)); // Example to reactivate
        }

        // If you use PlayerPowerUp to handle hits (for shields etc.)
        PlayerPowerUp playerPowerUp = other.GetComponent<PlayerPowerUp>();
        if (playerPowerUp != null)
        {
             playerPowerUp.TakeHit(); // This would then internally call ApplyObstaclePenalty if not shielded
        }
    }
}
