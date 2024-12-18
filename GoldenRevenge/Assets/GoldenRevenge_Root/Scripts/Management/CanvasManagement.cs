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

        // Gesti�n del panel de transici�n
        TransitionPanel();
    }

    // Llenado de barras de estado
    void StateBarsFilling()
    {
        // Llenamos las barras de Salud y Stamina seg�n los valores del Singleton
        healthBar.fillAmount = (float) GameManager.Instance.health / GameManager.Instance.maxHealth;
        staminaBar.fillAmount = (float) GameManager.Instance.stamina / GameManager.Instance.maxStamina;
    }

    // Mostrar cantidad de items
    void ShowItemNumber()
    {
        healingsNum.text = GameManager.Instance.healItems.ToString();

        // Mostrar poci�n vac�a cuando no tengamos curativos
        healingImage.enabled = (GameManager.Instance.healItems != 0);
    }

    // Gesti�n del panel de transici�n
    void TransitionPanel()
    {
        // Si estamos muertos, activamos la transici�n
        bool panelOn = GameManager.Instance.dead ? true : false;
        bool panelOff = panelOn ? false : true;

        // Gestion animaci�n del panel
        transitionAnim.SetBool("on", panelOn);
        transitionAnim.SetBool("off", panelOff);
    }
}
