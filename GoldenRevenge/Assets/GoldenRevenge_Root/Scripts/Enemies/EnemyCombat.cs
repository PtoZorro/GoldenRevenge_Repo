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
    void TakeDamage()
    {
        // Restamos da�o especificado
        health -= damageRecived;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Si el arma del Player inpacta
        if (other.CompareTag("PlayerWeapon"))
        {
            // Recibir da�o
            TakeDamage();
        }
    }
}
