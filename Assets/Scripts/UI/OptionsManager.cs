using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using MoreMountains.Tools;


public class OptionsManager : MonoBehaviour
{
    [Header("UI Panel Reference")]
    public GameObject optionsPanelObject;

    [Header("Feel Sound Manager Reference")]
    [Tooltip("Assign your MMSoundManager instance here if not automatically found.")]
    public MMSoundManager soundManager; // Can be auto-found if it's a singleton

    [Header("UI Element References")]
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider masterVolumeSlider;
    public Slider uiVolumeSlider;

    private bool slidersInitializedCorrectly = false;

    void Awake()
    {
        if (optionsPanelObject != null) optionsPanelObject.SetActive(false);
        else { Debug.LogError("OptionsPanelObject NULL", this); enabled = false; return; }

        if (soundManager == null) soundManager = MMSoundManager.Instance;
        if (soundManager == null) Debug.LogError("MMSoundManager instance NULL", this);
    }

    void Start()
    {
        StartCoroutine(DelayedSliderInit());
    }

    private IEnumerator DelayedSliderInit()
    {
        if (soundManager == null)
        {
            Debug.LogError("MMSoundManager is null in DelayedSliderInit. Cannot initialize sliders.");
            yield break;
        }
        // Wait a few frames to give MMSoundManager a chance to fully initialize and load settings
        yield return null;
        yield return null;
        Debug.Log($"[{Time.frameCount}] Attempting InitializeSlidersFromSoundManager. MMSoundManager Master Volume (reported): {soundManager.GetTrackVolume(MMSoundManager.MMSoundManagerTracks.Master, true)}");
        InitializeSlidersFromSoundManager();
    }

    public void ShowOptionsPanel()
    {
        if (optionsPanelObject != null)
        {
            optionsPanelObject.SetActive(true);
            Debug.Log($"[{Time.frameCount}] Options Panel Shown. Re-initializing sliders. MMSoundManager Master Volume (reported): {soundManager.GetTrackVolume(MMSoundManager.MMSoundManagerTracks.Master, true)}");
            InitializeSlidersFromSoundManager(); // Refresh sliders when panel is shown
        }
    }

    public void HideOptionsPanel()
    {
        if (optionsPanelObject != null)
        {
            optionsPanelObject.SetActive(false);
        }
    }

    private void InitializeSlidersFromSoundManager()
    {
        if (soundManager == null)
        {
            Debug.LogWarning("Cannot initialize sliders: MMSoundManager not found.");
            return;
        }

        Debug.Log("Attempting to initialize sliders from MMSoundManager...");

        if (musicVolumeSlider != null)
        {
            // Get volume BEFORE setting slider to see what Feel reports
            float reportedVol = soundManager.GetTrackVolume(MMSoundManager.MMSoundManagerTracks.Music, true);
            Debug.Log($"Music Track Volume reported by MMSoundManager: {reportedVol}");
            musicVolumeSlider.SetValueWithoutNotify(reportedVol);
        }
        // ... (similar for SFX, Master, UI with logs) ...
        if (sfxVolumeSlider != null)
        {
            float reportedVol = soundManager.GetTrackVolume(MMSoundManager.MMSoundManagerTracks.Sfx, true);
            Debug.Log($"SFX Track Volume reported by MMSoundManager: {reportedVol}");
            sfxVolumeSlider.SetValueWithoutNotify(reportedVol);
        }
        if (masterVolumeSlider != null)
        {
            float reportedVol = soundManager.GetTrackVolume(MMSoundManager.MMSoundManagerTracks.Master, true);
            Debug.Log($"Master Track Volume reported by MMSoundManager: {reportedVol}");
            masterVolumeSlider.SetValueWithoutNotify(reportedVol);
        }
        if (uiVolumeSlider != null)
        {
            float reportedVol = soundManager.GetTrackVolume(MMSoundManager.MMSoundManagerTracks.UI, true);
            Debug.Log($"UI Track Volume reported by MMSoundManager: {reportedVol}");
            uiVolumeSlider.SetValueWithoutNotify(reportedVol);
        }
        slidersInitializedCorrectly = true;
    }

    // --- Audio Settings Methods for Sliders ---
    public void SetMasterVolume(float sliderValue)
    {
        if (soundManager == null) return;
        Debug.Log($"SetMasterVolume called by slider with value: {sliderValue}");
        soundManager.SetTrackVolume(MMSoundManager.MMSoundManagerTracks.Master, sliderValue);
    }
    // ... (SetMusicVolume, SetSFXVolume, SetUIVolume are similar, with logs) ...
    public void SetMusicVolume(float sliderValue)
    {
        if (soundManager == null) return;
        Debug.Log($"SetMusicVolume called by slider with value: {sliderValue}");
        soundManager.SetTrackVolume(MMSoundManager.MMSoundManagerTracks.Music, sliderValue);
    }
    public void SetSFXVolume(float sliderValue)
    {
        if (soundManager == null) return;
        Debug.Log($"SetSFXVolume called by slider with value: {sliderValue}");
        soundManager.SetTrackVolume(MMSoundManager.MMSoundManagerTracks.Sfx, sliderValue);
    }
    public void SetUIVolume(float sliderValue)
    {
        if (soundManager == null) return;
        Debug.Log($"SetUIVolume called by slider with value: {sliderValue}");
        soundManager.SetTrackVolume(MMSoundManager.MMSoundManagerTracks.UI, sliderValue);
    }
}
