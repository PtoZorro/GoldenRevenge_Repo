using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    [Header("Components")]
    PlayerMovement move;
    [SerializeField] CameraBehaviour cam;
    [SerializeField] Animator anim;

    [Header("Stats")]
    [SerializeField] float minSpeedToRunAnim; // Velocidad mínima a la que el personaje empezará a trotar
    [SerializeField] float walkAnimMinSpeed; // Velocidad mínima a la que la animación de caminar se reproduce
    [SerializeField] float smoothBlendMove; // Velocidad a la que transiciona entre los distintos caminados en el Blend Tree

    // Valores condicionales
    string currentAnim; // Animación que activamos
    string prevAnim; // Animación previa
    float currentXSpeed;
    float currentYSpeed;

    void Start()
    {
        // Obtenemos componentes
        move = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        // Manejos de animaciones
        MoveAnimations();

        // Si la cámara está bloqueada se calcula el lado hacia el que caminamos
        LockedMoveValues();
    }

    void MoveAnimations()
    {
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

        // Se cambia animación (Primer valor: animación para reproducir, Segundo valor: animación que interrumpir)
        UpdateAnimationState(currentAnim, prevAnim);
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

    void UpdateAnimationState(string animToSet, string previousAnim)
    {
        // Activar animación seleccionada solo si cambia el estado
        if ((animToSet != previousAnim) || (previousAnim == null))
        {
            // Activamos animación actual
            if (animToSet != null) anim.SetBool(animToSet, true);

            // Desactivamos animación anterior
            if (previousAnim != null) anim.SetBool(previousAnim, false);
            prevAnim = currentAnim; // Indicamos cual será la animación previa en la próxima transición
        }
    }
}
