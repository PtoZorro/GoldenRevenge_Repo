using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject lockCamPoint;

    [Header("Stats")]
    [SerializeField] int health;
    [SerializeField] int maxHealth;
    [SerializeField] int damageRecived; // (Provisional, se debe mover)

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
    void TakeDamage()
    {
        // Restamos daño especificado
        health -= damageRecived;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Si el arma del Player inpacta
        if (other.CompareTag("PlayerWeapon"))
        {
            // Recibir daño
            TakeDamage();
        }
    }
}
