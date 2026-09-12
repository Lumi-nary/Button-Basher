using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

public class KeyButtonUI : MonoBehaviour
{
    [Header("Player Association")]
    [Tooltip("The PlayerMovement script this UI button is associated with.")]
    public PlayerMovement playerMovementInstance;

    [Header("Button Properties")]
    [Tooltip("The key this button currently represents. Will be updated by KeyRandomizer.")]
    public KeyCode assignedKey;

    [Tooltip("The UI Image component of this button.")]
    public Image buttonImage;

    [Tooltip("Default and pressed sprites.")]
    public Sprite defaultSprite;
    public Sprite pressedSprite;

    [Tooltip("Text that displays the key (child of this button).")]
    public TextMeshProUGUI keyTextDisplay;

    [Header("Colors")]
    [Tooltip("Default color of the button.")]
    public Color defaultColor = Color.white;
    [Tooltip("Color when the correct key is pressed.")]
    public Color correctColor = Color.green;
    [Tooltip("Color when the wrong key is pressed.")]
    public Color wrongColor = Color.red;

    [Header("Animation")]
    [Tooltip("Y position of the key text when button is pressed.")]
    public float textPressedYOffset = 0f;
    [Tooltip("Default Y position of the key text.")]
    public float textDefaultYOffset = 25f;

    private void Awake()
    {
        if (buttonImage == null)
            buttonImage = GetComponent<Image>();

        if (keyTextDisplay == null)
            keyTextDisplay = GetComponentInChildren<TextMeshProUGUI>(); // This will display the assignedKey

        if (buttonImage == null) Debug.LogError("KeyButtonUI: Button Image not found on " + gameObject.name);
        if (keyTextDisplay == null) Debug.LogWarning("KeyButtonUI: Key Text Display (TextMeshProUGUI child) not found on " + gameObject.name);
    }

    private void Start()
    {
        // Ensure playerMovementInstance is assigned
        if (playerMovementInstance == null)
        {
            Debug.LogError("KeyButtonUI: PlayerMovementInstance not assigned on " + gameObject.name + ". UI will not respond to key presses.");
            enabled = false; // Disable script if essential reference is missing
            return;
        }
        // Initial setup of text and sprite
        if (keyTextDisplay != null)
        {
            keyTextDisplay.text = InputDisplayUtility.GetDisplayString(assignedKey); // Call utility
        }
        if (buttonImage != null) buttonImage.sprite = defaultSprite;
        if (buttonImage != null) buttonImage.color = defaultColor;
        if (keyTextDisplay != null)
        {
            RectTransform textRect = keyTextDisplay.GetComponent<RectTransform>();
            if (textRect != null) textRect.anchoredPosition = new Vector2(textRect.anchoredPosition.x, textDefaultYOffset);
        }
    }

    private void OnEnable()
    {
        if (playerMovementInstance != null)
        {
            // Subscribe to the INSTANCE event
            playerMovementInstance.OnKeyPressed += HandleKeyPressed;
        }
        else if (Application.isPlaying) // Only log error if in play mode and it wasn't caught in Start
        {
            Debug.LogError("KeyButtonUI: PlayerMovementInstance is null on Enable. Cannot subscribe to OnKeyPressed. (" + gameObject.name + ")");
        }
    }

    private void OnDisable()
    {
        if (playerMovementInstance != null)
        {
            // Unsubscribe from the INSTANCE event
            playerMovementInstance.OnKeyPressed -= HandleKeyPressed;
        }
    }

    // Signature updated to match the instance event from PlayerMovement
    private void HandleKeyPressed(PlayerMovement sourcePlayer, KeyCode pressedKey, bool isCorrect)
    {
        // We are subscribed to a specific playerMovementInstance, so sourcePlayer should match playerMovementInstance.
        // No explicit check is strictly needed here if setup correctly, but doesn't hurt for sanity:
        // if (sourcePlayer != playerMovementInstance) return;

        if (pressedKey == assignedKey)
        {
            if (buttonImage == null || keyTextDisplay == null) return; // Safety check

            // Change sprite and color
            buttonImage.color = isCorrect ? correctColor : wrongColor;
            if (pressedSprite != null) buttonImage.sprite = pressedSprite; // Only change if pressedSprite is assigned

            RectTransform textRect = keyTextDisplay.GetComponent<RectTransform>();
            if (textRect != null)
                textRect.anchoredPosition = new Vector2(textRect.anchoredPosition.x, textPressedYOffset);

            // Stop any previous reset coroutine to handle rapid presses correctly
            StopAllCoroutines();
            StartCoroutine(ResetAfterDelay(0.5f));
        }
    }

    private IEnumerator ResetAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (buttonImage == null || keyTextDisplay == null) yield break; // Safety check

        // Reset color and sprite
        buttonImage.color = defaultColor;
        if (defaultSprite != null) buttonImage.sprite = defaultSprite;

        RectTransform textRect = keyTextDisplay.GetComponent<RectTransform>();
        if (textRect != null)
            textRect.anchoredPosition = new Vector2(textRect.anchoredPosition.x, textDefaultYOffset);
    }

    // This method will be called by KeyRandomizer to update the displayed key
    public void UpdateDisplayedKey(KeyCode newKey)
    {
        assignedKey = newKey;
        if (keyTextDisplay != null)
        {
            keyTextDisplay.text = InputDisplayUtility.GetDisplayString(newKey); // Call utility
        }
        // Reset visual state to default when key changes
        if (buttonImage != null)
        {
            buttonImage.color = defaultColor;
            if (defaultSprite != null) buttonImage.sprite = defaultSprite;
        }
        if (keyTextDisplay != null)
        {
            RectTransform textRect = keyTextDisplay.GetComponent<RectTransform>();
            if (textRect != null) textRect.anchoredPosition = new Vector2(textRect.anchoredPosition.x, textDefaultYOffset);
        }

    }

}
