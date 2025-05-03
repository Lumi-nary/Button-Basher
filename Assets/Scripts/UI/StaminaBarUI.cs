using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaminaBarUI : MonoBehaviour
{
    [Tooltip("UI Image that represents the fill of the stamina bar.")]
    public Image fillImage;

    [Tooltip("Reference to the PlayerStamina script for this UI.")]
    public PlayerStamina playerStamina;

    private void OnEnable()
    {
        if (playerStamina != null)
            playerStamina.OnStaminaChanged += UpdateStaminaUI;
    }

    private void OnDisable()
    {
        if (playerStamina != null)
            playerStamina.OnStaminaChanged -= UpdateStaminaUI;
    }

    /// <summary>
    /// Updates the UI fill image based on normalized stamina value.
    /// </summary>
    /// <param name="normalizedStamina">Value between 0 and 1.</param>
    private void UpdateStaminaUI(float normalizedStamina)
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = normalizedStamina;
        }
    }
}
