using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class SpeedometerUI : MonoBehaviour
{
    [Tooltip("The PlayerMovement script this UI is associated with.")]
    public PlayerMovement targetPlayerMovement;

    [Tooltip("The TextMeshProUGUI element to display the speed.")]
    public TextMeshProUGUI speedText;

    void Awake()
    {
        if (speedText == null)
        {
            speedText = GetComponent<TextMeshProUGUI>();
            if (speedText == null)
            {
                Debug.LogError("SpeedometerUI: SpeedText (TextMeshProUGUI) not found or assigned on " + gameObject.name);
                enabled = false;
                return;
            }
        }
    }

    void Start() // Changed from OnEnable to Start to ensure targetPlayerMovement is more likely set up
    {
        if (targetPlayerMovement == null)
        {
            Debug.LogError("SpeedometerUI: TargetPlayerMovement not assigned on " + gameObject.name);
            enabled = false;
        }
    }

    void Update()
    {
        if (targetPlayerMovement != null && speedText != null)
        {
            speedText.text = $"SPEED: {Mathf.RoundToInt(targetPlayerMovement.moveSpeed)}";
        }
    }
}
