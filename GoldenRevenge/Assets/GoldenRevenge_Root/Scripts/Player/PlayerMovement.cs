using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    // Components
    Rigidbody rb;

    [Header("References")]
    [SerializeField] CameraBehaviour cam; // Script de control de c�mara
    PlayerCombat combat; // Script de control de comabte
    [SerializeField] Transform cameraPos; // Posici�n de la c�mara

    [Header("Movement Stats")]
    [SerializeField] float speed; // Valor para controlar la velocidad
    [SerializeField] float rotationSmooth; // Suavizado de la rotaci�n del personaje
    [SerializeField] float lockSpeed; // Velocidad a la cual el jugador apuntar� hacia el enemigo

    [Header("Input values")]
    public Vector2 moveInput; // Input de movimiento
    Vector3 inputFixed; // Vector corregido del input en base al movimiento en mundo (x, y, 0) >> (x, 0 ,y)
    Vector3 desiredMoveDirection; // Direcci�n a la que el jugador se mover� y rotar�

    void Start()
    {
        // Obtenemos componentes
        rb = GetComponent<Rigidbody>();
        combat = GetComponent<PlayerCombat>();
    }

    void FixedUpdate()
    {
        // Direcci�n a la que nos movemos y rotamos
        CalculeDirection();

        // Movimiento del personaje
        HandleMovement();

        // Rotaci�n del personaje en modo normal
        if (!cam.camLocked) { HandleRotation(); }
        else { FaceEnemy(); } // Mirar hacia el enemigo cuando lo hemos marcado
    }

    #region MovementHandling

    // Direcci�n a la que nos movemos y rotamos
    void CalculeDirection() 
    {
        // Si estamos atacando no hace falta calcular
        if (combat.rotationLocked)
        {
            // No ejecutamos el m�todo
            return;
        }

        // Obtener la direcci�n hacia adelante y hacia la derecha de la c�mara
        Vector3 forward = cameraPos.forward;
        Vector3 right = cameraPos.right;

        // Asegurarse de que la direcci�n hacia adelante y derecha est� en el plano horizontal
        forward.y = 0f;
        right.y = 0f;

        // Normalizar los vectores para que no se vean afectados por la inclinaci�n de la c�mara
        forward.Normalize();
        right.Normalize();

        // Ajustamos el input ya que queremos movernos en ejes X y Z
        inputFixed = new Vector3(moveInput.x, 0, moveInput.y);

        // Convertimos el input en la direcci�n del mundo usando la direcci�n de la c�mara
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
            
            // No ejecutamos el m�todo
            return;
        }

        // Calculamos la magnitud del input para ajustar la velocidad seg�n la intensidad del joystick
        float inputMagnitude = inputFixed.magnitude;

        // Aplicamos la direcci�n deseada y la velocidad al Rigidbody utilizando la velocidad
        Vector3 targetVelocity = desiredMoveDirection.normalized * speed * inputMagnitude;

        // Conservamos la velocidad normal de caida en eje Y
        targetVelocity.y = rb.velocity.y;

        // Suavizamos la transici�n de velocidad para evitar movimientos bruscos
        rb.velocity = Vector3.Lerp(rb.velocity, targetVelocity, Time.fixedDeltaTime * 10f);
    }

    // Rotaci�n del personaje
    void HandleRotation()
    {
        // Si estamos atacando no siempre podemos rotar
        if (combat.rotationLocked)
        {
            // Detenemos la rotaci�n residual
            rb.angularVelocity = Vector3.zero; 

            // No ejecutamos el m�todo
            return;
        }

        if (moveInput != Vector2.zero)
        {
            // Calcular la rotaci�n hacia la direcci�n en la que queremos movernos
            Quaternion targetRotation = Quaternion.LookRotation(desiredMoveDirection);

            // Interpola suavemente hacia la rotaci�n deseada usando Slerp para una rotaci�n m�s suave
            Quaternion smoothedRotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSmooth * Time.fixedDeltaTime);

            // Aplica la rotaci�n al Rigidbody
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
            // No ejecutamos el m�todo
            return;
        }

        // Calculamos la direcci�n hacia el enemigo
        Vector3 directionToEnemy = cam.enemyLocked.transform.position - transform.position;

        // Mantener solo la direcci�n en el plano X-Z, eliminando cualquier inclinaci�n en Y
        directionToEnemy.y = 0;

        // Calculamos la rotaci�n deseada solo en el eje Y
        Quaternion targetRotation = Quaternion.LookRotation(directionToEnemy);

        // Mantenemos la rotaci�n suavizada solo en el eje Y
        Quaternion currentRotation = rb.rotation;
        Quaternion smoothedRotation = Quaternion.Slerp(currentRotation, targetRotation, rotationSmooth * Time.fixedDeltaTime);

        // Aplicar solo la rotaci�n en el eje Y (manteniendo las otras componentes intactas)
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
