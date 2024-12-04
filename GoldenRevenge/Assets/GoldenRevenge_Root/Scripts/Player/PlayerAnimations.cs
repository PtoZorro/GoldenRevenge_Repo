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

    [Header("Conditional Values")]
    string currentAnim; // Animaci�n que activamos
    string prevAnim; // Animaci�n previa

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

        // Seg�n la velocidad activamos animaci�n de caminar o trotar
        if (inputMagnitude > 0 && inputMagnitude < minSpeedToRunAnim)
        {
            currentAnim = "walk";

            // Velocidad de la animaci�n seg�n la velocidad de movimiento
            anim.SetFloat("walkSpeed", Mathf.Lerp(walkAnimMinSpeed, 1f, inputMagnitude / minSpeedToRunAnim));
        }
        else if (inputMagnitude >= minSpeedToRunAnim) // Animaci�n de trote
        {
            // Si no hay fijaci�n en el enemigo nos movemos hacia delante
            if (!cam.camLocked)
            {
                currentAnim = "run";
            }
            else // Si hay fijaci�n en el enemigo comprobamos hacia que lado caminamos
            {
                // Valor absoluto de movimiento en cada eje
                float absMoveX = Mathf.Abs(moveX);
                float absMoveY = Mathf.Abs(moveY);

                // Si el movimiento horizontal tiene m�s peso
                if (absMoveX > absMoveY)
                {
                    currentAnim = moveX > 0 ? "runRight" : "runLeft";
                }
                else // Si el movimiento vertical tiene m�s peso
                {
                    currentAnim = moveY > 0 ? "run" : "runBack";
                }
            }

            // Velocidad de la animaci�n seg�n la velocidad de movimiento
            anim.SetFloat("walkSpeed", 1f);
        }
        else // Animaci�n de Iddle
        {
            currentAnim = "iddle";
            anim.SetFloat("walkSpeed", 0f);
        }

        // Activar animaci�n seleccionada solo si cambia el estado
        if ((currentAnim != prevAnim) || (prevAnim == null))
        {
            UpdateAnimationState();
        }
    }

    void UpdateAnimationState()
    {
        // Activamos animaci�n actual
        anim.SetBool(currentAnim, true);

        // Desactivamos animaci�n anterior
        if (prevAnim != null) anim.SetBool(prevAnim, false);
        prevAnim = currentAnim; // Indicamos cual ser� la animaci�n previo en la pr�xima transici�n
    }
}
