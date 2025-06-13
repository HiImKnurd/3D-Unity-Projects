using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] Slider healthbar;
    private void OnEnable()
    {
        PlayerController.OnHealthChanged += UpdateHealth;
    }
    private void OnDisable()
    {
        PlayerController.OnHealthChanged -= UpdateHealth;
    }
    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        healthbar.value = currentHealth / maxHealth;
    }
}
