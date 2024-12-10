using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimationEvents : MonoBehaviour
{
    [Header("External References")]
    [SerializeField] PlayerCombat combat; // Script de control de combate

    // Notificar que ha comenzado la animación de ataque
    void StartAttackAnimation() 
    {
        combat.OnStartAttack();
    }

    // Habilitar el collider de las armas mediante la animación
    void EnableCollider() 
    {
        combat.EnableCollider();
    }

    // Deshabilitar el collider de las armas mediante la animación
    void DisableCollider() 
    {
        combat.DisableCollider();
    }

    // Deshabilita la rotación en cierto punto de la animación
    void LockRotation() 
    {
        combat.LockRotation();
    }

    // Permite leer el siguiente input en cierto punto de la animación
    void CanInterrupt() 
    {
        combat.CanInterrupt();
    }

    // Notificar que se ha acabado la animación de ataque
    void EndAttackAnimation() 
    {
        combat.OnEndAttack();
    }
}
