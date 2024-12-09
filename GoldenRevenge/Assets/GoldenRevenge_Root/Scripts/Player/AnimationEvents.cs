using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimationEvents : MonoBehaviour
{
    [Header("External References")]
    [SerializeField] PlayerCombat combat; // Script de control de combate

    void EnableCollider() // Habilitar el collider de las armas mediante la animaci�n
    {
        combat.EnableCollider();
    }

    void DisableCollider() // Deshabilitar el collider de las armas mediante la animaci�n
    {
        combat.DisableCollider();
    }

    void LockRotation() // Deshabilita la rotaci�n en el momento de inpacto
    {
        combat.LockRotation();
    }

    void CanReadNextAction() // En cierto punto de la animaci�n se puede leer el input de la siguiente acci�n
    {
        combat.CanReadNextAction();
    }

    void CanInterrupt() // Llegado a cierto punto de la animaci�n, se puede interrumpir
    {

    }

    void EndAttackAnimation(int attackNum) // Notificar que se ha acabado la animaci�n de ataque
    {
        combat.EndAttack(attackNum);
    }
}
