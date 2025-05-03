using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Endurance : MonoBehaviour
{
    public float maxEndurance = 100f;
    public float currentEndurance = 100f;
    public float enduranceDrainRate = 10f; // per second
    public float enduranceGainPerPress = 5f;

    public event Action<float> OnEnduranceChanged;
    public event Action OnGameOver;

    public bool isRunning = false;

    private int randomizeCount = 0;
    private float baseDrainRate;
    private float baseGainPerPress;

    void Start()
    {
        baseDrainRate = enduranceDrainRate;
        baseGainPerPress = enduranceGainPerPress;
    }
    public void ForceStop()
    {
        isRunning = false;
    }
    private void Update()
    {
        if (!isRunning) return;

        // Drain endurance over time
        currentEndurance -= enduranceDrainRate * Time.deltaTime;
        currentEndurance = Mathf.Clamp(currentEndurance, 0, maxEndurance);

        OnEnduranceChanged?.Invoke(currentEndurance / maxEndurance);

        if (currentEndurance <= 0)
        {
            isRunning = false;
            OnGameOver?.Invoke();
        }
    }

    public void StartEndurance()
    {
        isRunning = true;
        currentEndurance = maxEndurance;
    }

    public void GainEndurance()
    {
        if (!isRunning) return;

        currentEndurance += enduranceGainPerPress;
        currentEndurance = Mathf.Clamp(currentEndurance, 0, maxEndurance);
        OnEnduranceChanged?.Invoke(currentEndurance / maxEndurance);
    }
    //public void IncreaseDrainRate(float amount)
    //{
    //    enduranceDrainRate += amount;
    //}

    //public void DecreaseGainPerPress(float amount)
    //{
    //    enduranceGainPerPress = Mathf.Max(enduranceGainPerPress - amount, 1f); // never go below 1
    //}

    //public void ApplyPenalty(float amount)
    //{
    //    currentEndurance -= amount;
    //    currentEndurance = Mathf.Clamp(currentEndurance, 0, maxEndurance);
    //    OnEnduranceChanged?.Invoke(currentEndurance / maxEndurance);
    //}

    public void OnKeysRandomized()
    {
        randomizeCount++;

        float drainMultiplier = 1f + (randomizeCount * 0.09f); 
        float gainMultiplier = 1f - (randomizeCount * 0.01f); 

        enduranceDrainRate = baseDrainRate * drainMultiplier;
        enduranceGainPerPress = Mathf.Max(baseGainPerPress * gainMultiplier, 1f); // never below 1

        Debug.Log($"[Endurance] Drain Rate: {enduranceDrainRate:F2}, Gain Per Press: {enduranceGainPerPress:F2}");
    }
}
