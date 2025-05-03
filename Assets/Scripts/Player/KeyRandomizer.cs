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

    [Header("Game Modes")]
    public Endurance endurance;

    // List of allowed keys for randomization.
    public List<KeyCode> allowedKeys = new List<KeyCode>
    {
        KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F,
        KeyCode.J, KeyCode.K, KeyCode.L
    };

    private int keyPressCount = 0;
    private int pressThreshold;

    private void Start()
    {
        GenerateNewThreshold();
        RandomizeKeys();

        // Subscribe to key press event
        PlayerMovement.OnKeyPressed += HandleKeyPress;
    }

    private void OnDestroy()
    {
        PlayerMovement.OnKeyPressed -= HandleKeyPress;
    }

    private void Update()
    {
        // Debug: Press 'R' to randomize keys.
        //if (Input.GetKeyDown(KeyCode.R))
        //{
        //    RandomizeKeys();
        //}
    }

    private void HandleKeyPress(KeyCode key, bool isCorrect)
    {
        if (isCorrect)
        {
            keyPressCount++;

            if (keyPressCount >= pressThreshold)
            {
                RandomizeKeys();
                keyPressCount = 0;
                GenerateNewThreshold();
            }
        }
    }

    private void GenerateNewThreshold()
    {
        pressThreshold = Random.Range(5, 21); // Inclusive lower, exclusive upper
        //Debug.Log("Next key randomization in: " + pressThreshold + " presses.");
    }

    private void RandomizeKeys()
    {
        if (allowedKeys.Count < 2) return;

        int index1 = Random.Range(0, allowedKeys.Count);
        int index2;
        do { index2 = Random.Range(0, allowedKeys.Count); }
        while (index2 == index1);

        KeyCode newKey1 = allowedKeys[index1];
        KeyCode newKey2 = allowedKeys[index2];

        playerMovement.key1 = newKey1;
        playerMovement.key2 = newKey2;

        UpdateUIButton(button1UI, newKey1);
        UpdateUIButton(button2UI, newKey2);

        playerMovement.currentLaneCharges = playerMovement.maxLaneCharges;
        //Debug.Log($"Keys randomized: {newKey1} and {newKey2}");

        if (endurance != null)
        {
            endurance.OnKeysRandomized();
        }
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
