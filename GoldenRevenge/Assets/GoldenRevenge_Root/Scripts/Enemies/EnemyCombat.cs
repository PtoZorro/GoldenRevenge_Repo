using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject lockCamPoint; // Referencia al punto de fijado en c�mara del enemigo

    [Header("Stats")]
    [SerializeField] int health; // Salud del enemigo
    [SerializeField] int maxHealth; // Salud m�xima

    [Header("Stats")]
    public bool isAttacking; // Estado de atacando
    public bool rotationLocked; // Negaci�n de rotaci�n

    void Start()
    {
        // Valores de inicio
        health = maxHealth;
    }

    void Update()
    {
        // Gesti�n de la salud
        HealthManagement();
    }

    // Gesti�n de la salud
    void HealthManagement()
    {
        // Si se queda sin salud
        if (health <= 0)
        {
            // Desactivamos (provisional)
            lockCamPoint.SetActive(false);
            gameObject.SetActive(false);
        }
    }

    // Recibir da�o
    public void TakeDamage(int damageRecived)
    {
        // Restamos da�o especificado
        health -= damageRecived;
    }
}
