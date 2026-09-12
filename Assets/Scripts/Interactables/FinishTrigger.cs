using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishTrigger : MonoBehaviour
{
    private bool raceOver = false;

    private void OnTriggerEnter(Collider other)
    {
        if (raceOver) return;

        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player != null)
        {
            raceOver = true;
            Debug.Log("Winner: " + player.name);
            // You can show UI here, stop movement, etc.
        }
    }
}
