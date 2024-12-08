using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimationEvents : MonoBehaviour
{
    [Header("External References")]
    [SerializeField] PlayerAnimations animations; // Script de control de animaciones
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

    void EndAttackAnimation() // Notificar que se ha acabado la animación de ataque
    {
        animations.EndAttackAnimation();
    }
}
