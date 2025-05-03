using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimeAttackManager : MonoBehaviour
{
    public TMP_Text timeText; // UI text to display time
    public bool timerRunning = false;
    private float elapsedTime = 0f;

    private void Update()
    {
        if (timerRunning)
        {
            elapsedTime += Time.deltaTime;
            timeText.text = FormatTime(elapsedTime);
        }
    }

    public void StartTimer()
    {
        timerRunning = true;
        elapsedTime = 0f;
    }

    public void StopTimer()
    {
        timerRunning = false;
    }

    string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        int milliseconds = Mathf.FloorToInt((time * 100f) % 100f);
        return $"{minutes:00}:{seconds:00}.{milliseconds:00}";
    }

    public float GetFinalTime() => elapsedTime;
}
