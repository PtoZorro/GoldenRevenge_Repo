using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    [Header("Components")]
    PlayerMovement move;
    [SerializeField] Animator anim;

    [Header("Conditional Values")]
    bool iddle;
    bool walk;
    bool walkBack;
    bool walkRight;
    bool walkLeft;
    bool run;

    [Header("Stats")]
    [SerializeField] float minSpeedToRunAnim;
    [SerializeField] float walkAnimMinSpeed;

    void Start()
    {
        // Obtenemos componentes
        move = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        // Manejo de animaciones
        HandleAnimations();
    }

    void HandleAnimations()
    {
        // Obtener la magnitud del input de movimiento
        float inputMagnitude = move.moveInput.magnitude;

        // Según la velocidad activamos animación de caminar o trotar
        if (inputMagnitude > 0 && inputMagnitude < minSpeedToRunAnim)
        {
            walk = true;
            run = false;
            anim.SetFloat("walkSpeed", Mathf.Lerp(walkAnimMinSpeed, 1f, inputMagnitude / minSpeedToRunAnim)); // Velocidad de animación ajustada
        }
        else if (inputMagnitude >= minSpeedToRunAnim)
        {
            walk = false;
            run = true;
            anim.SetFloat("walkSpeed", 1f); // Velocidad estándar para trotar/correr
        }
        else
        {
            walk = false;
            run = false;
            anim.SetFloat("walkSpeed", 0f); // Detener la animación
        }

        // Activamos la animación correspondiente
        anim.SetBool("walk", walk);
        anim.SetBool("run", run);
    }
}
