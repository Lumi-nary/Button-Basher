using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QTE_UI_Display : MonoBehaviour
{
    public TextMeshProUGUI qtePromptText;

    void Awake()
    {
        if (qtePromptText == null)
        {
            qtePromptText = GetComponent<TextMeshProUGUI>();
        }
        if (qtePromptText == null)
        {
            Debug.LogError("QTE_UI_Display: TextMeshProUGUI component not found or assigned!");
            enabled = false;
            return;
        }
        qtePromptText.gameObject.SetActive(false);
    }

    public void ShowPrompt(KeyCode keyToPress)
    {
        qtePromptText.text = $"Press [{InputDisplayUtility.GetDisplayString(keyToPress)}]!";
        qtePromptText.gameObject.SetActive(true);
    }

    public void HidePrompt()
    {
        qtePromptText.gameObject.SetActive(false);
    }
}
