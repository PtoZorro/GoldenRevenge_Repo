using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject lockCamPoint; // Referencia al punto de fijado en cámara del enemigo

    [Header("Stats")]
    [SerializeField] int health; // Salud del enemigo
    [SerializeField] int maxHealth; // Salud máxima

    [Header("Stats")]
    public bool isAttacking; // Estado de atacando
    public bool rotationLocked; // Negación de rotación

    void Start()
    {
        // Valores de inicio
        health = maxHealth;
    }

    void Update()
    {
        // Gestión de la salud
        HealthManagement();
    }

    // Gestión de la salud
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

    // Recibir daño
    public void TakeDamage(int damageRecived)
    {
        // Restamos daño especificado
        health -= damageRecived;
    }
}
