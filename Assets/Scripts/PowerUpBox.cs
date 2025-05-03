using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpBox : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerPowerUp playerPowerUp = other.GetComponent<PlayerPowerUp>();
        if (playerPowerUp != null && !playerPowerUp.HasPowerUp())
        {
            PowerUpTypes randomType = GetRandomPowerUp();
            playerPowerUp.PickUp(randomType);
            Destroy(gameObject);
        }
    }

    private PowerUpTypes GetRandomPowerUp()
    {
        int totalTypes = System.Enum.GetValues(typeof(PowerUpTypes)).Length;
        return (PowerUpTypes)Random.Range(0, totalTypes);
    }
}
