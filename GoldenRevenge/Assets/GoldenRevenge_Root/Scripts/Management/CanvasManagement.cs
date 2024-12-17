using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManagement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Image healthBar;
    [SerializeField] Image staminaBar;

    void Start()
    {
        
    }

    void Update()
    {
        // Llenado de barras de estado
        StateBarsFilling();
    }

    void StateBarsFilling()
    {
        // Llenamos las barras de Salud y Stamina según los valores del Singleton
        healthBar.fillAmount = (float) GameManager.Instance.health / GameManager.Instance.maxHealth;
        staminaBar.fillAmount = (float) GameManager.Instance.stamina / GameManager.Instance.maxStamina;
    }
}
