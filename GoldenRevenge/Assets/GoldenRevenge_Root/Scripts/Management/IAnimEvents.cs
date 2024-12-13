using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICombatEvents
{
    void OnStartAttack(int attackNum); // Método para notificar inicio de un ataque
    void EnableCollider(); // Método para habilitar el collider
    void DisableCollider(); // Método para deshabilitar el collider
    void LockRotation(); // Método para bloquear la rotación
    void CanInterrupt(); // Método para permitir interrupción
    void OnEndAttack(); // Método para notificar el fin de un ataque
}