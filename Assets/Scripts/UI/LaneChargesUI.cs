using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class LaneChargesUI : MonoBehaviour
{
    [Tooltip("The PlayerMovement script this UI is associated with.")]
    public PlayerMovement targetPlayerMovement;

    [Tooltip("The TextMeshProUGUI element to display the charges.")]
    public TextMeshProUGUI chargesText;

    void Awake()
    {
        if (chargesText == null)
        {
            chargesText = GetComponent<TextMeshProUGUI>();
            if (chargesText == null)
            {
                Debug.LogError("LaneChargesUI: ChargesText (TextMeshProUGUI) not found or assigned on " + gameObject.name);
                enabled = false;
                return;
            }
        }
    }

    void OnEnable()
    {
        if (targetPlayerMovement != null)
        {
            targetPlayerMovement.OnLaneChargesChanged += UpdateChargesDisplay;
            // Initial update when UI becomes active
            UpdateChargesDisplay(targetPlayerMovement.currentLaneCharges, targetPlayerMovement.maxLaneCharges);
        }
        else
        {
            Debug.LogError("LaneChargesUI: TargetPlayerMovement not assigned on " + gameObject.name);
            enabled = false;
        }
    }

    void OnDisable()
    {
        if (targetPlayerMovement != null)
        {
            targetPlayerMovement.OnLaneChargesChanged -= UpdateChargesDisplay;
        }
    }

    void UpdateChargesDisplay(int currentCharges, int maxCharges)
    {
        if (chargesText != null)
        {
            chargesText.text = $"SWITCH CHARGES: {currentCharges}/{maxCharges}";
        }
    }
}
