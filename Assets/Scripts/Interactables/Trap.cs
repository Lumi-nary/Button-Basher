using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour
{
    public float speedPenalty = 7f;
    private void OnTriggerEnter(Collider other)
    {
        // Check if it's a player and NOT the player who deployed it (if you add owner ID to trap)
        PlayerMovement opponentMovement = other.GetComponent<PlayerMovement>();
        PlayerPowerUp opponentPowerUp = other.GetComponent<PlayerPowerUp>();

        if (opponentMovement != null && opponentPowerUp != null)
        {
            Debug.Log("Trap triggered by: " + other.gameObject.name);
            opponentPowerUp.TakeHit(); // This will check for shield first

            opponentMovement.ApplyObstaclePenalty(speedPenalty);

            // if (destroyOnHit)
            // {
            Destroy(gameObject); // Destroy trap after it's triggered
            // }
        }
    }
}
