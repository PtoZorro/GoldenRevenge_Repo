using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAnimationEvents
{
    // Métodos generales de animación
    public interface IGeneralStatesEvents
    {
        void ManageMovement(string lockState); // Método para bloquear y desbloquear estados de movimiento, rotación y fijado de objetivos
        void CanInterrupt(string canInterrupt); // Método para permitir interrupción
        void OnEndAnimation(string animName); // Método para notificar el fin de una animación
    }

    // Métodos relacionados con el ataque
    public interface IAttackEvents
    {
        void AllowAttack(); // Método para permitir atacar
        void EnableCollider(); // Método para habilitar el collider del arma
        void DisableCollider(); // Método para deshabilitar el collider del arma
    }

    // Métodos relacionados con el esquive
    public interface IRollEvents
    {
        void ManageHitBox(string state); // Método para habilitar y deshabilitar la Hitbox del jugador
        void ManageImpulse(string state); // Método para habilitar o deshabilitar el impulso del esquive
    }
    
    // Métodos relacionados con la curación
    public interface IHealEvents
    {
        void RestoreHealth(); // Método para restablecer la salud
    }
}