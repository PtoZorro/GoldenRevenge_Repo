using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    // Components
    Rigidbody rb;

    [Header("References")]
    [SerializeField] CameraBehaviour cam; // Script de control de cámara
    PlayerCombat combat; // Script de control de combate
    [SerializeField] Transform cameraPos; // Posición de la cámara

    [Header("Movement Stats")]
    [SerializeField] float speed; // Valor para controlar la velocidad
    [SerializeField] float rotationSmooth; // Suavizado de la rotación del personaje
    [SerializeField] float rollRotationSmooth; // Suavizado de la rotación del personaje al inicio del esquive
    [SerializeField] float rollForce; // Velocidad a la cual el jugador esquiva
    [SerializeField] float rollSmooth; // Velocidad a la cual el jugador esquiva
    [SerializeField] float lockSpeed; // Velocidad a la cual el jugador apuntará hacia el enemigo

    [Header("Conditional Values")]
    public bool moveLocked; // Negación del movimiento
    public bool rotationLocked; // Negación de rotación
    public bool markEnemyLocked; // Negación de encarar hacia enemigo
    bool initImpulse; // Indicador de impulso incial para el esquive

    [Header("Input values")]
    public Vector2 moveInput; // Input de movimiento
    Vector3 inputFixed; // Vector corregido del input en base al movimiento en mundo (x, y, 0) >> (x, 0 ,y)
    Vector3 desiredMoveDirection; // Dirección a la que el jugador se moverá y rotará

    void Start()
    {
        // Obtenemos componentes
        rb = GetComponent<Rigidbody>();
        combat = GetComponent<PlayerCombat>();
    }

    void FixedUpdate()
    {
        // Dirección a la que nos movemos y rotamos
        CalculeDirection();

        // Movimiento del personaje
        HandleMovement();

        // Rotación del personaje 
        HandleRotation();

        // Mirar hacia el enemigo cuando lo hemos marcado
        FaceEnemy(); 

        // Esquive cuando el input lo marca
        Roll();
    }

    #region MovementHandling

    // Dirección a la que nos movemos y rotamos
    void CalculeDirection() 
    {
        // Obtener la dirección hacia adelante y hacia la derecha de la cámara
        Vector3 forward = cameraPos.forward;
        Vector3 right = cameraPos.right;

        // Asegurarse de que la dirección hacia adelante y derecha esté en el plano horizontal
        forward.y = 0f;
        right.y = 0f;

        // Normalizar los vectores para que no se vean afectados por la inclinación de la cámara
        forward.Normalize();
        right.Normalize();

        // Ajustamos el input ya que queremos movernos en ejes X y Z
        inputFixed = new Vector3(moveInput.x, 0, moveInput.y);

        // Convertimos el input en la dirección del mundo usando la dirección de la cámara
        desiredMoveDirection = forward * inputFixed.z + right * inputFixed.x;
    }

    // Movimiento del personaje
    void HandleMovement()
    {
        // Si no se nos permite movernos
        if (moveLocked) return;

        // Calculamos la magnitud del input para ajustar la velocidad según la intensidad del joystick
        float inputMagnitude = inputFixed.magnitude;

        // Aplicamos la dirección deseada y la velocidad al Rigidbody utilizando la velocidad
        Vector3 targetVelocity = desiredMoveDirection.normalized * speed * inputMagnitude;

        // Conservamos la velocidad normal de caida en eje Y
        targetVelocity.y = rb.velocity.y;

        // Suavizamos la transición de velocidad para evitar movimientos bruscos
        rb.velocity = Vector3.Lerp(rb.velocity, targetVelocity, Time.fixedDeltaTime * 10f);
    }

    // Rotación del personaje
    void HandleRotation()
    {
        // Si no se nos permite rotar
        if (rotationLocked) return;

        // Valor de la velocidad a la que rota el Jugador
        float smooth;

        // Aun que la cámara esté bloqueada, el esquive se hace hacia cualquier dirección y se rota a una velocidad distinta
        if (!combat.isRolling)
        {
            //Si la cámara está bloqueada no ejecutamos, a no ser que que estemos esquivando
            if (cam.camLocked) return;

            // Velocidad de rotación normal
            smooth = rotationSmooth;
        }
        else smooth = rollRotationSmooth; // Velocidad de rotación al principio del esquive
        
        if (moveInput != Vector2.zero)
        {
            // Calcular la rotación hacia la dirección en la que queremos movernos
            Quaternion targetRotation = Quaternion.LookRotation(desiredMoveDirection);

            // Interpola suavemente hacia la rotación deseada usando Slerp para una rotación más suave
            Quaternion smoothedRotation = Quaternion.Slerp(rb.rotation, targetRotation, smooth * Time.fixedDeltaTime);

            // Aplica la rotación al Rigidbody
            rb.MoveRotation(smoothedRotation);
        }
    }

    // Detener las fuerzas de movimiento residual
    public void CancelResidualMove()
    {
        rb.velocity = Vector3.zero;
    }
    
    // Detener las fuerzas de rotación residual
    public void CancelResidualRot()
    {
        rb.angularVelocity = Vector3.zero;
    }

    #endregion

    #region Movement/EnemyInteraction

    void FaceEnemy()
    {
        // Solo encaramos al enemigo cuando lo hemos fijado
        // Si no se nos permite encarar al enemigo no ejecutamos
        if (!cam.camLocked || markEnemyLocked) return;

        // Calculamos la dirección hacia el enemigo
        Vector3 directionToEnemy = cam.enemyLocked.transform.position - transform.position;

        // Mantener solo la dirección en el plano X-Z, eliminando cualquier inclinación en Y
        directionToEnemy.y = 0;

        // Calculamos la rotación deseada solo en el eje Y
        Quaternion targetRotation = Quaternion.LookRotation(directionToEnemy);

        // Mantenemos la rotación suavizada solo en el eje Y
        Quaternion currentRotation = rb.rotation;
        Quaternion smoothedRotation = Quaternion.Slerp(currentRotation, targetRotation, rotationSmooth * Time.fixedDeltaTime);

        // Aplicar solo la rotación en el eje Y (manteniendo las otras componentes intactas)
        rb.MoveRotation(Quaternion.Euler(0, smoothedRotation.eulerAngles.y, 0));
    }

    #endregion

    #region CombatMovementMechanics

    void Roll()
    {
        // Solo se ejecuta si estamos en estado de Roll
        if (!combat.isRolling || !combat.rollImpulse)
        {
            // Reiniciamos el indicador de impulso inicial
            initImpulse = false;

            return;
        }

        // Obtener la dirección hacia adelante basada en la orientación del personaje
        Vector3 rollDirection = transform.forward;

        // Si el jugador acaba de iniciar el roll, aplicar la fuerza inicial
        if (!initImpulse)
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0); // Limpiar velocidad previa
            rb.AddForce(rollDirection * rollForce, ForceMode.Impulse); // Impulso inicial
            combat.rollImpulse = false; // Marcar que ya se aplicó el impulso

            // Impulso inicial realizado
            initImpulse = true;
        }

        // Reducir progresivamente la velocidad del roll
        Vector3 currentVelocity = rb.velocity;
        Vector3 reducedVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, rollSmooth * Time.fixedDeltaTime);

        // Aplicar la velocidad suavizada, manteniendo la componente Y para la gravedad
        rb.velocity = new Vector3(reducedVelocity.x, currentVelocity.y, reducedVelocity.z);
    }

    #endregion

    #region InputReading

    // Lectura de input de movimiento
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    #endregion
}
