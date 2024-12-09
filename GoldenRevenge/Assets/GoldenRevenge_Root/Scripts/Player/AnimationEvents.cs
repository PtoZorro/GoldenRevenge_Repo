using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimationEvents : MonoBehaviour
{
    [Header("External References")]
    [SerializeField] PlayerCombat combat; // Script de control de combate

    void EnableCollider() // Habilitar el collider de las armas mediante la animación
    {
        combat.EnableCollider();
    }

    void DisableCollider() // Deshabilitar el collider de las armas mediante la animación
    {
        combat.DisableCollider();
    }

    void LockRotation() // Deshabilita la rotación en el momento de inpacto
    {
        combat.LockRotation();
    }

    void CanReadNextAction() // En cierto punto de la animación se puede leer el input de la siguiente acción
    {
        combat.CanReadNextAction();
    }

    void CanInterrupt() // Llegado a cierto punto de la animación, se puede interrumpir
    {

    }

    void EndAttackAnimation(int attackNum) // Notificar que se ha acabado la animación de ataque
    {
        combat.EndAttack(attackNum);
    }
}
