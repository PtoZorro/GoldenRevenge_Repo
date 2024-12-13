using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WeaponCollision : MonoBehaviour
{
    // Script para detectar objetivos golpeados por el arma

    [Header("References")]
    [SerializeField] MonoBehaviour combatGeneric; // Referencia genérica al script de combate;

    // Condiciones
    bool isPlayer;
    bool isEnemy;

    void Awake()
    {
        // Depende del Script asignado reconocemos si somos jugador o enemigo
        isPlayer = combatGeneric is PlayerCombat;
        isEnemy = combatGeneric is EnemyCombat;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Si somos el Jugador
        if (isPlayer)
        {
            // Verificamos si el objeto con el que colisiona es un enemigo
            EnemyCombat target = other.GetComponent<EnemyCombat>();

            if (target != null)
            {
                // Obtenemos la referencia del Script de combate
                PlayerCombat combat = combatGeneric.GetComponent<PlayerCombat>();

                // Ejercemos daño al enemigo
                combat.InflictDamage(target);
            }
        }
        else if (isEnemy) // Si somos el enemigo
        {
            // Verificamos si el objeto con el que colisiona es un enemigo
            PlayerCombat target = other.GetComponent<PlayerCombat>();

            if (target != null)
            {
                // Obtenemos la referencia del Script de combate
                EnemyCombat combat = combatGeneric.GetComponent<EnemyCombat>();

                // Ejercemos daño al Player
                combat.InflictDamage(target);
            }
        }
    }
}
