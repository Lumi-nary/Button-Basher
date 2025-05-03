using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EnduranceTimer : MonoBehaviour
{
    public TMP_Text timeText;
    public Endurance player1Endurance;
    public Endurance player2Endurance;
    private float elapsedTime = 0f;
    private bool isRunning = false;
    private bool player1Dead = false;
    private bool player2Dead = false;

    private void Start()
    {
        // Start both players' endurance and subscribe to their game over events
        player1Endurance.OnGameOver += () => OnPlayerGameOver(1);
        player2Endurance.OnGameOver += () => OnPlayerGameOver(2);

        player1Endurance.StartEndurance();
        player2Endurance.StartEndurance();

        isRunning = true;
    }

    private void Update()
    {
        if (!isRunning) return;

        elapsedTime += Time.deltaTime;
        timeText.text = FormatTime(elapsedTime);
    }
    void OnPlayerGameOver(int playerIndex)
    {
        if (playerIndex == 1) player1Dead = true;
        if (playerIndex == 2) player2Dead = true;

        // Stop timer if either player is out
        isRunning = false;

        // Stop both players
        player1Endurance.ForceStop();
        player2Endurance.ForceStop();

        if (playerIndex == 1)
            Debug.Log("Player 2 Wins!");
        else if (playerIndex == 2)
            Debug.Log("Player 1 Wins!");
    }
    void StopTimer()
    {
        isRunning = false;
    }

    string FormatTime(float t)
    {
        int mins = Mathf.FloorToInt(t / 60);
        int secs = Mathf.FloorToInt(t % 60);
        int ms = Mathf.FloorToInt((t * 100f) % 100);
        return $"{mins:00}:{secs:00}.{ms:00}";
    }
}
