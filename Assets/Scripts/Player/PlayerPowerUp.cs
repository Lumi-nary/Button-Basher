using UnityEngine;

public class PlayerPowerUp : MonoBehaviour
{
    private PowerUpTypes? heldPowerUp = null;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) // temporary key for use
        {
            UsePowerUp();
        }
    }


    public void PickUp(PowerUpTypes type)
    {
        heldPowerUp = type;
        Debug.Log("Picked up power-up: " + type);
        // You can show UI, play a sound, etc. here.
    }

    public bool HasPowerUp()
    {
        return heldPowerUp.HasValue;
    }

    public void UsePowerUp()
    {
        if (!heldPowerUp.HasValue) return;

        switch (heldPowerUp.Value)
        {
            case PowerUpTypes.SpeedBoost:
                GetComponent<PlayerMovement>().moveSpeed += 5f;
                break;
            case PowerUpTypes.IncreaseStamina:
                GetComponent<PlayerStamina>().currentStamina += 20f;
                break;
            case PowerUpTypes.DecreaseOpponentStamina:
                // Later: Add multiplayer opponent targeting
                break;
            case PowerUpTypes.SlowOpponent:
                // Later: Add multiplayer opponent targeting
                break;
            case PowerUpTypes.StunOpponent:
                // Later: Add multiplayer opponent targeting
                break;
            case PowerUpTypes.Trap:
                // Later: Deploy trap prefab
                break;
            case PowerUpTypes.Shield:
            // Later: Add shield visual + block next hit
                break;
        }

        Debug.Log("Used power-up: " + heldPowerUp);
        heldPowerUp = null;
    }
}
