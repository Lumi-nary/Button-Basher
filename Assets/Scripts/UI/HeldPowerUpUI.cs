using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeldPowerUpUI : MonoBehaviour
{
    [Header("Player Association")]
    [Tooltip("The PlayerPowerUp script this UI is associated with.")]
    public PlayerPowerUp targetPlayerPowerUp;

    [Header("UI References")]
    [Tooltip("The UI Image element (named 'Content' in your hierarchy) that will display the power-up icon.")]
    public Image powerUpIconDisplayImage; // Assign your 'Content' Image GameObject here

    [Tooltip("Sprite to display when no power-up is held (e.g., your UIMask sprite). Assign this in the Inspector.")]
    public Sprite noPowerUpSprite;

    // Struct to map PowerUpTypes to Sprites in the Inspector
    [System.Serializable]
    public struct PowerUpIcon
    {
        public PowerUpTypes powerUpType;
        public Sprite iconSprite;
    }
    [Tooltip("Define icons for each power-up type here.")]
    public List<PowerUpIcon> powerUpIconsList;

    private Dictionary<PowerUpTypes, Sprite> iconLookup;

    void Awake()
    {
        // Attempt to get Image component if not assigned, but prioritize Inspector assignment
        if (powerUpIconDisplayImage == null)
        {
            // If this script is on the 'Content' GameObject itself:
            // powerUpIconDisplayImage = GetComponent<Image>();

            // If this script is on 'PowerUp' (root) and 'Content' is a child:
            Transform contentTransform = transform.Find("Content"); // Case-sensitive find
            if (contentTransform != null)
            {
                powerUpIconDisplayImage = contentTransform.GetComponent<Image>();
            }

            if (powerUpIconDisplayImage == null)
            {
                Debug.LogError("HeldPowerUpUI: PowerUpIconDisplayImage could not be found or assigned on " + gameObject.name +
                               ". Please assign the 'Content' Image component in the Inspector.");
                enabled = false;
                return;
            }
        }

        // Populate the dictionary from the inspector list for efficient lookup
        iconLookup = new Dictionary<PowerUpTypes, Sprite>();
        foreach (var PUIcon in powerUpIconsList)
        {
            if (PUIcon.iconSprite == null)
            {
                Debug.LogWarning($"PowerUpType '{PUIcon.powerUpType}' in iconMappings on {gameObject.name} has a null sprite assigned.");
                continue;
            }
            if (!iconLookup.ContainsKey(PUIcon.powerUpType))
            {
                iconLookup.Add(PUIcon.powerUpType, PUIcon.iconSprite);
            }
            else
            {
                Debug.LogWarning($"Duplicate PowerUpType '{PUIcon.powerUpType}' in iconMappings for {gameObject.name}");
            }
        }

        // Set initial state to no power-up sprite
        SetNoPowerUpDisplay();
    }

    void OnEnable()
    {
        if (targetPlayerPowerUp != null)
        {
            targetPlayerPowerUp.OnHeldPowerUpChanged += UpdateIconDisplay;
            // Initial update based on current state when UI becomes active
            UpdateIconDisplay(targetPlayerPowerUp.GetCurrentHeldPowerUp());
        }
        else
        {
            Debug.LogError("HeldPowerUpUI: TargetPlayerPowerUp not assigned on " + gameObject.name + ". UI will not function.");
            enabled = false; // Disable if critical reference is missing
        }
    }

    void OnDisable()
    {
        if (targetPlayerPowerUp != null)
        {
            targetPlayerPowerUp.OnHeldPowerUpChanged -= UpdateIconDisplay;
        }
    }

    void SetNoPowerUpDisplay()
    {
        if (powerUpIconDisplayImage != null)
        {
            if (noPowerUpSprite != null)
            {
                powerUpIconDisplayImage.sprite = noPowerUpSprite;
                powerUpIconDisplayImage.enabled = true; // Ensure it's visible
            }
            else
            {
                // If no "noPowerUpSprite" is assigned, just disable the image
                powerUpIconDisplayImage.enabled = false;
                // Debug.LogWarning("HeldPowerUpUI: NoPowerUpSprite is not assigned. Hiding icon image.");
            }
        }
    }

    void UpdateIconDisplay(PowerUpTypes? heldPowerUpType)
    {
        if (powerUpIconDisplayImage == null) return; // Should not happen if Awake check passed

        if (heldPowerUpType.HasValue)
        {
            if (iconLookup.TryGetValue(heldPowerUpType.Value, out Sprite iconToShow))
            {
                powerUpIconDisplayImage.sprite = iconToShow;
                powerUpIconDisplayImage.enabled = true; // Make sure image is visible
            }
            else
            {
                Debug.LogWarning($"HeldPowerUpUI: No icon defined for power-up type: {heldPowerUpType.Value} on {gameObject.name}. Displaying 'no power-up' state.");
                SetNoPowerUpDisplay(); // Fallback to no power-up display if specific icon is missing
            }
        }
        else
        {
            // No power-up held, show the default/mask sprite
            SetNoPowerUpDisplay();
        }
    }
}
