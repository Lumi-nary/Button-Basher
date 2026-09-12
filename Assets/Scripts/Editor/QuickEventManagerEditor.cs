using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(QuickEventManager))]
public class QuickEventManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector elements (all the public variables)
        DrawDefaultInspector();

        // Get a reference to the QuickEventManager script instance being inspected
        QuickEventManager qteManager = (QuickEventManager)target;

        // Add a little space
        EditorGUILayout.Space();

        // Add a button
        if (GUILayout.Button("Manually Start QTE"))
        {
            // Check if the game is running because StartQTE might rely on game state
            if (Application.isPlaying)
            {
                qteManager.StartQTE();
                Debug.Log("Manually triggered QTE via Inspector button.");
            }
            else
            {
                Debug.LogWarning("Cannot start QTE: Game is not running. Press Play first.");
            }
        }
    }
}
