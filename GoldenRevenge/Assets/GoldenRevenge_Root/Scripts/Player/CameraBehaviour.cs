using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraBehaviour : MonoBehaviour
{
    [Header("External References")]
    [SerializeField] Transform playerPos; 
    [SerializeField] PlayerInput playerInput; 
    [SerializeField] Camera playerCamera; 
    [SerializeField] LayerMask enemyLayer; // Layer en la que detectamos enemigos
    public GameObject enemyLocked; // Enemigo marcado
    GameObject marker; // Marcador que indica el enemigo marcado

    [Header("Stats")]
    [SerializeField] float cameraMoveSpeed; // Velocidad suavizada a la que la cámara sigue al jugador
    [SerializeField] float clampAngle; // Angulo en que limitamos el giro vertical
    [SerializeField] float mouseSensitivity; // Sensibilidad de input de Ratón
    [SerializeField] float stickSensitivity; // Sensibilidad de input de Gamepad
    [SerializeField] float lockEnemyRadius; // Distancia a la que detectamos enemigos que podamos marcar
    [SerializeField] float lockEnemyAngle; // Ángulo al que deben encontrarse los enemigos respecto al frente de la cámara
    [SerializeField] float cameraLockSpeed; // Velocidad a la que la cámara sigue al enemigo al marcarlo
    private float rotX; // Valor de input vertical corregido a horizontal en mundo
    private float rotY; // Valor de input horizontal corregido a vertical en mundo

    [Header("Conditional Values")]
    bool isGamepad; // Nos indica si estamos usando Gamepad
    public bool camLocked; // Indicamos si estamos marcando al enemigo

    // Input
    Vector2 lookInput; // Se almacena el valor del input de cámara

    void Start()
    {
        // Obtener referencias
        marker = GameObject.Find("EnemyMarker");

        // Definir valores de inicio
        Vector3 rot = transform.localRotation.eulerAngles;
        rotX = rot.x;
        rotY = rot.y;
        enemyLocked = null;
        marker.SetActive(false);
    }

    void Update()
    {
        // Bloquar y esconder cursor (¡¡¡Hay que mover al Start cuando se pueda!!!)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Manejo de rotación de cámara
        if (!camLocked) HandleRotation();
    }

    void LateUpdate()
    {
        // Seguimiento de la camara al Player
        FollowPlayer();

        // Fijación de cámara hacia un enemigo
        if (camLocked) { LookAtEnemy(); MarkerPosOnEnemy(); }
    }

    # region CameraHandling

    // Seguimiento de la camara al Player
    void FollowPlayer()
    {
        // Desplazamos la cámara suavemente hacia la posición objetivo del jugador
        transform.position = Vector3.Lerp(transform.position, playerPos.position, cameraMoveSpeed * Time.deltaTime);
    }

    // Manejo de rotación de cámara 
    void HandleRotation()
    {
        // Obtener input
        float inputX = lookInput.x;
        float inputY = lookInput.y;

        // Aplicar Sensibilidad
        float sensitivity = isGamepad? stickSensitivity : mouseSensitivity;

        rotX += inputY * sensitivity * Time.deltaTime;
        rotY += inputX * sensitivity * Time.deltaTime;

        // Limitar el giro vertical de la cámara
        rotX = Mathf.Clamp(rotX, -clampAngle, clampAngle);

        // Aplicar el giro obtenido
        Quaternion localRotation = Quaternion.Euler(-rotX, rotY, 0.0f);
        transform.rotation = localRotation;
    }

    #endregion

    #region Camera/EnemyInteraction

    // Detectar enemigo para marcarlo y fijar la cámara
    void DetectEnemy()
    {
        if (!camLocked)
        {
            // Almacenamos enemigos que hay dentro del rango de detección
            Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, lockEnemyRadius, enemyLayer);

            float closestDistanceToCenter = Mathf.Infinity;
            GameObject closestEnemy = null;

            // Coordenadas del centro de la pantalla
            Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);

            // Elegimos enemigo más cercano al centro de la pantalla
            foreach (Collider enemy in enemiesInRange)
            {
                // Convertimos la posición del enemigo al espacio de pantalla
                Vector3 enemyInScreenPos = playerCamera.WorldToScreenPoint(enemy.transform.position);

                // Si el enemigo está detrás de la cámara, ignorarlo
                if (enemyInScreenPos.z < lockEnemyAngle) continue;

                // Calculamos la distancia del enemigo al centro de la pantalla
                float distanceToCenter = Vector2.Distance(new Vector2(enemyInScreenPos.x, enemyInScreenPos.y), screenCenter);

                // Comprobamos si este enemigo está más cerca del centro que el anterior más cercano
                if (distanceToCenter < closestDistanceToCenter)
                {
                    closestDistanceToCenter = distanceToCenter;
                    closestEnemy = enemy.gameObject;
                }
            }

            if (closestEnemy != null)
            {
                // Si hemos encontrado un enemigo cercano al centro, lo almacenamos
                enemyLocked = closestEnemy;

                // Indicamos que la cámara está fijada
                camLocked = true;
            }
            else
            {
                // Si no encontramos ningún enemigo
                enemyLocked = null;
            }
            
        }
        else if (camLocked)
        {
            // Indicamos que la cámara ya no debe estar fijada
            camLocked = false;

            // Escondemos el marcador 
            marker.SetActive(false);
        }
    }

    // Fijación de cámara hacia un enemigo
    void LookAtEnemy()
    {
        // Calculamos la dirección hacia el enemigo
        Vector3 directionToEnemy = enemyLocked.transform.position - transform.position;

        // Calculamos la rotación deseada para mirar al enemigo
        Quaternion targetRotation = Quaternion.LookRotation(directionToEnemy);

        // Realizamos una rotación suave desde la rotación actual hacia la rotación deseada
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, cameraLockSpeed * Time.deltaTime);

        // Si el enemigo ha sido derrotado, dejamos de fijar cámara
        camLocked = enemyLocked.activeSelf;
    }

    // Colocar el marcador encima del enemigo (¡¡¡Considerar cambiar método a otro script!!!)
    void MarkerPosOnEnemy()
    {
        // Comprueba si hay un enemigo para seguir
        if (enemyLocked != null && camLocked)
        {
            // Mostramos marcador
            marker.SetActive(true);

            // Obtenemos la posición del marcador, en este caso del objeto padre del LockPoint
            Transform markerTransform = enemyLocked.transform.parent;

            // Convierte la posición del enemigo en el mundo a coordenadas de pantalla
            Vector3 screenPos = playerCamera.WorldToScreenPoint(markerTransform.position);

            // Establece la posición del marcador
            marker.transform.position = screenPos;
        }
    }

    #endregion

    #region InputReading

    // Leer input de cámara
    public void OnLook(InputAction.CallbackContext context) 
    {
        lookInput = context.ReadValue<Vector2>();
    }

    // Leer input de fijado de cámara en enemigo
    public void OnLockCam(InputAction.CallbackContext context) 
    {
        if (context.started)
        {
            DetectEnemy();
        }
    }

    // Detección del input que estamos utilizando
    public void OnDeviceChange() 
    {
        // Verificar si el control es mediante gamepad
        isGamepad = playerInput.currentControlScheme.Equals("Gamepad");
    }

    #endregion
}
