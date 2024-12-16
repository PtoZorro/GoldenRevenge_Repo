using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAnimationEvents
{
    // Métodos generales de animación
    public interface IGeneralStatesEvents
    {
        void OnStartAnimation(string animName); // Método para notificar inicio de una animación
        void OnEndAnimation(string animName); // Método para notificar el fin de una animación
        void ManageRotation(string lockState); // Método para bloquear y desbloquear la rotación
        void CanInterrupt(); // Método para permitir interrupción
    }

    // Métodos relacionados con el ataque
    public interface IAttackEvents
    {
        void OnStartAttack(int attackNum); // Método para notificar inicio de un ataque
        void EnableCollider(); // Método para habilitar el collider del arma
        void DisableCollider(); // Método para deshabilitar el collider del arma
    }

    // Métodos relacionados con el esquive
    public interface IRollEvents
    {
        void ManageHitBox(string state); // Método para habilitar y deshabilitar la Hitbox del jugador
        void ManageImpulse(string state); // Método para habilitar o deshabilitar el impulso del esquive
    }
}