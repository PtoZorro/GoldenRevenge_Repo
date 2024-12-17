using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraCollision : MonoBehaviour
{
    [Header("Values")]
    [SerializeField] float minDistance; // Distancia mínima a la que puede estar la cámara
    [SerializeField] float maxDistance; // Distancia máxima de la cámara
    [SerializeField] float distanceFromHit; // Distancia a la que se aleja la cámara respecto al obstáculo
    [SerializeField] float collisionOffset; // Desfase adicional para prevenir colisiones
    [SerializeField] float smooth; // Suavizado del movimiento de la cámara

    // Variables para controlar las esquinas de la cámara
    [SerializeField] float cornerOffsetX; // Desfase en el eje X para las esquinas
    [SerializeField] float cornerOffsetY; // Desfase en el eje Y para las esquinas

    private Vector3 cameraDir; // Dirección inicial de la cámara
    private float currentDistance; // Distancia actual de la cámara

    // Variables para detectar input
    Vector2 lookInput; // Input de cámara
    Vector2 moveInput; // Input de movimiento

    void Awake()
    {
        cameraDir = transform.localPosition.normalized; // Dirección inicial normalizada
        currentDistance = maxDistance; // La cámara empieza en la distancia máxima
    }

    void LateUpdate()
    {
        // Solo detectamos colisiones cuando leemos input de movimiento o cámara con tal de optimizar
        if (lookInput != Vector2.zero || moveInput != Vector2.zero)
        {
            // Detectar colisiones entre cámara y personaje
            DetectCollisions();
        }
    }

    void DetectCollisions()
    {
        // Inicializar la distancia actual
        currentDistance = maxDistance;

        // Posiciones de las 4 esquinas de la cámara y centro
        Vector3[] corners = new Vector3[5];

        corners[0] = transform.position;  // centro
        corners[1] = transform.position + transform.right * cornerOffsetX + transform.up * cornerOffsetY;  // esquina superior derecha
        corners[2] = transform.position + transform.right * cornerOffsetX - transform.up * cornerOffsetY;  // esquina inferior derecha
        corners[3] = transform.position - transform.right * cornerOffsetX + transform.up * cornerOffsetY;  // esquina superior izquierda
        corners[4] = transform.position - transform.right * cornerOffsetX - transform.up * cornerOffsetY;  // esquina inferior izquierda

        // Inicializar la distancia más cercana detectada con la distancia máxima
        float nearestDistance = maxDistance;

        // Realizar 4 raycasts para detectar las colisiones en cada esquina
        foreach (Vector3 corner in corners)
        {
            // Dibujar los rayos en la escena para depuración
            Debug.DrawRay(transform.parent.position, corner - transform.parent.position, Color.green);

            // Detectar colisiones para cada esquina
            RaycastHit hit;
            if (Physics.Linecast(transform.parent.position, corner + (corner - transform.parent.position).normalized * (maxDistance + collisionOffset), out hit))
            {
                // Si se detecta una colisión, actualizar la distancia más cercana
                float adjustedDistance = hit.distance - distanceFromHit;
                nearestDistance = Mathf.Min(nearestDistance, adjustedDistance);
            }
        }

        // Ajustar la distancia final de la cámara asegurando que esté dentro de los límites definidos
        currentDistance = Mathf.Clamp(nearestDistance, minDistance, maxDistance);

        // Calcular la posición final deseada de la cámara
        Vector3 finalPosition = cameraDir * currentDistance;

        // Aplicar el movimiento suavizado a la cámara
        transform.localPosition = Vector3.Lerp(transform.localPosition, finalPosition, smooth * Time.deltaTime);
    }

    // Método del Input System para recibir el movimiento de la cámara
    public void OnLook(InputAction.CallbackContext context)
    {
        // Leer input de cámara
        lookInput = context.ReadValue<Vector2>();
    }

    // Método del Input System para recibir el movimiento del personaje
    public void OnMove(InputAction.CallbackContext context)
    {
        // Lectura de input de movimiento
        moveInput = context.ReadValue<Vector2>();
    }
}



