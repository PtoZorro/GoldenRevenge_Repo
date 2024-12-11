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
    PlayerCombat combat; // Script de control de comabte
    [SerializeField] Transform cameraPos; // Posición de la cámara

    [Header("Movement Stats")]
    [SerializeField] float speed; // Valor para controlar la velocidad
    [SerializeField] float rotationSmooth; // Suavizado de la rotación del personaje
    [SerializeField] float lockSpeed; // Velocidad a la cual el jugador apuntará hacia el enemigo

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

        // Rotación del personaje en modo normal
        if (!cam.camLocked) { HandleRotation(); }
        else { FaceEnemy(); } // Mirar hacia el enemigo cuando lo hemos marcado
    }

    #region MovementHandling

    // Dirección a la que nos movemos y rotamos
    void CalculeDirection() 
    {
        // Si estamos atacando no hace falta calcular
        if (combat.rotationLocked)
        {
            // No ejecutamos el método
            return;
        }

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
        // Si estamos atacando no hay movimiento
        if (combat.isAttacking)
        {
            // Detenemos las fuerzas de movimiento residual
            rb.velocity = Vector3.zero;
            
            // No ejecutamos el método
            return;
        }

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
        // Si estamos atacando no siempre podemos rotar
        if (combat.rotationLocked)
        {
            // Detenemos la rotación residual
            rb.angularVelocity = Vector3.zero; 

            // No ejecutamos el método
            return;
        }

        if (moveInput != Vector2.zero)
        {
            // Calcular la rotación hacia la dirección en la que queremos movernos
            Quaternion targetRotation = Quaternion.LookRotation(desiredMoveDirection);

            // Interpola suavemente hacia la rotación deseada usando Slerp para una rotación más suave
            Quaternion smoothedRotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSmooth * Time.fixedDeltaTime);

            // Aplica la rotación al Rigidbody
            rb.MoveRotation(smoothedRotation);
        }
    }

    #endregion

    #region Movement/EnemyInteraction

    void FaceEnemy()
    {
        // Si estamos atacando no miramos siempre al enemigo
        if (combat.rotationLocked)
        {
            // No ejecutamos el método
            return;
        }

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

    #region InputReading

    // Lectura de input de movimiento
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    #endregion
}
