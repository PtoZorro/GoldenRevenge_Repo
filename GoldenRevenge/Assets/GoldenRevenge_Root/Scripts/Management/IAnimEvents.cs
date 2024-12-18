using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAnimationEvents
{
    // M�todos generales de animaci�n
    public interface IGeneralStatesEvents
    {
        void ManageMovement(string lockState); // M�todo para bloquear y desbloquear estados de movimiento, rotaci�n y fijado de objetivos
        void CanInterrupt(string canInterrupt); // M�todo para permitir interrupci�n
        void OnEndAnimation(string animName); // M�todo para notificar el fin de una animaci�n
    }

    // M�todos relacionados con el ataque
    public interface IAttackEvents
    {
        void AllowAttack(); // M�todo para permitir atacar
        void EnableCollider(); // M�todo para habilitar el collider del arma
        void DisableCollider(); // M�todo para deshabilitar el collider del arma
    }

    // M�todos relacionados con el esquive
    public interface IRollEvents
    {
        void ManageHitBox(string state); // M�todo para habilitar y deshabilitar la Hitbox del jugador
        void ManageImpulse(string state); // M�todo para habilitar o deshabilitar el impulso del esquive
    }
    
    // M�todos relacionados con la curaci�n
    public interface IHealEvents
    {
        void RestoreHealth(); // M�todo para restablecer la salud
    }
}