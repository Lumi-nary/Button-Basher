using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct RewardData
{
    public string displayText;
    public Color displayColor;
    public System.Action<PlayerMovement, PlayerStamina, PlayerMovement> applyAction; // Action to apply the reward
                                                                                     // Params: winnerMove, winnerStamina, loserMove

    // Add rarity later
    // public enum Rarity { Common, Rare, Jackpot }
    // public Rarity rarityTier;

    public RewardData(string text, Color color, System.Action<PlayerMovement, PlayerStamina, PlayerMovement> action)
    {
        displayText = text;
        displayColor = color;
        applyAction = action;
    }
}
