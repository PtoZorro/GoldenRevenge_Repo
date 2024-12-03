using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Components")]
    Rigidbody rb;

    [Header("External Components")]
    [SerializeField] CameraBehaviour cam; // Script de control de c�mara
    [SerializeField] Transform cameraPos; // Posici�n de la c�mara

    [Header("Movement Stats")]
    [SerializeField] float speed; // Valor para controlar la velocidad
    [SerializeField] float rotationSmooth; // Suavizado de la rotaci�n del personaje
    [SerializeField] float lockSpeed; // Velocidad a la cual el jugador apuntar� hacia el enemigo

    [Header("Input values")]
    public Vector2 moveInput; // Input de movimiento
    Vector3 desiredMoveDirection; // Direcci�n a la que el jugador se mover� y rotar�

    void Start()
    {
        // Obtenemos las referencias necesarias
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Movimiento del personaje
        HandleMovement();

        // Rotaci�n del personaje
        if (!cam.camLocked) { HandleRotation(); }
        // Mirar hacia el enemigo
        else { FaceEnemy(); }
    }

    void HandleMovement()
    {
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
        Vector3 inputFixed = new Vector3(moveInput.x, 0, moveInput.y);

        // Convertimos el input en la direcci�n del mundo usando la direcci�n de la c�mara
        desiredMoveDirection = forward * inputFixed.z + right * inputFixed.x;

        // Calculamos la magnitud del input para ajustar la velocidad seg�n la intensidad del joystick
        float inputMagnitude = inputFixed.magnitude;

        // Aplicamos la direcci�n deseada y la velocidad al Rigidbody utilizando la velocidad
        Vector3 targetVelocity = desiredMoveDirection.normalized * speed * inputMagnitude;

        // Suavizamos la transici�n de velocidad para evitar movimientos bruscos
        rb.velocity = Vector3.Lerp(rb.velocity, targetVelocity, Time.fixedDeltaTime * 10f);

        // Evitar que se acumule velocidad en el eje Y
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
    }


    void HandleRotation()
    {
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

    void FaceEnemy()
    {
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

    public void OnMove(InputAction.CallbackContext context)
    {
        // Lectura de input de movimiento
        moveInput = context.ReadValue<Vector2>();
    }
}
