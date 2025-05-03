using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStamina : MonoBehaviour
{
    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float currentStamina = 100f;
    public float staminaRegenRate = 10f;

    public bool isExhausted = false;

    public event Action<float> OnStaminaChanged;

    private void Update()
    {
        RegenerateStamina();
    }

    private void RegenerateStamina()
    {
        if (currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            currentStamina = Mathf.Min(currentStamina, maxStamina);
            if (currentStamina >= maxStamina)
                isExhausted = false;

            OnStaminaChanged?.Invoke(currentStamina / maxStamina);
        }
    }
    /// <summary>
    /// Call this method to reduce stamina.
    /// </summary>
    /// <param name="amount">Amount of stamina to consume.</param>
    public void ConsumeStamina(float amount)
    {
        currentStamina -= amount;
        currentStamina = Mathf.Max(currentStamina, 0f);

        // If stamina reaches 0, set the exhaustion flag.
        if (currentStamina == 0)
            isExhausted = true;

        OnStaminaChanged?.Invoke(currentStamina / maxStamina);
    }
}
