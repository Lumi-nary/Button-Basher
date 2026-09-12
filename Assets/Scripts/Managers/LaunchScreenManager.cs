using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LaunchScreenManager : MonoBehaviour
{
    [Header("Timing")]
    [Tooltip("How long the splash screen stays visible after full fade-in (in seconds).")]
    public float displayDuration = 2.0f;
    [Tooltip("Duration of the fade-in effect (in seconds).")]
    public float fadeInDuration = 1.0f;
    [Tooltip("Duration of the fade-out effect (in seconds).")]
    public float fadeOutDuration = 1.0f;

    [Header("Scene To Load")]
    [Tooltip("Name of the Main Menu scene to load after the splash screen.")]
    public string mainMenuSceneName = "MainMenu";

    [Header("UI Elements (Optional for Fade)")]
    [Tooltip("The main UI element (e.g., an Image with the logo) to fade.")]
    public CanvasGroup splashCanvasGroup;

    private float timer;

    void Start()
    {
        if (splashCanvasGroup == null)
        {
            splashCanvasGroup = GetComponent<CanvasGroup>();
            if (splashCanvasGroup == null)
                Debug.LogWarning("LaunchScreenManager: SplashCanvasGroup not assigned. Fade effects will be skipped.");
        }

        if (string.IsNullOrEmpty(mainMenuSceneName))
        {
            Debug.LogError("LaunchScreenManager: MainMenuSceneName is not set!");
            enabled = false; // Disable if no scene to go to
            return;
        }

        StartCoroutine(LaunchSequence());
    }

    private System.Collections.IEnumerator LaunchSequence()
    {
        // 1. Fade In
        if (splashCanvasGroup != null)
        {
            splashCanvasGroup.alpha = 0f;
            float elapsedTime = 0f;
            while (elapsedTime < fadeInDuration)
            {
                splashCanvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeInDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            splashCanvasGroup.alpha = 1f;
        }
        else
        {
            yield return new WaitForSeconds(fadeInDuration);
        }

        // 2. Display Duration
        yield return new WaitForSeconds(displayDuration);

        // 3. Fade Out
        if (splashCanvasGroup != null)
        {
            float elapsedTime = 0f;
            while (elapsedTime < fadeOutDuration)
            {
                splashCanvasGroup.alpha = 1f - Mathf.Clamp01(elapsedTime / fadeOutDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            splashCanvasGroup.alpha = 0f;
        }
        else
        {
            yield return new WaitForSeconds(fadeOutDuration);
        }

        // 4. Load Main Menu Scene
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
