using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnduranceInput : MonoBehaviour
{
    public Endurance endurance;
    public PlayerMovement playerMovement;

    private KeyCode lastKeyPressed = KeyCode.None;

    private void Start()
    {
        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        KeyCode key1 = playerMovement.key1;
        KeyCode key2 = playerMovement.key2;

        if (Input.GetKeyDown(key1) && lastKeyPressed != key1)
        {
            endurance.GainEndurance();
            lastKeyPressed = key1;
        }
        else if (Input.GetKeyDown(key2) && lastKeyPressed != key2)
        {
            endurance.GainEndurance();
            lastKeyPressed = key2;
        }
    }
}
