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
    [SerializeField] float minSpeedToRunAnim; // Velocidad m�nima a la que el personaje empezar� a trotar
    [SerializeField] float walkAnimMinSpeed; // Velocidad m�nima a la que la animaci�n de caminar se reproduce
    [SerializeField] float smoothBlendMove; // Velocidad a la que transiciona entre los distintos caminados en el Blend Tree

    // Valores condicionales
    string currentAnim; // Animaci�n que activamos
    string prevAnim; // Animaci�n previa
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

        // Si la c�mara est� bloqueada se calcula el lado hacia el que caminamos
        LockedMoveValues();
    }

    void MoveAnimations()
    {
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

        // Se cambia animaci�n (Primer valor: animaci�n para reproducir, Segundo valor: animaci�n que interrumpir)
        UpdateAnimationState(currentAnim, prevAnim);
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

    void UpdateAnimationState(string animToSet, string previousAnim)
    {
        // Activar animaci�n seleccionada solo si cambia el estado
        if ((animToSet != previousAnim) || (previousAnim == null))
        {
            // Activamos animaci�n actual
            if (animToSet != null) anim.SetBool(animToSet, true);

            // Desactivamos animaci�n anterior
            if (previousAnim != null) anim.SetBool(previousAnim, false);
            prevAnim = currentAnim; // Indicamos cual ser� la animaci�n previa en la pr�xima transici�n
        }
    }
}
