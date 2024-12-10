using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimationEvents : MonoBehaviour
{
    [Header("External References")]
    [SerializeField] PlayerCombat combat; // Script de control de combate

    // Notificar que ha comenzado la animaci�n de ataque
    void StartAttackAnimation() 
    {
        combat.OnStartAttack();
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
