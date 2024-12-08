using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    [Header("Components")]
    PlayerMovement move; // Script de control de movimiento
    PlayerCombat combat; // Script de control de combate
    [SerializeField] CameraBehaviour cam; // Script de control de cámara
    [SerializeField] Animator anim;

    [Header("Stats")]
    [SerializeField] float minSpeedToRunAnim; // Velocidad mínima a la que el personaje empezará a trotar
    [SerializeField] float walkAnimMinSpeed; // Velocidad mínima a la que la animación de caminar se reproduce
    [SerializeField] float smoothBlendMove; // Velocidad a la que transiciona entre los distintos caminados en el Blend Tree

    // Valores condicionales
    string currentAnim; // Animación que activamos
    string prevAnim; // Animación previa
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

        // Según la velocidad activamos animación de caminar o trotar
        if (inputMagnitude > 0 && inputMagnitude < minSpeedToRunAnim)
        {
            currentAnim = "walk";

            // Velocidad de la animación según la velocidad de movimiento
            anim.SetFloat("walkSpeed", Mathf.Lerp(walkAnimMinSpeed, 1f, inputMagnitude / minSpeedToRunAnim));
        }
        else if (inputMagnitude >= minSpeedToRunAnim) // Animación de trote
        {
            currentAnim = "run";

            // Velocidad de la animación según la velocidad de movimiento
            anim.SetFloat("walkSpeed", 1f);
        }
        else // Animación de Iddle
        {
            currentAnim = null;
        }

        // Si la cámara está bloqueada se calcula el lado hacia el que caminamos
        LockedMoveValues();

        // Se cambia animación (Primer valor: animación para reproducir, Segundo valor: animación que interrumpir)
        UpdateAnimationState();
    }

    void LockedMoveValues()
    {
        if (currentAnim == "run")
        {
            // Input instantáneo del teclado
            float targetXSpeed = move.moveInput.x;
            float targetYSpeed = move.moveInput.y;

            // Interpolación suave de la velocidad (De esta manera evitamos que no haya interpolación al no usar input analógico)
            currentXSpeed = Mathf.Lerp(currentXSpeed, targetXSpeed, Time.deltaTime / smoothBlendMove);
            currentYSpeed = Mathf.Lerp(currentYSpeed, targetYSpeed, Time.deltaTime / smoothBlendMove);

            if (!cam.camLocked) // Si la cámara no está fijada, solo trotamos hacia delante 
            {
                // Aplicar la magnitud de input a las variables del animator
                anim.SetFloat("XSpeed", 0);
                anim.SetFloat("YSpeed", Mathf.Abs(currentYSpeed));
            }
            else // Si la cámara está fijada en enemigo, se contemplan el trote hacia los 4 lados
            {
                // Aplicar la magnitud de input a las variables del animator
                anim.SetFloat("XSpeed", currentXSpeed);
                anim.SetFloat("YSpeed", currentYSpeed);
            }
        }
    }

    public void AttackAnimation() // Animación de ataque en marcha
    {
        // Activamos animación dea ataque
        currentAnim = "attack1";
        UpdateAnimationState();
    }

    public void EndAttackAnimation() // Al acabar la animación de ataque quitamos estado de ataque general
    {
        combat.isAttacking = false; 
        combat.rotationLocked = false; // se libera la rotación de personaje
    }

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
