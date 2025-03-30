using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class KeyRandomizer : MonoBehaviour
{
    [Tooltip("Reference to the PlayerMovement script.")]
    public PlayerMovement playerMovement;

    [Tooltip("Reference to the UI for the first key button.")]
    public KeyButtonUI button1UI;

    [Tooltip("Reference to the UI for the second key button.")]
    public KeyButtonUI button2UI;

    // List of allowed keys for randomization.
    public List<KeyCode> allowedKeys = new List<KeyCode>
    {
        KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F,
        KeyCode.J, KeyCode.K, KeyCode.L
    };

    private void Start()
    {
        // Randomize the keys at the start.
        RandomizeKeys();
    }

    private void Update()
    {
        // Debug: Press 'R' to randomize keys.
        if (Input.GetKeyDown(KeyCode.R))
        {
            RandomizeKeys();
        }
    }

    private void RandomizeKeys()
    {
        if (allowedKeys.Count < 2)
        {
            Debug.LogWarning("Not enough allowed keys to randomize.");
            return;
        }

        // Randomly select two different keys.
        int index1 = Random.Range(0, allowedKeys.Count);
        int index2 = Random.Range(0, allowedKeys.Count);
        while (index2 == index1)
        {
            index2 = Random.Range(0, allowedKeys.Count);
        }
        KeyCode newKey1 = allowedKeys[index1];
        KeyCode newKey2 = allowedKeys[index2];

        // Update the PlayerMovement script with the new keys.
        playerMovement.key1 = newKey1;
        playerMovement.key2 = newKey2;
        Debug.Log("Randomized Keys: " + newKey1 + " and " + newKey2);

        // Update the UI for both key buttons.
        UpdateUIButton(button1UI, newKey1);
        UpdateUIButton(button2UI, newKey2);
    }

    void UpdateUIButton(KeyButtonUI buttonUI, KeyCode newKey)
    {
        // Update the assigned key in the UI script.
        buttonUI.assignedKey = newKey;

        // Update the TextMeshProUGUI child component to show the new key.
        TextMeshProUGUI tmp = buttonUI.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = newKey.ToString();
        }
        else
        {
            Debug.LogWarning("KeyButtonUI: No TextMeshProUGUI component found in children to update key text.");
        }
    }
}
