using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Components")]
    Rigidbody rb;

    [Header("External Components")]
    [SerializeField] CameraBehaviour cam; // Script de control de cámara
    [SerializeField] Transform cameraPos; // Posición de la cámara

    [Header("Movement Stats")]
    [SerializeField] float speed; // Valor para controlar la velocidad
    [SerializeField] float rotationSmooth; // Suavizado de la rotación del personaje
    [SerializeField] float lockSpeed; // Velocidad a la cual el jugador apuntará hacia el enemigo

    [Header("Input values")]
    public Vector2 moveInput; // Input de movimiento
    Vector3 desiredMoveDirection; // Dirección a la que el jugador se moverá y rotará

    void Start()
    {
        // Obtenemos las referencias necesarias
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Movimiento del personaje
        HandleMovement();

        // Rotación del personaje
        if (!cam.camLocked) { HandleRotation(); }
        // Mirar hacia el enemigo
        else { FaceEnemy(); }
    }

    void HandleMovement()
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
        Vector3 inputFixed = new Vector3(moveInput.x, 0, moveInput.y);

        // Convertimos el input en la dirección del mundo usando la dirección de la cámara
        desiredMoveDirection = forward * inputFixed.z + right * inputFixed.x;

        // Calculamos la magnitud del input para ajustar la velocidad según la intensidad del joystick
        float inputMagnitude = inputFixed.magnitude;

        // Aplicamos la dirección deseada y la velocidad al Rigidbody utilizando la velocidad
        Vector3 targetVelocity = desiredMoveDirection.normalized * speed * inputMagnitude;

        // Suavizamos la transición de velocidad para evitar movimientos bruscos
        rb.velocity = Vector3.Lerp(rb.velocity, targetVelocity, Time.fixedDeltaTime * 10f);

        // Evitar que se acumule velocidad en el eje Y
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
    }


    void HandleRotation()
    {
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

    void FaceEnemy()
    {
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

    public void OnMove(InputAction.CallbackContext context)
    {
        // Lectura de input de movimiento
        moveInput = context.ReadValue<Vector2>();
    }
}
