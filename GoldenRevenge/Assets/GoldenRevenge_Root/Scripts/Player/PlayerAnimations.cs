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

    [Header("Conditional Values")]
    string currentAnim; // Animación que activamos
    string prevAnim; // Animación previa

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

        // Según la velocidad activamos animación de caminar o trotar
        if (inputMagnitude > 0 && inputMagnitude < minSpeedToRunAnim)
        {
            currentAnim = "walk";

            // Velocidad de la animación según la velocidad de movimiento
            anim.SetFloat("walkSpeed", Mathf.Lerp(walkAnimMinSpeed, 1f, inputMagnitude / minSpeedToRunAnim));
        }
        else if (inputMagnitude >= minSpeedToRunAnim) // Animación de trote
        {
            // Si no hay fijación en el enemigo nos movemos hacia delante
            if (!cam.camLocked)
            {
                currentAnim = "run";
            }
            else // Si hay fijación en el enemigo comprobamos hacia que lado caminamos
            {
                // Valor absoluto de movimiento en cada eje
                float absMoveX = Mathf.Abs(moveX);
                float absMoveY = Mathf.Abs(moveY);

                // Si el movimiento horizontal tiene más peso
                if (absMoveX > absMoveY)
                {
                    currentAnim = moveX > 0 ? "runRight" : "runLeft";
                }
                else // Si el movimiento vertical tiene más peso
                {
                    currentAnim = moveY > 0 ? "run" : "runBack";
                }
            }

            // Velocidad de la animación según la velocidad de movimiento
            anim.SetFloat("walkSpeed", 1f);
        }
        else // Animación de Iddle
        {
            currentAnim = "iddle";
            anim.SetFloat("walkSpeed", 0f);
        }

        // Activar animación seleccionada solo si cambia el estado
        if ((currentAnim != prevAnim) || (prevAnim == null))
        {
            UpdateAnimationState();
        }
    }

    void UpdateAnimationState()
    {
        // Activamos animación actual
        anim.SetBool(currentAnim, true);

        // Desactivamos animación anterior
        if (prevAnim != null) anim.SetBool(prevAnim, false);
        prevAnim = currentAnim; // Indicamos cual será la animación previo en la próxima transición
    }
}
