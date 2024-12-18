using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CanvasManagement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Image healthBar;
    [SerializeField] Image staminaBar;
    [SerializeField] Image healingImage;
    [SerializeField] TMP_Text healingsNum;
    [SerializeField] Animator transitionAnim;

    void Update()
    {
        // Llenado de barras de estado
        StateBarsFilling();

        // Mostrar cantidad de items
        ShowItemNumber();

        // Gestión del panel de transición
        TransitionPanel();
    }

    // Llenado de barras de estado
    void StateBarsFilling()
    {
        // Llenamos las barras de Salud y Stamina según los valores del Singleton
        healthBar.fillAmount = (float) GameManager.Instance.health / GameManager.Instance.maxHealth;
        staminaBar.fillAmount = (float) GameManager.Instance.stamina / GameManager.Instance.maxStamina;
    }

    // Mostrar cantidad de items
    void ShowItemNumber()
    {
        healingsNum.text = GameManager.Instance.healItems.ToString();

        // Mostrar poción vacía cuando no tengamos curativos
        healingImage.enabled = (GameManager.Instance.healItems != 0);
    }

    // Gestión del panel de transición
    void TransitionPanel()
    {
        // Si estamos muertos, activamos la transición
        bool panelOn = GameManager.Instance.dead ? true : false;
        bool panelOff = panelOn ? false : true;

        // Gestion animación del panel
        transitionAnim.SetBool("on", panelOn);
        transitionAnim.SetBool("off", panelOff);
    }
}
