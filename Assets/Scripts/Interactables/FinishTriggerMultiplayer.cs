using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FinishTriggerMultiplayer : MonoBehaviour
{
    [Header("Players")]
    public PlayerMovement player1;
    public PlayerMovement player2;

    [Header("UI References")]
    public CanvasGroup blackOverlay;
    public RectTransform resultsPanel;
    public TMP_Text winnerText;

    [Header("Winner Showcase")]
    public Transform winnerShowcasePoint; // Where winner is teleported
    public Camera raceCamera;
    public Camera raceCamera2;
    public Camera showcaseCamera; // Or Cinemachine virtual camera

    [Header("Animation Settings")]
    public float fadeDuration = 1f;
    public float panelSlideDuration = 0.5f;
    public Vector2 resultsOffscreenPos;
    public Vector2 resultsOnscreenPos;

    private bool raceEnded = false;

    private void Start()
    {
        // Make sure blackOverlay starts fully transparent
        if (blackOverlay != null)
        {
            blackOverlay.alpha = 0f;
        }

        // Make sure showcaseCamera is disabled at start
        if (showcaseCamera != null)
        {
            showcaseCamera.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (raceEnded) return;

        PlayerMovement winner = other.GetComponent<PlayerMovement>();
        if (winner == null) return;

        winner.moveSpeed = 0f; // Stop the winner's movement
        raceEnded = true;
        StartCoroutine(HandleFinish(winner));
    }

    private IEnumerator HandleFinish(PlayerMovement winner)
    {

        // 1. Stop all movement
        player1.enabled = false;
        player2.enabled = false;

        // Force animation state change immediately
        var animator = winner.GetComponentInChildren<Animator>();
        if (animator != null)
        {
            // Reset all animation states first
            animator.SetBool("isRunning", false);
            animator.SetBool("isWalking", false);

            // Force the idle animation explicitly
            animator.SetBool("isIdling", true);

            // Apply the changes immediately
            animator.Update(0f);

            Debug.Log("Animation states reset: Running=" + animator.GetBool("isRunning") +
                      ", Walking=" + animator.GetBool("isWalking") +
                      ", Idling=" + animator.GetBool("isIdling"));
        }

        // 2. Fade to black
        yield return StartCoroutine(FadeBlackOverlay(0f, 1f));

        // Wait a moment at full black
        yield return new WaitForSeconds(0.5f);

        // 3. Teleport winner to showcase point
        winner.transform.position = winnerShowcasePoint.position;
        winner.transform.rotation = winnerShowcasePoint.rotation;

        // 4. Switch camera
        raceCamera.enabled = false;
        showcaseCamera.enabled = true;

        // 5. Fade from black
        yield return StartCoroutine(FadeBlackOverlay(1f, 0f));

        // 6. Slide in results panel
        // Show winner text
        winnerText.text = (winner == player1 ? "Player 1" : "Player 2") + " Wins!";
        resultsPanel.anchoredPosition = resultsOffscreenPos;
        yield return new WaitForSeconds(0.2f);

        float u = 0f;
        while (u < panelSlideDuration)
        {
            u += Time.deltaTime;
            resultsPanel.anchoredPosition = Vector2.Lerp(
                resultsOffscreenPos,
                resultsOnscreenPos,
                Mathf.SmoothStep(0, 1, u / panelSlideDuration)
            );
            yield return null;
        }
        resultsPanel.anchoredPosition = resultsOnscreenPos;
    }

    private IEnumerator FadeBlackOverlay(float startAlpha, float targetAlpha)
    {
        // Ensure the canvas group exists
        if (blackOverlay == null)
        {
            Debug.LogError("Black overlay canvas group is missing!");
            yield break;
        }

        // Set the starting alpha
        blackOverlay.alpha = startAlpha;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(t / fadeDuration);
            blackOverlay.alpha = Mathf.Lerp(startAlpha, targetAlpha, normalizedTime);
            yield return null;
        }

        // Ensure we reach the exact target alpha
        blackOverlay.alpha = targetAlpha;
    }
}
