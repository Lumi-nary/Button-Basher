using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerQTEHandler : MonoBehaviour
{
    public int playerID; // 0 for Player 1, 1 for Player 2 (Assign in Inspector)
    private KeyCode activeQTEKey = KeyCode.None;
    private bool isQTEInputActive = false;

    void Update()
    {
        if (isQTEInputActive && activeQTEKey != KeyCode.None)
        {
            if (Input.GetKeyDown(activeQTEKey))
            {
                QuickEventManager.Instance.PlayerResponded(playerID);
                ClearActiveQTEKey(); // Prevent multiple responses
            }
        }
    }

    public void SetActiveQTEKey(KeyCode key)
    {
        activeQTEKey = key;
        isQTEInputActive = true;
    }

    public void ClearActiveQTEKey()
    {
        activeQTEKey = KeyCode.None;
        isQTEInputActive = false;
    }
}
