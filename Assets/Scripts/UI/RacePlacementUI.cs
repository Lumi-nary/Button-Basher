using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using BezierSolution;

public class RacePlacementUI : MonoBehaviour
{
    [Header("Players")]
    public Transform player1;
    public Transform player2;

    [Header("Track")]
    public BezierSpline trackSpline; // Reference to your BezierSolution spline
    public float lapThreshold = 0.95f; // Threshold to detect lap completion

    [Header("UI")]
    public TMP_Text player1PositionText;    // UI text to display player 1's position
    public TMP_Text player2PositionText;    // UI text to display player 2's position

    // Track progress variables
    private int player1LapCount = 0;
    private int player2LapCount = 0;
    private float player1Progress = 0f;  // 0 to 1 representing progress through the track
    private float player2Progress = 0f;
    private float player1PrevProgress = 0f; // Used to detect lap completion
    private float player2PrevProgress = 0f;

    // Start is called before the first frame update
    void Start()
    {
        if (trackSpline == null)
        {
            Debug.LogError("No BezierSpline assigned to RacePositionManager!");
            return;
        }

        // Initialize positions
        UpdateRacePositions();
    }

    // Update is called once per frame
    void Update()
    {
        player1PrevProgress = player1Progress;
        player2PrevProgress = player2Progress;

        UpdatePlayerProgress(player1, ref player1LapCount, ref player1Progress, player1PrevProgress);
        UpdatePlayerProgress(player2, ref player2LapCount, ref player2Progress, player2PrevProgress);

        UpdateRacePositions();
    }

    void UpdatePlayerProgress(Transform player, ref int lapCount, ref float progress, float prevProgress)
    {
        if (trackSpline == null) return;

        // Find the closest point on the spline to the player's position
        float normalizedT;
        Vector3 closestPoint = trackSpline.FindNearestPointTo(player.position, out normalizedT);
        progress = normalizedT;

        // Detect lap completion (when progress jumps from near 1 back to near 0)
        if (prevProgress > lapThreshold && progress < 0.1f)
        {
            //lapCount++; todo later
            //Debug.Log($"Player completed lap {lapCount}");
            return;
        }
    }

    void UpdateRacePositions()
    {
        // Compare progress to determine positions
        string player1Position = "2nd";
        string player2Position = "1st";

        // Calculate total progress
        float player1TotalProgress = player1LapCount + player1Progress;
        float player2TotalProgress = player2LapCount + player2Progress;

        // Determine positions
        if (player1TotalProgress > player2TotalProgress)
        {
            player1Position = "1st";
            player2Position = "2nd";
        }

        // Update UI
        if (player1PositionText != null)
        {
            player1PositionText.text = player1Position;
        }

        if (player2PositionText != null)
        {
            player2PositionText.text = player2Position;
        }
    }
}
