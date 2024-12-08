using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    [Header("Components")]
    PlayerMovement move; // Script de control de movimiento
    PlayerCombat combat; // Script de control de combate
    [SerializeField] CameraBehaviour cam; // Script de control de c�mara
    [SerializeField] Animator anim;

    [Header("Stats")]
    [SerializeField] float minSpeedToRunAnim; // Velocidad m�nima a la que el personaje empezar� a trotar
    [SerializeField] float walkAnimMinSpeed; // Velocidad m�nima a la que la animaci�n de caminar se reproduce
    [SerializeField] float smoothBlendMove; // Velocidad a la que transiciona entre los distintos caminados en el Blend Tree

    // Valores condicionales
    string currentAnim; // Animaci�n que activamos
    string prevAnim; // Animaci�n previa
    float currentXSpeed; // Valor de input Horizontal para BlendTree de caminado
    float currentYSpeed; // Valor de input Vertical para BlendTree de caminado

    void Start()
    {
        // Obtenemos componentes
        move = GetComponent<PlayerMovement>();
        combat = GetComponent<PlayerCombat>();
    }

    void Update()
    {
        // Manejos de animaciones
        MoveAnimations();

        // Manejo de animaciones de atque solo si recibe input
        if (combat.isAttacking)
        {
            AttackAnimation();
        }
    }

    void MoveAnimations()
    {
        // Si estamos atacando no hay movimiento
        if (combat.isAttacking)
        {
            return;
        }

        // Obtener la magnitud del input de movimiento
        float inputMagnitude = move.moveInput.magnitude;

        // Seg�n la velocidad activamos animaci�n de caminar o trotar
        if (inputMagnitude > 0 && inputMagnitude < minSpeedToRunAnim)
        {
            currentAnim = "walk";

            // Velocidad de la animaci�n seg�n la velocidad de movimiento
            anim.SetFloat("walkSpeed", Mathf.Lerp(walkAnimMinSpeed, 1f, inputMagnitude / minSpeedToRunAnim));
        }
        else if (inputMagnitude >= minSpeedToRunAnim) // Animaci�n de trote
        {
            currentAnim = "run";

            // Velocidad de la animaci�n seg�n la velocidad de movimiento
            anim.SetFloat("walkSpeed", 1f);
        }
        else // Animaci�n de Iddle
        {
            currentAnim = null;
        }

        // Si la c�mara est� bloqueada se calcula el lado hacia el que caminamos
        LockedMoveValues();

        // Se cambia animaci�n (Primer valor: animaci�n para reproducir, Segundo valor: animaci�n que interrumpir)
        UpdateAnimationState();
    }

    void LockedMoveValues()
    {
        if (currentAnim == "run")
        {
            // Input instant�neo del teclado
            float targetXSpeed = move.moveInput.x;
            float targetYSpeed = move.moveInput.y;

            // Interpolaci�n suave de la velocidad (De esta manera evitamos que no haya interpolaci�n al no usar input anal�gico)
            currentXSpeed = Mathf.Lerp(currentXSpeed, targetXSpeed, Time.deltaTime / smoothBlendMove);
            currentYSpeed = Mathf.Lerp(currentYSpeed, targetYSpeed, Time.deltaTime / smoothBlendMove);

            if (!cam.camLocked) // Si la c�mara no est� fijada, solo trotamos hacia delante 
            {
                // Aplicar la magnitud de input a las variables del animator
                anim.SetFloat("XSpeed", 0);
                anim.SetFloat("YSpeed", Mathf.Abs(currentYSpeed));
            }
            else // Si la c�mara est� fijada en enemigo, se contemplan el trote hacia los 4 lados
            {
                // Aplicar la magnitud de input a las variables del animator
                anim.SetFloat("XSpeed", currentXSpeed);
                anim.SetFloat("YSpeed", currentYSpeed);
            }
        }
    }

    public void AttackAnimation() // Animaci�n de ataque en marcha
    {
        // Activamos animaci�n dea ataque
        currentAnim = "attack1";
        UpdateAnimationState();
    }

    public void EndAttackAnimation() // Al acabar la animaci�n de ataque quitamos estado de ataque general
    {
        combat.isAttacking = false; 
        combat.rotationLocked = false; // se libera la rotaci�n de personaje
    }

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
