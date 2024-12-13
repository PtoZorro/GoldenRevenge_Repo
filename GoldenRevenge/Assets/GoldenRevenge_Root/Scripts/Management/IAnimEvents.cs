using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICombatEvents
{
    void OnStartAttack(int attackNum); // M�todo para notificar inicio de un ataque
    void EnableCollider(); // M�todo para habilitar el collider
    void DisableCollider(); // M�todo para deshabilitar el collider
    void LockRotation(); // M�todo para bloquear la rotaci�n
    void CanInterrupt(); // M�todo para permitir interrupci�n
    void OnEndAttack(); // M�todo para notificar el fin de un ataque
}