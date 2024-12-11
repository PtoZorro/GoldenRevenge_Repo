using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WeaponCollision : MonoBehaviour
{
    // Script para detectar enemigos golpeados por el arma

    [Header("References")]
    [SerializeField] PlayerCombat combat; // Script de control de animaciones

    private void OnTriggerEnter(Collider other)
    {
        // Verificamos si el objeto con el que colisiona es un enemigo
        EnemyCombat enemy = other.GetComponent<EnemyCombat>();

        if (enemy != null)
        {
            // Llamamos a la funci�n de aplicar da�o en el control de combate del Player
            combat.InflictDamage(enemy);
        }
    }
}
