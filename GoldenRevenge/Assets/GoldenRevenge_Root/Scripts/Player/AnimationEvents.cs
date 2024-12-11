using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimationEvents : MonoBehaviour
{
    // Script exclusivo para controlar eventos de la animación

    [Header("External References")]
    [SerializeField] PlayerCombat combat; // Script de control de combate

    // Notificar que ha comenzado la animación de ataque y avisar de que ataque se está ejecutando
    void StartAttackAnimation(int attackNum) 
    {
        combat.OnStartAttack(attackNum);
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
