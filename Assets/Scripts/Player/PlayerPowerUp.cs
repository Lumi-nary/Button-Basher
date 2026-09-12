using MoreMountains.Feedbacks;
using System.Collections;
using UnityEngine;

public class PlayerPowerUp : MonoBehaviour
{
#if UNITY_EDITOR // Only include this method in the editor for debugging
    // Call this from your custom editor button
    public void Debug_ClearHeldPowerUp()
    {
        if (heldPowerUp.HasValue)
        {
            Debug.Log(gameObject.name + " - Debug: Cleared held power-up: " + heldPowerUp.Value);
            heldPowerUp = null;
            OnHeldPowerUpChanged?.Invoke(heldPowerUp); // Important to update UI
        }
    }

    // Optional: A method to force give a powerup, bypassing the "already holding" check for debug
    public void Debug_ForceGivePowerUp(PowerUpTypes type)
    {
        heldPowerUp = type; // Overwrite current
        Debug.Log(gameObject.name + " - Debug: Force gave power-up: " + type);
        OnHeldPowerUpChanged?.Invoke(heldPowerUp);
    }
#endif

    private PowerUpTypes? heldPowerUp = null;
    private PlayerMovement playerMovement;
    private PlayerStamina playerStamina;

    [Header("Player Identification")]
    public int playerID = 0; // 0 for P1, 1 for P2. Assign in Inspector.

    [Header("Opponent (Assign in Inspector or find dynamically)")]
    public PlayerMovement opponentMovement; // For affecting opponent's speed
    public PlayerStamina opponentStamina;   // For affecting opponent's stamina
    public PlayerPowerUp opponentPowerUpScript; // For stunning opponent

    // Key for using power-up (can be made public to assign per player)
    public KeyCode usePowerUpKey = KeyCode.None;

    [Header("Trap prefab")]
    public GameObject trapPrefab;

    [Header("Shield Visuals")]
    [Tooltip("Assign the GameObject that represents the shield visual (child of this player).")]
    public GameObject shieldVisualObject; // Assign your shield GameObject here

    [Header("Audio/Visual Feedbacks")]
    public MMF_Player shieldActivateFeedback;
    public MMF_Player shieldDeactivateFeedback;
    public MMF_Player shieldBlockFeedback;
    public MMF_Player speedBoostActivateFeedback;
    public MMF_Player increaseStaminaFeedback;
    public MMF_Player decreaseOpponentStaminaFeedback;
    public MMF_Player slowOpponentFeedback;
    public MMF_Player stunOpponentFeedback;
    public MMF_Player trapFeedback;


    // Power-up effect durations
    private const float SPEED_BOOST_DURATION = 5f;
    private const float SLOW_DURATION = 3f;
    private const float STUN_DURATION = 2f;
    private const float SHIELD_DURATION = 7f;
    private const float INCREASE_STAMINA_MAX_DURATION = 3f;
    private const float DECREASE_OPPONENT_MAX_STAMINA_DURATION = 3f;

    // Status flags
    private bool isShielded = false;
    private bool isStunned = false; // For when this player gets stunned

    public event System.Action<PowerUpTypes?> OnHeldPowerUpChanged;

    public PowerUpTypes? GetCurrentHeldPowerUp()
    {
        return heldPowerUp;
    }

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerStamina = GetComponent<PlayerStamina>();

        if (playerMovement == null) Debug.LogError("PlayerMovement not found on " + gameObject.name);
        if (playerStamina == null) Debug.LogError("PlayerStamina not found on " + gameObject.name);

