using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static IAnimationEvents;

public class AnimationEvents : MonoBehaviour
{
    // Script exclusivo para controlar eventos de la animación

    [Header("External References")]
    [SerializeField] MonoBehaviour combatScript; // Referencia genérica al script de combate

    // Interfaces
    private IGeneralStatesEvents state; // Referencia a la interfaz de estados generales
    private IAttackEvents attack; // Referencia a la interfaz de ataque
    private IRollEvents roll; // Referencia a la interfaz de esquive

    private void Awake()
    {
        // Almacenar en la variable el script seleccionado (Jugador o Enemigo) usando la interfaz especificada
        state = combatScript as IGeneralStatesEvents;
        attack = combatScript as IAttackEvents;
        roll = combatScript as IRollEvents;
    }

    // Notificar que ha comenzado la animación y especificar cual es
    void OnStartAnimation(string animName) 
    {
        state.OnStartAnimation(animName);
    }

    // Notificar que ha finalizado la animación y especificar cual es
    void OnEndAnimation(string animName)
    {
        state.OnEndAnimation(animName);
    }

    // Deshabilita la rotación
    void ManageRotation(string lockState)
    {
        state.ManageRotation(lockState);
    }

    // Permite leer el siguiente input en cierto punto de la animación
    void CanInterrupt()
    {
        state.CanInterrupt();
    }

    // Notifica que ha empezado el ataque e indica su número
    void OnStartAttack(int attackNum)
    {
        attack.OnStartAttack(attackNum);
    }

    // Habilitar el collider de las armas mediante la animación
    void EnableCollider() 
    {
        attack.EnableCollider();
    }

    // Deshabilitar el collider de las armas mediante la animación
    void DisableCollider() 
    {
        attack.DisableCollider();
    }

    // Habilitar y deshabilitar la Hitbox del jugador
    void ManageHitBox(string state)
    {
        roll.ManageHitBox(state);
    }

    // Habilitar impulso del esquive
    void ManageImpulse(string state)
    {
        roll.ManageImpulse(state);
    }
}
