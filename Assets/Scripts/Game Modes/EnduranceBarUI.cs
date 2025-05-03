using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnduranceBarUI : MonoBehaviour
{
    public Image fillImage;
    public Endurance endurance;

    private void OnEnable()
    {
        endurance.OnEnduranceChanged += UpdateBar;
    }

    private void OnDisable()
    {
        endurance.OnEnduranceChanged -= UpdateBar;
    }

    private void UpdateBar(float normalizedValue)
    {
        fillImage.fillAmount = normalizedValue;
    }
}
