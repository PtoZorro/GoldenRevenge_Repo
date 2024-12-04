using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraCollision : MonoBehaviour
{
    [Header("Values")]
    [SerializeField] float minDistance; // Distancia m�nima a la que puede estar la c�mara
    [SerializeField] float maxDistance; // Distancia m�xima de la c�mara
    [SerializeField] float distanceFromHit; // Distancia a la que se aleja la c�mara respecto al obst�culo
    [SerializeField] float collisionOffset; // Desfase adicional para prevenir colisiones
    [SerializeField] float smooth; // Suavizado del movimiento de la c�mara

    // Variables para controlar las esquinas de la c�mara
    [SerializeField] float cornerOffsetX; // Desfase en el eje X para las esquinas
    [SerializeField] float cornerOffsetY; // Desfase en el eje Y para las esquinas

    private Vector3 cameraDir; // Direcci�n inicial de la c�mara
    private float currentDistance; // Distancia actual de la c�mara

    // Variables para detectar input
    Vector2 lookInput; // Input de c�mara
    Vector2 moveInput; // Input de movimiento

    void Awake()
    {
        cameraDir = transform.localPosition.normalized; // Direcci�n inicial normalizada
        currentDistance = maxDistance; // La c�mara empieza en la distancia m�xima
    }

    void LateUpdate()
    {
        // Solo detectamos colisiones cuando leemos input de movimiento o c�mara con tal de optimizar
        if (lookInput != Vector2.zero || moveInput != Vector2.zero)
        {
            // Detectar colisiones entre c�mara y personaje
            DetectCollisions();
        }
    }

    void DetectCollisions()
    {
        // Inicializar la distancia actual
        currentDistance = maxDistance;

        // Posiciones de las 4 esquinas de la c�mara y centro
        Vector3[] corners = new Vector3[5];

        corners[0] = transform.position;  // centro
        corners[1] = transform.position + transform.right * cornerOffsetX + transform.up * cornerOffsetY;  // esquina superior derecha
        corners[2] = transform.position + transform.right * cornerOffsetX - transform.up * cornerOffsetY;  // esquina inferior derecha
        corners[3] = transform.position - transform.right * cornerOffsetX + transform.up * cornerOffsetY;  // esquina superior izquierda
        corners[4] = transform.position - transform.right * cornerOffsetX - transform.up * cornerOffsetY;  // esquina inferior izquierda

        // Inicializar la distancia m�s cercana detectada con la distancia m�xima
        float nearestDistance = maxDistance;

        // Realizar 4 raycasts para detectar las colisiones en cada esquina
        foreach (Vector3 corner in corners)
        {
            // Dibujar los rayos en la escena para depuraci�n
            Debug.DrawRay(transform.parent.position, corner - transform.parent.position, Color.green);

            // Detectar colisiones para cada esquina
            RaycastHit hit;
            if (Physics.Linecast(transform.parent.position, corner + (corner - transform.parent.position).normalized * (maxDistance + collisionOffset), out hit))
            {
                // Si se detecta una colisi�n, actualizar la distancia m�s cercana
                float adjustedDistance = hit.distance - distanceFromHit;
                nearestDistance = Mathf.Min(nearestDistance, adjustedDistance);
            }
        }

        // Ajustar la distancia final de la c�mara asegurando que est� dentro de los l�mites definidos
        currentDistance = Mathf.Clamp(nearestDistance, minDistance, maxDistance);

        // Calcular la posici�n final deseada de la c�mara
        Vector3 finalPosition = cameraDir * currentDistance;

        // Aplicar el movimiento suavizado a la c�mara
        transform.localPosition = Vector3.Lerp(transform.localPosition, finalPosition, smooth * Time.deltaTime);
    }

    // M�todo del Input System para recibir el movimiento de la c�mara
    public void OnLook(InputAction.CallbackContext context)
    {
        // Leer input de c�mara
        lookInput = context.ReadValue<Vector2>();
    }

    // M�todo del Input System para recibir el movimiento del personaje
    public void OnMove(InputAction.CallbackContext context)
    {
        // Lectura de input de movimiento
        moveInput = context.ReadValue<Vector2>();
    }
}



