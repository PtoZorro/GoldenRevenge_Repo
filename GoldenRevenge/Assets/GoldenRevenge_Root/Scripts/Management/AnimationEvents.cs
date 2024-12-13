using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimationEvents : MonoBehaviour
{
    // Script exclusivo para controlar eventos de la animaci�n

    [Header("External References")]
    [SerializeField] MonoBehaviour combatScript; // Referencia gen�rica al script de combate
    private ICombatEvents combat; // Referencia a la interfaz

    private void Awake()
    {
        // Almacenar en la variable el script seleccionado (Player o Enemigo) usando la interfaz especificada
        combat = combatScript as ICombatEvents;
    }

    // Notificar que ha comenzado la animaci�n de ataque y avisar de que ataque se est� ejecutando
    void StartAttackAnimation(int attackNum) 
    {
        combat.OnStartAttack(attackNum);
    }

    // Habilitar el collider de las armas mediante la animaci�n
    void EnableCollider() 
    {
        combat.EnableCollider();
    }

    // Deshabilitar el collider de las armas mediante la animaci�n
    void DisableCollider() 
    {
        combat.DisableCollider();
    }

    // Deshabilita la rotaci�n en cierto punto de la animaci�n
    void LockRotation() 
    {
        combat.LockRotation();
    }

    // Permite leer el siguiente input en cierto punto de la animaci�n
    void CanInterrupt() 
    {
        combat.CanInterrupt();
    }

    // Notificar que se ha acabado la animaci�n de ataque
    void EndAttackAnimation() 
    {
        combat.OnEndAttack();
    }
}
