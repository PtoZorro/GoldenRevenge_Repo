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
    private IHealEvents heal; // Referencia a la interfaz de curación

    private void Awake()
    {
        // Almacenar en la variable el script seleccionado (Jugador o Enemigo) usando la interfaz especificada
        state = combatScript as IGeneralStatesEvents;
        attack = combatScript as IAttackEvents;
        roll = combatScript as IRollEvents;
        heal = combatScript as IHealEvents;
    }

    // Deshabilita la rotación
    void ManageMovement(string lockState)
    {
        state.ManageMovement(lockState);
    }

    // Permite leer el siguiente input en cierto punto de la animación
    void CanInterrupt()
    {
        state.CanInterrupt();
    }

    // Notificar que ha finalizado la animación y especificar cual es
    void OnEndAnimation(string animName)
    {
        state.OnEndAnimation(animName);
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
    
    // Habilitar impulso del esquive
    void RestoreHealth()
    {
        heal.RestoreHealth();
    }
}
