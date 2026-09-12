using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using MoreMountains.Feedbacks;

public class QuickEventManager : MonoBehaviour
{
    public static QuickEventManager Instance;

    [Header("QTE Settings")]
    public float minTimeBetweenQTEs = 15f;
    public float maxTimeBetweenQTEs = 30f;
    public float qteDuration = 3f; // How long players have to press the key

    [Header("Player References")]
    public PlayerMovement player1Movement; // Assign P1's PlayerMovement
    public PlayerMovement player2Movement; // Assign P2's PlayerMovement
    public PlayerQTEHandler player1QTEHandler; // Assign P1's PlayerQTEHandler
    public PlayerQTEHandler player2QTEHandler; // Assign P2's PlayerQTEHandler

    [Header("UI References")]
    public QTE_UI_Display player1QTE_UI; // Assign P1's QTE UI Display
    public QTE_UI_Display player2QTE_UI; // Assign P2's QTE UI Display

    [Header("Audio Feedbacks")]
    public MMF_Player qteClickedFeedback;
    public MMF_Player qteLoadingFeedback;
    public MMF_Player qteRewardRevealedFeedback;

    [Header("Player 1 QTE Keys (Left Side of Keyboard)")]
    public List<KeyCode> player1AvailableQTEKeys = new List<KeyCode>
    {
    // Number row
    KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5,
    // Top letter row
    KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.T,
    // Middle letter row
    KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.G,
    // Bottom letter row
    KeyCode.Z, KeyCode.X, KeyCode.C, KeyCode.V, KeyCode.B
    };

    [Header("Player 2 QTE Keys (Right Side of Keyboard)")]
    public List<KeyCode> player2AvailableQTEKeys = new List<KeyCode>
    {
    // Number row
    KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9,
    // Top letter row
    KeyCode.Y, KeyCode.U, KeyCode.I, KeyCode.O, KeyCode.P,
    // Middle letter row
    KeyCode.H, KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.Semicolon,
    // Bottom letter row
    KeyCode.N, KeyCode.M, KeyCode.Comma, KeyCode.Period, KeyCode.Slash
    };

    [Header("Reward Reveal UI")]
    public TextMeshProUGUI player1RewardRevealText;
    public TextMeshProUGUI player2RewardRevealText;
    public float rewardRevealCycleDuration = 0.07f;
    public int rewardRevealCycles = 20;
    public float rewardDisplayTime = 2.5f;

    [Header("Reward Colors")]
    public Color commonRewardColor = Color.green;
    public Color rareRewardColor = new Color(0.2f, 0.6f, 1f);
    public Color jackpotRewardColor = Color.yellow;

    private List<RewardData> definedRewards;

    // For the lottery text display
    private readonly string[] cyclingPreviewTexts = new string[] {
        "SPEED UP!", "STAMINA +", "LANE CHARGE!", "OPPONENT SLOW!",
        //"MEGA BOOST??", "SHIELD?", "TRIPLE POINTS?", "MYSTERY BOX!"
    };

