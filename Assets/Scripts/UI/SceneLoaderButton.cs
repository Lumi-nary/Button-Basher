using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))] // Ensures this script is always on a GameObject with a Button
public class SceneLoaderButton : MonoBehaviour
{
    [Header("Scene Navigation")]
    [Tooltip("The exact name of the scene to load when this button is clicked.")]
    public string sceneNameToLoad;

    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("SceneLoaderButton requires a Button component on the same GameObject.", this);
            enabled = false; // Disable script if no button found
            return;
        }
    }

    void Start()
    {
        // Check if scene name is provided
        if (string.IsNullOrEmpty(sceneNameToLoad))
        {
            Debug.LogWarning("SceneNameToLoad is not set on SceneLoaderButton: " + gameObject.name + ". Button will do nothing.", this);
            // Optionally disable the button if no scene is set
            // button.interactable = false;
            return; // Don't add listener if no scene is specified
        }

        // Add a listener to the button's onClick event to call our LoadTargetScene method
        button.onClick.AddListener(LoadTargetScene);
    }

    void OnDestroy()
    {
        // Clean up the listener when the button is destroyed to prevent issues
        if (button != null)
        {
            button.onClick.RemoveListener(LoadTargetScene);
        }
    }

    public void LoadTargetScene()
    {
        if (string.IsNullOrEmpty(sceneNameToLoad))
        {
            Debug.LogError("SceneNameToLoad is not set or is empty for button: " + gameObject.name, this);
            return;
        }

        Debug.Log("Loading scene: " + sceneNameToLoad, this);
        SceneManager.LoadScene(sceneNameToLoad);
    }

    // Optional: A method to change the scene name at runtime if needed
    public void SetTargetScene(string newSceneName)
    {
        sceneNameToLoad = newSceneName;
        if (string.IsNullOrEmpty(sceneNameToLoad))
        {
            if (button != null) button.interactable = false; // Disable if new scene name is invalid
        }
        else
        {
            if (button != null) button.interactable = true;
        }
    }
}
