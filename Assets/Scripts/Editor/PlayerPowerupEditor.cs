using UnityEngine;
using UnityEditor; 
using System.Collections.Generic;

// This attribute tells Unity to use this class to draw the inspector for PlayerPowerUp
[CustomEditor(typeof(PlayerPowerUp))]
public class PlayerPowerUpEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector elements (all the public variables)
        DrawDefaultInspector();

        // Get a reference to the PlayerPowerUp script instance being inspected
        PlayerPowerUp powerUpManager = (PlayerPowerUp)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Debug Power-up Controls", EditorStyles.boldLabel);

        // Check if the game is running, as PickUp usually relies on game state
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Power-up debug buttons are only active during Play Mode.", MessageType.Info);
            return; // Don't draw buttons if not in play mode
        }

        // --- Option 1: Buttons for each specific power-up type ---
        EditorGUILayout.LabelField("Give Specific Power-up:", EditorStyles.miniBoldLabel);

        // Get all values from the PowerUpTypes enum
        PowerUpTypes[] allPowerUpTypes = (PowerUpTypes[])System.Enum.GetValues(typeof(PowerUpTypes));

        // Create a button for each power-up type
        // For better layout, you might want to put these in a horizontal group if there are many
        int buttonsPerRow = 3;
        int currentButton = 0;

        foreach (PowerUpTypes type in allPowerUpTypes)
        {
            if (currentButton % buttonsPerRow == 0)
            {
                if (currentButton > 0) EditorGUILayout.EndHorizontal(); // End previous row
                EditorGUILayout.BeginHorizontal(); // Start new row
            }

            if (GUILayout.Button("Give " + type.ToString()))
            {
                if (!powerUpManager.HasPowerUp()) // Only give if player isn't already holding one
                {
                    powerUpManager.PickUp(type);
                    Debug.Log("Gave " + type.ToString() + " to " + powerUpManager.gameObject.name + " via Inspector button.");
                }
                else
                {
                    Debug.LogWarning(powerUpManager.gameObject.name + " is already holding a power-up. Cannot give " + type.ToString() + ".");
                }
            }
            currentButton++;
        }
        if (currentButton > 0) EditorGUILayout.EndHorizontal(); // End the last row

        EditorGUILayout.Space();

        // --- Button to Clear Held Power-up ---
        if (powerUpManager.HasPowerUp())
        {
            if (GUILayout.Button("Clear Held Power-up (" + powerUpManager.GetCurrentHeldPowerUp().ToString() + ")"))
            {
#if UNITY_EDITOR
                powerUpManager.Debug_ClearHeldPowerUp();
#endif
            }
        }
    }
}