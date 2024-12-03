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
    [SerializeField] float minSpeedToRunAnim;
    [SerializeField] float walkAnimMinSpeed;

    // Estados previos para resetear animaciones
    bool prevIddle = false;
    bool prevWalk = false;
    bool prevRun = false;
    bool prevRunBack = false;
    bool prevRunRight = false;
    bool prevRunLeft = false;

    void Start()
    {
        // Obtenemos componentes
        move = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        // Manejos de animaciones
        HandleAnimations();
    }

    void HandleAnimations()
    {
        // Obtener la magnitud del input de movimiento
        float inputMagnitude = move.moveInput.magnitude;
        float moveX = move.moveInput.x;
        float moveY = move.moveInput.y;

        // Flags de estado de animación actual
        bool iddle = false;
        bool walk = false;
        bool run = false;
        bool runBack = false;
        bool runRight = false;
        bool runLeft = false;

        // Según la velocidad activamos animación de caminar o trotar
        if (inputMagnitude > 0 && inputMagnitude < minSpeedToRunAnim)
        {
            walk = true;

            // Velocidad de la animación según la velocidad de movimiento
            anim.SetFloat("walkSpeed", Mathf.Lerp(walkAnimMinSpeed, 1f, inputMagnitude / minSpeedToRunAnim));
        }
        else if (inputMagnitude >= minSpeedToRunAnim) // Animación de trote
        {
            // Si no hay fijación en el enemigo nos movemos hacia delante
            if (!cam.camLocked)
            {
                run = true;
            }
            else // Si hay fijación en el enemigo comprobamos hacia que lado caminamos
            {
                // Valor absoluto de movimiento en cada eje
                float absMoveX = Mathf.Abs(moveX);
                float absMoveY = Mathf.Abs(moveY);

                // Si el movimiento horizontal tiene más peso
                if (absMoveX > absMoveY)
                {
                    runRight = moveX > 0;
                    runLeft = moveX < 0;
                }
                else // Si el movimiento vertical tiene más peso
                {
                    run = moveY > 0;
                    runBack = moveY < 0;
                }
            }

            // Velocidad de la animación según la velocidad de movimiento
            anim.SetFloat("walkSpeed", 1f);
        }
        else // Animación de Iddle
        {
            iddle = true;
            anim.SetFloat("walkSpeed", 0f);
        }

        // Activar animaciones solo si el estado ha cambiado
        UpdateAnimationState("iddle", iddle, ref prevIddle);
        UpdateAnimationState("walk", walk, ref prevWalk);
        UpdateAnimationState("run", run, ref prevRun);
        UpdateAnimationState("runBack", runBack, ref prevRunBack);
        UpdateAnimationState("runRight", runRight, ref prevRunRight);
        UpdateAnimationState("runLeft", runLeft, ref prevRunLeft);
    }

    void UpdateAnimationState(string parameterName, bool currentState, ref bool previousState)
    {
        if (currentState != previousState)
        {
            anim.SetBool(parameterName, currentState);
            previousState = currentState;
        }
        else { currentState = false; }
    }
}
