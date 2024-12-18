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
        // Manejo de animaciones de movimiento
        MoveAnimations();
    }

    #region MoveAnimationsLogic

    // Manejo de animaciones de movimiento
    void MoveAnimations()
    {
        // Obtener la magnitud del input de movimiento
        float inputMagnitude = move.moveInput.magnitude;

        // Según la velocidad o si tenemos cámara fijada activamos animación de caminar o trotar

        if (inputMagnitude > 0 && inputMagnitude < minSpeedToRunAnim && !cam.camLocked) // Animación de caminado
        {
            currentAnim = "walk";

            // Velocidad de la animación según la velocidad de movimiento
            anim.SetFloat("walkSpeed", Mathf.Lerp(walkAnimMinSpeed, 1f, inputMagnitude / minSpeedToRunAnim));
        }
        else if (inputMagnitude > 0 && (inputMagnitude >= minSpeedToRunAnim || cam.camLocked)) // Animación de trote
        {
            currentAnim = "run";
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

    // Interpretación del input como dirección hacia la que el personaje corre mediante animación con BlendTree
    void LockedMoveValues()
    {
        if (currentAnim != "run") return;

        // Input instantáneo del teclado
        float targetXSpeed = move.moveInput.x;
        float targetYSpeed = move.moveInput.y;

        // Interpolación suave de la velocidad (De esta manera evitamos que no haya interpolación al no usar input analógico)
        currentXSpeed = Mathf.Lerp(currentXSpeed, targetXSpeed, Time.deltaTime / smoothBlendMove);
        currentYSpeed = Mathf.Lerp(currentYSpeed, targetYSpeed, Time.deltaTime / smoothBlendMove);

        // Si la cámara no está fijada, solo trotamos hacia delante 
        if (!cam.camLocked)
        {
            // La magnitud del imput suavizada
            float speedMagnitude = new Vector2(currentXSpeed, currentYSpeed).magnitude;

            // Aplicar la magnitud de input a las variables del animator
            anim.SetFloat("XSpeed", 0);
            anim.SetFloat("YSpeed", Mathf.Abs(speedMagnitude));
        }
        else // Si la cámara está fijada en enemigo, se contemplan el trote hacia los 4 lados
        {
            // Aplicar la magnitud de input a las variables del animator
            anim.SetFloat("XSpeed", currentXSpeed);
            anim.SetFloat("YSpeed", currentYSpeed);
        }
    }

    #endregion

    #region CombatAnimationsLogic

    // Animación de ataque en marcha
    public void AttackAnimations(int attackNum) 
    {
        // Activamos animación de ataque correspondiente a su número
        anim.SetTrigger("attack" + attackNum);
    }

    // Animación de roll en marcha
    public void RollAnimation()
    {
        anim.SetTrigger("roll");
    }    
    
    // Animación de curación en marcha
    public void HealAnimation()
    {
        anim.SetTrigger("heal");
    }

    // Animación de muerte en marcha
    public void DeathAnimation()
    {
        anim.SetTrigger("death");
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
