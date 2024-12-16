using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAnimationEvents
{
    // M�todos generales de animaci�n
    public interface IGeneralStatesEvents
    {
        void OnStartAnimation(string animName); // M�todo para notificar inicio de una animaci�n
        void OnEndAnimation(string animName); // M�todo para notificar el fin de una animaci�n
        void ManageRotation(string lockState); // M�todo para bloquear y desbloquear la rotaci�n
        void CanInterrupt(); // M�todo para permitir interrupci�n
    }

    // M�todos relacionados con el ataque
    public interface IAttackEvents
    {
        void OnStartAttack(int attackNum); // M�todo para notificar inicio de un ataque
        void EnableCollider(); // M�todo para habilitar el collider del arma
        void DisableCollider(); // M�todo para deshabilitar el collider del arma
    }

    // M�todos relacionados con el esquive
    public interface IRollEvents
    {
        void ManageHitBox(string state); // M�todo para habilitar y deshabilitar la Hitbox del jugador
        void ManageImpulse(string state); // M�todo para habilitar o deshabilitar el impulso del esquive
    }
}