    private bool isQTEActive = false;
    private KeyCode player1TargetKey;
    private KeyCode player2TargetKey;
    private Coroutine activeQTECoroutine;
    private int? qteWinnerID = null; // null if no winner yet, 0 for P1, 1 for P2

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        if (player1Movement == null || player2Movement == null ||
            qteClickedFeedback == null || qteLoadingFeedback == null || qteRewardRevealedFeedback == null)
        {
            Debug.LogError("QuickEventManager: Not all essential references are assigned!");
            enabled = false;
            return;
        }
        InitializeRewards();
        if (definedRewards == null || definedRewards.Count == 0)
        {
            Debug.LogError("No rewards defined in QuickEventManager!");
        }
        StartCoroutine(QTEScheduler());
    }

    private IEnumerator QTEScheduler()
    {
        while (true)
        {
            float waitTime = Random.Range(minTimeBetweenQTEs, maxTimeBetweenQTEs);
            yield return new WaitForSeconds(waitTime);

            if (!isQTEActive) // Only start a new QTE if one isn't already running
            {
                StartQTE();
            }
        }
    }

    void InitializeRewards()
    {
        definedRewards = new List<RewardData>();

        // --- Common Rewards ---
        definedRewards.Add(new RewardData("STAMINA REFILL!", commonRewardColor,
            (winMove, winStamina, loseMove) => { 
                if (winStamina != null) winStamina.AddStamina(winStamina.currentMaxStamina * 0.4f);
            }
        ));
        definedRewards.Add(new RewardData("LANE CHARGE +1", commonRewardColor,
            (winMove, winStamina, loseMove) => { 
                if (winMove != null) winMove.RechargeLaneSwitch(1);
            }
        ));

        // --- Rare Rewards ---
        definedRewards.Add(new RewardData("SPEED BOOST!", rareRewardColor,
            (winMove, winStamina, loseMove) => {
                this.StartCoroutine(TemporarySpeedBoost(winMove, 7f, 3.5f));
            }
        ));
        definedRewards.Add(new RewardData("OPPONENT SLOWED!", rareRewardColor,
            (winMove, winStamina, loseMove) => {
                this.StartCoroutine(TemporarySlowOpponent(loseMove, 0.7f, 2f));
            }
        ));

        // --- Jackpot Reward ---
        definedRewards.Add(new RewardData("MAX STAMINA & CHARGES!!", jackpotRewardColor,
            (winMove, winStamina, loseMove) => {
                if (winStamina != null) winStamina.AddStamina(winStamina.currentMaxStamina);
                if (winMove != null) winMove.RechargeLaneSwitch(winMove.maxLaneCharges);
            }
        ));
    }

    public void StartQTE()
    {
        if (isQTEActive) return;

        isQTEActive = true;
        qteWinnerID = null;

        // Get current movement keys AND lane switch keys to avoid them
        List<KeyCode> p1ControlKeys = new List<KeyCode> {
        player1Movement.key1, player1Movement.key2,
        player1Movement.leftSwitchKey, player1Movement.rightSwitchKey
    };
        List<KeyCode> p2ControlKeys = new List<KeyCode> {
        player2Movement.key1, player2Movement.key2,
        player2Movement.leftSwitchKey, player2Movement.rightSwitchKey
    };

        // Filter available keys for each player from their specific lists
        // Also ensure the chosen QTE key for P1 isn't one of P2's control keys, and vice-versa,
        // to prevent accidental QTE triggering if P2's hand is near P1's QTE key.
        // This is an extra layer of safety beyond just avoiding the player's own keys.

        List<KeyCode> validKeysForP1 = player1AvailableQTEKeys
                                        .Except(p1ControlKeys) // Remove P1's own control keys
                                        .Except(p2ControlKeys) // Remove P2's control keys (optional, but safer)
                                        .ToList();

        List<KeyCode> validKeysForP2 = player2AvailableQTEKeys
                                        .Except(p2ControlKeys) // Remove P2's own control keys
                                        .Except(p1ControlKeys) // Remove P1's control keys (optional, but safer)
                                        .ToList();

        if (validKeysForP1.Count == 0)
        {
            Debug.LogWarning("Player 1: Not enough unique keys available for QTE after filtering.");
            // Fallback: Use keys even if they might be P2's controls, but not P1's own.
            validKeysForP1 = player1AvailableQTEKeys.Except(p1ControlKeys).ToList();
            if (validKeysForP1.Count == 0)
            {
                Debug.LogError("Player 1: Critically no keys available for QTE!");
                isQTEActive = false;
                return;
            }
        }

        if (validKeysForP2.Count == 0)
        {
            Debug.LogWarning("Player 2: Not enough unique keys available for QTE after filtering.");
            // Fallback: Use keys even if they might be P1's controls, but not P2's own.
            validKeysForP2 = player2AvailableQTEKeys.Except(p2ControlKeys).ToList();
            if (validKeysForP2.Count == 0)
            {
                Debug.LogError("Player 2: Critically no keys available for QTE!");
                isQTEActive = false;
                return;
            }
        }


        // Assign unique keys to each player for the QTE
        player1TargetKey = validKeysForP1[Random.Range(0, validKeysForP1.Count)];

        // Try to ensure P2's key is different from P1's QTE key AND not one of P1's control keys
        List<KeyCode> tempValidKeysForP2 = validKeysForP2.Except(new List<KeyCode> { player1TargetKey }).ToList();
        if (tempValidKeysForP2.Count > 0)
        {
            player2TargetKey = tempValidKeysForP2[Random.Range(0, tempValidKeysForP2.Count)];
        }
        else if (validKeysForP2.Count > 0) // Fallback if only one key left and it's same as P1's
        {
            player2TargetKey = validKeysForP2[Random.Range(0, validKeysForP2.Count)];
            if (player2TargetKey == player1TargetKey)
            {
                Debug.LogWarning("QTE: Both players assigned the same target key due to limited options after filtering.");
            }
        }
        else // Should be caught by earlier checks, but as a safeguard
        {
            Debug.LogError("Player 2: No valid QTE key could be assigned!");
            isQTEActive = false;
            return;
        }


        Debug.Log($"QTE Started! P1: Press {player1TargetKey}, P2: Press {player2TargetKey}");

        // Notify PlayerQTEHandlers and UI
        player1QTEHandler.SetActiveQTEKey(player1TargetKey);
        player2QTEHandler.SetActiveQTEKey(player2TargetKey);

        player1QTE_UI.ShowPrompt(player1TargetKey);
        player2QTE_UI.ShowPrompt(player2TargetKey);

        activeQTECoroutine = StartCoroutine(QTECountdown());
    }

    private IEnumerator QTECountdown()
    {
        yield return new WaitForSeconds(qteDuration);

        if (isQTEActive) // If still active, means no one (or only one) pressed in time
        {
            Debug.Log("QTE timed out.");
            ResolveQTE(qteWinnerID); // qteWinnerID will be null if no one pressed, or set if one player pressed.
        }
    }

    // Called by PlayerQTEHandler when a player presses their key
    public void PlayerResponded(int playerID)
    {
        if (!isQTEActive || qteWinnerID.HasValue) return; // QTE not active or someone already won

        qteWinnerID = playerID;
        //Debug.Log($"Player {playerID + 1} responded first!");
        qteClickedFeedback?.PlayFeedbacks(transform.position);

        if (activeQTECoroutine != null)
        {
            StopCoroutine(activeQTECoroutine); // Stop the timeout countdown
        }
        ResolveQTE(qteWinnerID);
    }

    private void ResolveQTE(int? winnerID)
    {
        isQTEActive = false;
        player1QTEHandler.ClearActiveQTEKey();
        player2QTEHandler.ClearActiveQTEKey();
        player1QTE_UI.HidePrompt();
        player2QTE_UI.HidePrompt();

        if (winnerID.HasValue)
        {
            //Debug.Log($"QTE Winner: Player {winnerID.Value + 1}!");
            StartCoroutine(RevealAndApplyRewardVisual(winnerID.Value)); // New method name
        }
        else
        {
            //Debug.Log("QTE: No winner (timeout or both missed).");
            HideRewardText(player1RewardRevealText);
            HideRewardText(player2RewardRevealText);
        }
    }

    private IEnumerator RevealAndApplyRewardVisual(int winningPlayerID)
    {
        if (definedRewards == null || definedRewards.Count == 0)
        {
            Debug.LogError("No rewards defined. Cannot apply reward.");
            yield break;
        }

        TextMeshProUGUI winnerRewardTextUI = (winningPlayerID == 0) ? player1RewardRevealText : player2RewardRevealText;
        TextMeshProUGUI loserRewardTextUI = (winningPlayerID == 0) ? player2RewardRevealText : player1RewardRevealText;

        HideRewardText(loserRewardTextUI); // Hide loser's text

        winnerRewardTextUI.gameObject.SetActive(true);
        winnerRewardTextUI.alpha = 1f; // Ensure it's visible

        qteLoadingFeedback?.PlayFeedbacks(transform.position);

        // 1. Lottery cycling phase with random colors (or just one cycle color)
        Color cycleColor = Color.white; // Color for cycling text
        for (int i = 0; i < rewardRevealCycles; i++)
        {
            winnerRewardTextUI.text = cyclingPreviewTexts[Random.Range(0, cyclingPreviewTexts.Length)];
            winnerRewardTextUI.color = cycleColor;
            // Simple scale animation
            winnerRewardTextUI.transform.localScale = Vector3.one * (1f + Mathf.Sin(Time.time * 20f + i) * 0.05f); // Subtle pulse
            yield return new WaitForSeconds(rewardRevealCycleDuration);
        }
        winnerRewardTextUI.transform.localScale = Vector3.one; // Reset scale

        // 2. Determine and display the actual reward
        // For now, random selection. Later you can implement weighted random for rarities.
        RewardData actualReward = definedRewards[Random.Range(0, definedRewards.Count)];

        winnerRewardTextUI.text = actualReward.displayText;
        winnerRewardTextUI.color = actualReward.displayColor; // Use the reward's defined color

        // "Pop" animation for final reward
        qteRewardRevealedFeedback?.PlayFeedbacks(transform.position);
        StartCoroutine(AnimateTextPop(winnerRewardTextUI, 1.2f, 0.15f));
        // TODO: Play a sound effect based on reward rarity (common, rare, jackpot)

        Debug.Log($"Player {winningPlayerID + 1} won: {actualReward.displayText}");

        // 3. Apply the actual reward effect
        PlayerMovement winnerMovement = (winningPlayerID == 0) ? player1Movement : player2Movement;
        PlayerStamina winnerStamina = winnerMovement.GetComponent<PlayerStamina>();
        PlayerMovement loserMovement = (winningPlayerID == 0) ? player2Movement : player1Movement;
        actualReward.applyAction(winnerMovement, winnerStamina, loserMovement);

        // 4. Keep reward displayed for a bit, then fade out
        yield return new WaitForSeconds(rewardDisplayTime - 0.5f); // Start fade a bit before full display time
        StartCoroutine(AnimateTextFadeOut(winnerRewardTextUI, 0.5f));
    }

    private void HideRewardText(TextMeshProUGUI textUI)
    {
        if (textUI != null)
        {
            textUI.gameObject.SetActive(false);
        }
    }

    // --- Simple Text Animation Coroutines ---
    private IEnumerator AnimateTextPop(TextMeshProUGUI textUI, float targetScale, float duration)
    {
        if (textUI == null) yield break;
        Vector3 originalScale = Vector3.one;
        Vector3 scaledUp = Vector3.one * targetScale;
        float halfDuration = duration / 2f;

        // Scale up
        float timer = 0f;
        while (timer < halfDuration)
        {
            textUI.transform.localScale = Vector3.Lerp(originalScale, scaledUp, timer / halfDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        textUI.transform.localScale = scaledUp;

        // Scale down
        timer = 0f;
        while (timer < halfDuration)
        {
            textUI.transform.localScale = Vector3.Lerp(scaledUp, originalScale, timer / halfDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        textUI.transform.localScale = originalScale;
    }

    private IEnumerator AnimateTextFadeOut(TextMeshProUGUI textUI, float duration)
    {
        if (textUI == null) yield break;
        float startAlpha = textUI.alpha;
        float timer = 0f;

        while (timer < duration)
        {
            textUI.alpha = Mathf.Lerp(startAlpha, 0f, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }
        textUI.alpha = 0f;
        textUI.gameObject.SetActive(false); // Hide after fading
    }

    private void ActualApplyRandomReward(int winningPlayerID)
    {
        PlayerMovement winnerMovement = (winningPlayerID == 0) ? player1Movement : player2Movement;
        PlayerStamina winnerStamina = winnerMovement.GetComponent<PlayerStamina>();
        PlayerMovement loserMovement = (winningPlayerID == 0) ? player2Movement : player1Movement;

        int rewardType = Random.Range(0, 4);
        Debug.Log($"Player {winningPlayerID + 1} gets reward type (direct apply): {rewardType}");

        switch (rewardType)
        {
            case 0: StartCoroutine(TemporarySpeedBoost(winnerMovement, 5f, 3f)); break;
            case 1: if (winnerStamina != null) winnerStamina.AddStamina(winnerStamina.currentMaxStamina * 0.5f); break;
            case 2: if (winnerMovement != null) winnerMovement.RechargeLaneSwitch(1); break;
            case 3: StartCoroutine(TemporarySlowOpponent(loserMovement, 0.75f, 1.5f)); break;
        }
    }


    private IEnumerator TemporarySpeedBoost(PlayerMovement targetMovement, float boostAmount, float duration)
    {
        if (targetMovement == null) yield break;
        float originalMaxSpeed = targetMovement.maxSpeed;
        targetMovement.maxSpeed += boostAmount;
        targetMovement.moveSpeed = Mathf.Min(targetMovement.moveSpeed + boostAmount, targetMovement.maxSpeed); // Optional immediate nudge
        yield return new WaitForSeconds(duration);
        targetMovement.maxSpeed = originalMaxSpeed;
        targetMovement.moveSpeed = Mathf.Min(targetMovement.moveSpeed, targetMovement.maxSpeed);
    }
    private IEnumerator TemporarySlowOpponent(PlayerMovement targetMovement, float slowFactor, float duration)
    {
        if (targetMovement == null) yield break;
        float originalMaxSpeed = targetMovement.maxSpeed;
        targetMovement.maxSpeed *= slowFactor;
        targetMovement.moveSpeed = Mathf.Min(targetMovement.moveSpeed, targetMovement.maxSpeed);
        yield return new WaitForSeconds(duration);
        targetMovement.maxSpeed = originalMaxSpeed;
    }
}
