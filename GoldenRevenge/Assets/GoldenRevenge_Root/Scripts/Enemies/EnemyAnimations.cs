using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimations : MonoBehaviour
{
    [Header("Components")]
    EnemyMovement move; // Script de control de movimiento
    EnemyCombat combat; // Script de control de combate
    [SerializeField] Animator anim;

    // Valores condicionales
    string currentAnim; // Animación que activamos
    string prevAnim; // Animación previa

    void Start()
    {
        // Obtenemos componentes
        move = GetComponent<EnemyMovement>();
        combat = GetComponent<EnemyCombat>();
    }

    void Update()
    {
        // Manejo de animaciones de movimiento
        MoveAnimations();
    }

    #region MoveAnimationsLogic

    // Manejo de animaciones de movimiento
    void MoveAnimations()
    {
        // Si está atacando no hay movimiento
        if (combat.isAttacking)
        {
            return;
        }

        // Según el estado activamos animación de caminar o trotar

        if (move.isPatrolling) // Animación de caminado
        {
            currentAnim = "walk";
        }
        else if (move.isChasing) // Animación de trote
        {
            currentAnim = "run";
        }
        else // Animación de Iddle
        {
            currentAnim = null;
        }

        // Se cambia animación (Primer valor: animación para reproducir, Segundo valor: animación que interrumpir)
        UpdateAnimationState();
    }

    #endregion

    #region CombatAnimationsLogic

    // Animación de ataque en marcha
    public void AttackAnimations(int attackNum)
    {
        // Activamos animación de ataque correspondiente a su número
        currentAnim = "attack" + attackNum;
        UpdateAnimationState();
    }

    #endregion

    // Método para activar cualquier animación
    void UpdateAnimationState()
    {
        // Activar animación seleccionada solo si cambia el estado
        if ((currentAnim != prevAnim) || (prevAnim == null))
        {
            // Activamos animación actual
            if (currentAnim != null) anim.SetBool(currentAnim, true);

            // Desactivamos animación anterior
            if (prevAnim != null) anim.SetBool(prevAnim, false);
            prevAnim = currentAnim; // Indicamos cual será la animación previa en la próxima transición
        }
    }
}
