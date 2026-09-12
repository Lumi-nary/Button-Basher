using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyRandomizer : MonoBehaviour
{
    [Tooltip("Reference to the PlayerMovement script.")]
    public PlayerMovement playerMovement;

    [Tooltip("Reference to the UI for the first key button.")]
    public KeyButtonUI button1UI;

    [Tooltip("Reference to the UI for the second key button.")]
    public KeyButtonUI button2UI;

    [Header("Game Modes Integration (Optional)")]
    [Tooltip("Reference to the Endurance script if this randomizer affects it.")]
    public Endurance endurance; // Assuming Endurance script has an OnKeysRandomized() method

    // List of allowed keys for randomization.
    [Header("Key Randomization Settings")]
    [Tooltip("Keys that can be chosen for button mashing. Ensure these don't conflict with other critical player inputs.")]
    public List<KeyCode> allowedKeys = new List<KeyCode>
    {
        KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F,
        KeyCode.J, KeyCode.K, KeyCode.L // Example keys, customize as needed
    };

    [Tooltip("Minimum number of correct presses before keys randomize.")]
    public int minPressesForRandomize = 9;
    [Tooltip("Maximum number of correct presses (exclusive) before keys randomize.")]
    public int maxPressesForRandomize = 31;


    private int keyPressCount = 0;
    private int pressThreshold;

    private void Start()
    {
        if (playerMovement == null)
        {
            Debug.LogError("PlayerMovement not assigned to KeyRandomizer on " + gameObject.name + ". KeyRandomizer will be disabled.");
            enabled = false;
            return;
        }
        if (button1UI == null || button2UI == null)
        {
            Debug.LogWarning("One or both KeyButtonUI references are not assigned to KeyRandomizer on " + gameObject.name + ". UI will not update correctly.");
        }
        if (allowedKeys.Count < 2)
        {
            Debug.LogError("AllowedKeys list in KeyRandomizer needs at least 2 keys to function. KeyRandomizer will be disabled. (" + gameObject.name + ")");
            enabled = false;
            return;
        }


        GenerateNewThreshold();
        RandomizeKeysAndRecharge(); // Initial randomization and recharge

        // Subscribe to the specific player's key press event
        playerMovement.OnKeyPressed += HandleKeyPress;
    }

    private void OnDestroy()
    {
        // Unsubscribe when this object is destroyed to prevent memory leaks or errors
        if (playerMovement != null)
        {
            playerMovement.OnKeyPressed -= HandleKeyPress;
        }
    }

    private void HandleKeyPress(PlayerMovement sourcePlayer, KeyCode key, bool isCorrect)
    {
        // This KeyRandomizer is only interested in events from its assigned playerMovement instance.
        // The subscription in Start() already ensures this, so sourcePlayer check is redundant here.

        if (isCorrect)
        {
            keyPressCount++;

            if (keyPressCount >= pressThreshold)
            {
                RandomizeKeysAndRecharge(); // Randomize and recharge
                keyPressCount = 0;          // Reset counter
                GenerateNewThreshold();     // Generate new threshold for next randomization
            }
        }
    }

    private void GenerateNewThreshold()
    {
        if (minPressesForRandomize >= maxPressesForRandomize)
        {
            Debug.LogWarning("minPressesForRandomize should be less than maxPressesForRandomize. Using default threshold of 15.");
            pressThreshold = 15;
            return;
        }
        pressThreshold = Random.Range(minPressesForRandomize, maxPressesForRandomize);
        // Debug.Log(gameObject.name + ": Next key randomization in " + pressThreshold + " correct presses.");
    }

    /// <summary>
    /// Randomizes the player's movement keys, updates their UI, and recharges one lane switch charge.
    /// </summary>
    private void RandomizeKeysAndRecharge()
    {
        if (allowedKeys.Count < 2)
        {
            Debug.LogWarning("Cannot randomize keys: Not enough allowed keys defined. (" + gameObject.name + ")");
            return;
        }

        int index1 = Random.Range(0, allowedKeys.Count);
        int index2;
        do
        {
            index2 = Random.Range(0, allowedKeys.Count);
        } while (index2 == index1 && allowedKeys.Count > 1); // Ensure different keys if more than one option

        KeyCode newKey1 = allowedKeys[index1];
        KeyCode newKey2 = allowedKeys[index2];

        playerMovement.key1 = newKey1;
        playerMovement.key2 = newKey2;

        // Update the UI buttons to display the new keys
        UpdateUIButton(button1UI, newKey1);
        UpdateUIButton(button2UI, newKey2);

        // Recharge one lane switch charge by calling the method in PlayerMovement
        // This method should internally handle invoking OnLaneChargesChanged.
        playerMovement.RechargeLaneSwitch(1);
        // Debug.Log($"{gameObject.name}: Keys randomized to {newKey1} & {newKey2}. Lane charge added.");


        // Notify Endurance mode if it's being used
        if (endurance != null)
        {
            endurance.OnKeysRandomized(); // Assuming this method exists and is needed by Endurance mode
        }
    }

    private void UpdateUIButton(KeyButtonUI buttonUI, KeyCode newKey)
    {
        if (buttonUI != null)
        {
            buttonUI.UpdateDisplayedKey(newKey);
        }
        else
        {
            // This warning might be spammy if one UI isn't set, consider if it's needed once Start checks it.
            // Debug.LogWarning("KeyRandomizer: A KeyButtonUI reference is null during UpdateUIButton. (" + gameObject.name + ")");
        }
    }
}