        if (shieldVisualObject != null)
        {
            shieldVisualObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("ShieldVisualObject not assigned to PlayerPowerUp on " + gameObject.name + ". Shield visual will not work.");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(usePowerUpKey) && !isStunned) // Can't use power-ups if stunned
        {
            UsePowerUp();
        }
    }


    public void PickUp(PowerUpTypes type)
    {
        if (heldPowerUp.HasValue) return;

        heldPowerUp = type;
        Debug.Log(gameObject.name + " picked up power-up: " + type);
        OnHeldPowerUpChanged?.Invoke(heldPowerUp); // Invoke event
                                                   // TODO: Update UI to show held power-up (handled by subscriber now)
    }

    public bool HasPowerUp()
    {
        return heldPowerUp.HasValue;
    }

    public void UsePowerUp()
    {
        if (!heldPowerUp.HasValue || isStunned) return;

        Debug.Log(gameObject.name + " used power-up: " + heldPowerUp.Value);

        switch (heldPowerUp.Value)
        {
            case PowerUpTypes.SpeedBoost:
                StartCoroutine(ActivateSpeedBoostCoroutine());
                break;
            case PowerUpTypes.IncreaseStamina:
                StartCoroutine(ActivateIncreaseStaminaCoroutine());
                break;
            case PowerUpTypes.DecreaseOpponentStamina:
                ActivateDecreaseOpponentStamina();
                break;
            case PowerUpTypes.SlowOpponent:
                ActivateSlowOpponent();
                break;
            case PowerUpTypes.StunOpponent:
                ActivateStunOpponent();
                break;
            case PowerUpTypes.Trap:
                DeployTrap();
                break;
            case PowerUpTypes.Shield:
                if (!isShielded)
                {
                    StartCoroutine(ActivateShieldCoroutine());
                }
                break;
        }
        heldPowerUp = null;
        OnHeldPowerUpChanged?.Invoke(heldPowerUp);
    }

    // --- Individual Power-Up Coroutines and Methods ---
    #region Power-ups Coroutines & Methods
    private IEnumerator ActivateSpeedBoostCoroutine() // Renamed
    {
        if (playerMovement == null) yield break;

        // --- Store original values ---
        float originalMaxSpeed = playerMovement.maxSpeed;

        // --- Apply effects ---
        playerMovement.maxSpeed = 30f; // Set to specific max speed
        Debug.Log(gameObject.name + " Speed Boost activated! Max speed: " + playerMovement.maxSpeed);

        // Play Feel Feedbacks for activation (sound, camera FOV change, particles)
        speedBoostActivateFeedback?.PlayFeedbacks(transform.position);

        // --- Wait for duration ---
        yield return new WaitForSeconds(SPEED_BOOST_DURATION);

        // --- Revert effects ---
        // Check if still this player and not some other state has changed drastically
        if (playerMovement != null)
        {
            playerMovement.maxSpeed = originalMaxSpeed;
            playerMovement.moveSpeed = Mathf.Min(playerMovement.moveSpeed, playerMovement.maxSpeed); // Clamp current speed
            Debug.Log(gameObject.name + " Speed Boost ended. Max speed restored to: " + playerMovement.maxSpeed);
        }
    }

    public IEnumerator ApplySlowEffect() // Called on the OPPONENT's script
    {
        if (isShielded) { Debug.Log(gameObject.name + " slow blocked by shield."); yield break; }
        if (playerMovement == null) yield break; // playerMovement here is the opponent's own PlayerMovement

        float originalMaxSpeed = playerMovement.maxSpeed;

        // Modify this to set to a specific speed rather than a factor if that's the design
        // playerMovement.maxSpeed *= slowFactor;
        float actualTargetSpeed = 10f; // The desired slowed max speed
        playerMovement.maxSpeed = actualTargetSpeed;

        playerMovement.moveSpeed = Mathf.Min(playerMovement.moveSpeed, playerMovement.maxSpeed); // Cap current speed

        // TODO: OPPONENT'S visual/audio feedback for being slowed (e.g., play an MMF_Player on this opponent)
        // For example: opponentSlowedFeedback?.PlayFeedbacks(transform.position);

        yield return new WaitForSeconds(SLOW_DURATION); // SLOW_DURATION is already a const in your script (3f)

        playerMovement.maxSpeed = originalMaxSpeed;
        // TODO: OPPONENT'S visual/audio feedback for slow ending

        Debug.Log(gameObject.name + " slow effect ended.");
    }

    public IEnumerator ApplyStunEffect()
    {
        if (isShielded) // Check if THIS player (the one being targeted) is shielded
        {
            Debug.Log(gameObject.name + " stun blocked by shield.");
            shieldBlockFeedback?.PlayFeedbacks(transform.position); // Play shield block on the target
            yield break;
        }
        if (playerMovement == null) yield break; // playerMovement is this (targeted) player's movement

        isStunned = true;
        playerMovement.moveSpeed = 0f; // Stop movement


        Debug.Log(gameObject.name + " is STUNNED!");

        yield return new WaitForSeconds(STUN_DURATION); // STUN_DURATION is already 2f

        isStunned = false;

        Debug.Log(gameObject.name + " stun ended.");
    }

    public bool IsPlayerStunned()
    {
        return isStunned;
    }

    private void DeployTrap()
    {
        if (playerMovement == null || trapPrefab == null)
        {
            Debug.LogWarning("Cannot deploy trap: PlayerMovement or TrapPrefab not set.");
            return;
        }

        Quaternion trapRotation;
        Vector3 trapPosition = playerMovement.GetTrapSpawnPoint(out trapRotation);

        GameObject trapInstance = Instantiate(trapPrefab, trapPosition, trapRotation);
        // The trapInstance itself will need a script to detect collision with the opponent
        // and call opponentPowerUpScript.TakeHit() or opponentMovement.ApplyObstaclePenalty()
        trapFeedback?.PlayFeedbacks(trapPosition);
        //Debug.Log(gameObject.name + " deployed a trap at " + trapPosition);
    }

    private IEnumerator ActivateShieldCoroutine()
    {
        isShielded = true;
        if (shieldVisualObject != null)
        {
            shieldVisualObject.SetActive(true);
            // You could also trigger a "shield up" animation on the shieldVisualObject here
        }

        shieldActivateFeedback?.PlayFeedbacks(transform.position); // Play activation sound/visuals
        Debug.Log(gameObject.name + " Shield Activated!");

        yield return new WaitForSeconds(SHIELD_DURATION);

        // This part only runs if the shield expires naturally
        if (isShielded) // Check if it wasn't broken early
        {
            DeactivateShield(false); // false because it expired, not broken by hit
        }
    }

    private void DeactivateShield(bool wasBrokenByHit)
    {
        isShielded = false;
        if (shieldVisualObject != null)
        {
            shieldVisualObject.SetActive(false);
            // You could trigger a "shield down" animation here
        }

        if (wasBrokenByHit)
        {
            // shieldBlockFeedback already played, or play a specific "shield break" sound
            shieldDeactivateFeedback?.PlayFeedbacks(transform.position); // Could be same as regular deactivate or a different one
            Debug.Log(gameObject.name + " Shield Broken and Deactivated.");
        }
        else
        {
            shieldDeactivateFeedback?.PlayFeedbacks(transform.position);
            Debug.Log(gameObject.name + " Shield Expired and Deactivated.");
        }
    }

    public void TakeHit()
    {
        if (isShielded)
        {
            Debug.Log(gameObject.name + " hit blocked by shield!");
            shieldBlockFeedback?.PlayFeedbacks(transform.position); // Play block sound

            // Shield breaks on one hit
            StopCoroutine(nameof(ActivateShieldCoroutine)); // Stop the expiration timer
            DeactivateShield(true); // true because it was broken by a hit
            return;
        }

        playerMovement?.ApplyObstaclePenalty();
    }

    private IEnumerator ActivateIncreaseStaminaCoroutine()
    {
        if (playerStamina == null) yield break;

        Debug.Log(gameObject.name + " used Increase Stamina!");
        increaseStaminaFeedback?.PlayFeedbacks(transform.position); // Play sound/visual

        // Call the method on PlayerStamina to handle the temporary max boost and refill
        playerStamina.ApplyTemporaryMaxStaminaBoost(300f, INCREASE_STAMINA_MAX_DURATION);

        yield return null;
    }

    private void ActivateDecreaseOpponentStamina()
    {
        if (opponentStamina == null)
        {
            Debug.LogWarning(gameObject.name + ": OpponentStamina reference not set. Cannot decrease opponent stamina.");
            return;
        }

        Debug.Log(gameObject.name + " used Decrease Opponent Stamina on " + opponentStamina.gameObject.name);
        decreaseOpponentStaminaFeedback?.PlayFeedbacks(transform.position); // Play sound for the user

        // Apply the debuff to the opponent's stamina
        opponentStamina.ApplyTemporaryMaxStaminaModifier(50f, DECREASE_OPPONENT_MAX_STAMINA_DURATION, true, false); // setCurrentToZero = true
    }
    private void ActivateSlowOpponent()
    {
        if (opponentPowerUpScript == null) // We need the opponent's PlayerPowerUp script to call ApplySlowEffect on
        {
            Debug.LogWarning(gameObject.name + ": OpponentPowerUpScript reference not set. Cannot slow opponent.");
            return;
        }

        Debug.Log(gameObject.name + " used Slow Opponent on " + opponentPowerUpScript.gameObject.name);
        slowOpponentFeedback?.PlayFeedbacks(transform.position); // Play sound for the user of the power-up

        // Call the ApplySlowEffect coroutine ON the opponent's PlayerPowerUp script
        opponentPowerUpScript.StartCoroutine(opponentPowerUpScript.ApplySlowEffect());
    }
    private void ActivateStunOpponent()
    {
        // We need the opponent's PlayerPowerUp script to call ApplyStunEffect on
        if (opponentPowerUpScript == null)
        {
            Debug.LogWarning(gameObject.name + ": OpponentPowerUpScript reference not set. Cannot stun opponent.");
            return;
        }

        Debug.Log(gameObject.name + " used Stun Opponent on " + opponentPowerUpScript.gameObject.name);
        stunOpponentFeedback?.PlayFeedbacks(transform.position); // Play sound for the user of the power-up

        // Call the ApplyStunEffect coroutine ON the opponent's PlayerPowerUp script
        opponentPowerUpScript.StartCoroutine(opponentPowerUpScript.ApplyStunEffect());
    }

    #endregion
}
