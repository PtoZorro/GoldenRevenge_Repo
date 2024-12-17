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
    string currentAnim; // Animaci�n que activamos
    string prevAnim; // Animaci�n previa

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
        // Si est� atacando no hay movimiento
        if (combat.isAttacking)
        {
            return;
        }

        // Seg�n el estado activamos animaci�n de caminar o trotar

        if (move.isPatrolling) // Animaci�n de caminado
        {
            currentAnim = "walk";
        }
        else if (move.isChasing) // Animaci�n de trote
        {
            currentAnim = "run";
        }
        else // Animaci�n de Iddle
        {
            currentAnim = null;
        }

        // Se cambia animaci�n (Primer valor: animaci�n para reproducir, Segundo valor: animaci�n que interrumpir)
        UpdateAnimationState();
    }

    #endregion

    #region CombatAnimationsLogic

    // Animaci�n de ataque en marcha
    public void AttackAnimations(int attackNum)
    {
        // Activamos animaci�n de ataque correspondiente a su n�mero
        currentAnim = "attack" + attackNum;
        UpdateAnimationState();
    }

    #endregion

    // M�todo para activar cualquier animaci�n
    void UpdateAnimationState()
    {
        // Activar animaci�n seleccionada solo si cambia el estado
        if ((currentAnim != prevAnim) || (prevAnim == null))
        {
            // Activamos animaci�n actual
            if (currentAnim != null) anim.SetBool(currentAnim, true);

            // Desactivamos animaci�n anterior
            if (prevAnim != null) anim.SetBool(prevAnim, false);
            prevAnim = currentAnim; // Indicamos cual ser� la animaci�n previa en la pr�xima transici�n
        }
    }
}
