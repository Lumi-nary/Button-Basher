using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

public class KeyButtonUI : MonoBehaviour
{
    [Tooltip("The key this button represents.")]
    public KeyCode assignedKey;

    [Tooltip("The UI Image component of this button.")]
    public Image buttonImage;

    [Tooltip("Default color of the button.")]
    public Color defaultColor = Color.white;

    [Tooltip("Color when the correct key is pressed.")]
    public Color correctColor = Color.green;

    [Tooltip("Color when the wrong key is pressed.")]
    public Color wrongColor = Color.red;

    private void Awake()
    {
        if (buttonImage == null)
            buttonImage = GetComponent<Image>();

        // DEBUG: Parse the key from the TextMeshProUGUI component.
        //TextMeshProUGUI keyText = GetComponentInChildren<TextMeshProUGUI>();
        //if (keyText != null)
        //{
        //    string keyStr = keyText.text.Trim();
        //    if (Enum.TryParse(keyStr, true, out KeyCode parsedKey))
        //    {
        //        assignedKey = parsedKey;
        //    }
        //    else
        //    {
        //        Debug.LogWarning("KeyButtonUI: Unable to parse the key from text: " + keyStr);
        //    }
        //}
        //else
        //{
        //    Debug.LogWarning("KeyButtonUI: No TextMeshProUGUI component found in children to assign key.");
        //}
    }

    private void OnEnable()
    {
        PlayerMovement.OnKeyPressed += HandleKeyPressed;
    }
    private void OnDisable()
    {
        PlayerMovement.OnKeyPressed -= HandleKeyPressed;
    }

    private void HandleKeyPressed(KeyCode pressedKey, bool isCorrect)
    {
        if (pressedKey == assignedKey)
        {
            buttonImage.color = isCorrect ? correctColor : wrongColor;
            StartCoroutine(ResetColorAfterDelay(0.5f));
        }
    }
    private IEnumerator ResetColorAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        buttonImage.color = defaultColor;
    }
}
