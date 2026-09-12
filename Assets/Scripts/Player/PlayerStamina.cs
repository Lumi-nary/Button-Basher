using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerStamina : MonoBehaviour
{
    [Header("Stamina Settings")]
    public float baseMaxStamina = 100f;
    [Tooltip("Current stamina of the player.")]
    public float currentStamina;
    public float staminaRegenRate = 10f;
    public float exhaustThreshold = 0f;

    [HideInInspector]
    public float currentMaxStamina;

    public bool isExhausted = false;
    public event Action<float, float> OnStaminaChanged; 

    [Header("UI References")]
    public TMP_Text exhaustedText;

    private Coroutine temporaryMaxStaminaCoroutine;
    private Coroutine temporaryMaxStaminaModifierCoroutine;

    private void Awake()
    {
        currentMaxStamina = baseMaxStamina;
        currentStamina = currentMaxStamina;
        UpdateExhaustionStatus();
    }

    private void Start() 
    {
        OnStaminaChanged?.Invoke(currentStamina, currentMaxStamina); // Initial UI update
    }

    private void Update()
    {
        RegenerateStamina();
    }


    private void RegenerateStamina()
    {
        // Use currentMaxStamina for regeneration checks
        if (currentStamina < currentMaxStamina && !isExhausted)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            currentStamina = Mathf.Min(currentStamina, currentMaxStamina);
            OnStaminaChanged?.Invoke(currentStamina, currentMaxStamina);
        }
        else if (isExhausted)
        {
            currentStamina += staminaRegenRate * Time.deltaTime; // Or a different recoveryRate
            currentStamina = Mathf.Min(currentStamina, currentMaxStamina);
            OnStaminaChanged?.Invoke(currentStamina, currentMaxStamina);

            if (currentStamina >= currentMaxStamina * 0.5f) // Example: recover from exhaustion at 50% of current max
            {
                isExhausted = false;
                // Debug.Log(gameObject.name + " recovered from exhaustion.");
                UpdateExhaustionStatus();
            }
        }
    }
    /// <summary>
    /// Consume Stamina based on the amount specified.
    /// </summary>
    /// <param name="amount">Amount of stamina to consume.</param>
    public void ConsumeStamina(float amount)
    {
        if (isExhausted) return;
        currentStamina -= amount;
        currentStamina = Mathf.Max(currentStamina, 0f);
        OnStaminaChanged?.Invoke(currentStamina, currentMaxStamina);
        CheckAndSetExhaustion();
    }

    /// <summary>
    /// Adds a specified amount to current stamina, up to maxStamina.
    /// </summary>
    /// <param name="amount">Amount of stamina to add.</param>
    public void AddStamina(float amount)
    {
        currentStamina += amount;
        currentStamina = Mathf.Min(currentStamina, currentMaxStamina); // Clamp to current dynamic max
        OnStaminaChanged?.Invoke(currentStamina, currentMaxStamina);
        // Debug.Log(gameObject.name + " gained " + amount + " stamina. Current: " + currentStamina);
        if (isExhausted && currentStamina >= currentMaxStamina * 0.5f)
        {
            isExhausted = false;
            // Debug.Log(gameObject.name + " recovered from exhaustion due to added stamina.");
        }
        UpdateExhaustionStatus();
    }

    /// <summary>
    /// Reduces a specified amount from current stamina, down to 0.
    /// Can trigger exhaustion if stamina drops to the threshold.
    /// </summary>
    /// <param name="amount">Amount of stamina to reduce.</param>
    public void ReduceStamina(float amount)
    {
        currentStamina -= amount;
        currentStamina = Mathf.Max(currentStamina, 0f);
        OnStaminaChanged?.Invoke(currentStamina, currentMaxStamina);
        // Debug.Log(gameObject.name + " lost " + amount + " stamina. Current: " + currentStamina);
        CheckAndSetExhaustion();
    }

    /// <summary>
    /// Checks if stamina is at or below the exhaust threshold and updates the isExhausted flag.
    /// Also updates the UI text.
    /// </summary>
    private void CheckAndSetExhaustion()
    {
        if (currentStamina <= exhaustThreshold)
        {
            if (!isExhausted)
            {
                isExhausted = true;
                // Debug.Log(gameObject.name + " is exhausted!");
            }
        }
        UpdateExhaustionStatus();
    }

    /// <summary>
    /// Updates the exhaustion UI text based on the isExhausted flag.
    /// </summary>
    private void UpdateExhaustionStatus()
    {
        if (exhaustedText != null)
        {
            exhaustedText.SetText(isExhausted ? "Exhausted" : "");
        }
    }

    public void ApplyTemporaryMaxStaminaBoost(float newMaxStamina, float duration)
    {
        if (temporaryMaxStaminaCoroutine != null)
        {
            StopCoroutine(temporaryMaxStaminaCoroutine); // Stop any existing max stamina buff
        }
        temporaryMaxStaminaCoroutine = StartCoroutine(MaxStaminaBoostCoroutine(newMaxStamina, duration));
    }

    private IEnumerator MaxStaminaBoostCoroutine(float boostedMaxStamina, float duration)
    {
        float originalCurrentMax = currentMaxStamina;

        currentMaxStamina = boostedMaxStamina;
        currentStamina = boostedMaxStamina; // Refill to the new maximum

        Debug.Log($"{gameObject.name}: Max stamina boosted to {currentMaxStamina}, current stamina refilled.");
        OnStaminaChanged?.Invoke(currentStamina, currentMaxStamina); // Update UI

        yield return new WaitForSeconds(duration);

        currentMaxStamina = baseMaxStamina; // Revert to base, or to 'originalCurrentMax' if that's more appropriate
        currentStamina = Mathf.Min(currentStamina, currentMaxStamina); // Clamp current stamina to new (lower) max

        Debug.Log($"{gameObject.name}: Max stamina boost ended. Max reverted to {currentMaxStamina}. Current stamina: {currentStamina}");
        OnStaminaChanged?.Invoke(currentStamina, currentMaxStamina); // Update UI
        CheckAndSetExhaustion(); // Re-check exhaustion state if stamina dropped significantly

        temporaryMaxStaminaCoroutine = null;
    }
    private IEnumerator MaxStaminaModifierCoroutine(float targetMaxStamina, float duration, bool setCurrentToZero, bool refillToNewMax)
    {
        // Store the max stamina value that was active *before* this specific modifier started.
        // This helps revert correctly if multiple different modifiers could overlap (though current design stops previous).
        float maxStaminaBeforeThisModifier = currentMaxStamina;

        currentMaxStamina = targetMaxStamina; // Apply new max

        if (setCurrentToZero)
        {
            currentStamina = 0f;
            Debug.Log($"{gameObject.name}: Current stamina set to 0 by debuff.");
        }
        else if (refillToNewMax)
        {
            currentStamina = currentMaxStamina; // Refill to the new maximum
            Debug.Log($"{gameObject.name}: Max stamina set to {currentMaxStamina}, current stamina refilled.");
        }
        else
        {
            // If not setting to zero or refilling, just clamp current stamina to the new max
            currentStamina = Mathf.Min(currentStamina, currentMaxStamina);
        }

        OnStaminaChanged?.Invoke(currentStamina, currentMaxStamina);
        CheckAndSetExhaustion(); // Important if current stamina became 0

        yield return new WaitForSeconds(duration);

        currentMaxStamina = baseMaxStamina;

        currentStamina = Mathf.Min(currentStamina, currentMaxStamina); // Clamp current stamina to new (reverted) max

        Debug.Log($"{gameObject.name}: Max stamina modifier ended. Max reverted to {currentMaxStamina}. Current: {currentStamina}");
        OnStaminaChanged?.Invoke(currentStamina, currentMaxStamina);
        CheckAndSetExhaustion();

        temporaryMaxStaminaModifierCoroutine = null;
    }

    public void ApplyTemporaryMaxStaminaModifier(float newMaxStaminaValue, float duration, bool setCopyToZero = false, bool refillToNewMax = false)
    {
        if (temporaryMaxStaminaModifierCoroutine != null)
        {
            StopCoroutine(temporaryMaxStaminaModifierCoroutine);
            currentStamina = Mathf.Min(currentStamina, currentMaxStamina); // Clamp current
            OnStaminaChanged?.Invoke(currentStamina, currentMaxStamina);
        }
        temporaryMaxStaminaModifierCoroutine = StartCoroutine(MaxStaminaModifierCoroutine(newMaxStaminaValue, duration, setCopyToZero, refillToNewMax));
    }

}
