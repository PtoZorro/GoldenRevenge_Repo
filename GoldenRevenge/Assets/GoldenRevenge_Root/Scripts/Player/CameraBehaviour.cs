using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CameraBehaviour : MonoBehaviour
{
    [Header("External References")]
    [SerializeField] Transform target;
    [SerializeField] PlayerInput playerInput;
    [SerializeField] Camera playerCamera;
    [SerializeField] LayerMask enemyLayer;
    public GameObject enemyLocked;
    GameObject marker;

    [Header("Private References")]
    Vector3 velocity = Vector3.zero;

    [Header("Stats")]
    [SerializeField] float cameraMoveSpeed;
    [SerializeField] float clampAngle;
    [SerializeField] float mouseSensitivity;
    [SerializeField] float stickSensitivity;
    [SerializeField] float lockEnemyRadius;
    [SerializeField] float lockEnemyAngle;
    [SerializeField] float cameraLockSpeed;
    private float rotX;
    private float rotY;

    [Header("Conditional Values")]
    bool isGamepad;
    public bool camLocked;

    [Header("Input")]
    Vector2 lookInput;

    // Start is called before the first frame update
    void Start()
    {
        // Definir valores de inicio
        Vector3 rot = transform.localRotation.eulerAngles;
        rotX = rot.x;
        rotY = rot.y;
        enemyLocked = null;
        marker = GameObject.Find("EnemyMarker");
        marker.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // Bloquar y esconder cursor
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

    void FollowPlayer()
    {
        // Desplazamos la cámara suavemente hacia la posición objetivo del jugador
        transform.position = Vector3.Lerp(transform.position, target.position, cameraMoveSpeed * Time.deltaTime);
    }

    void DetectEnemy()
    {
        if (!camLocked)
        {
            Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, lockEnemyRadius, enemyLayer);

            float closestDistanceToCenter = Mathf.Infinity;
            GameObject closestEnemy = null;

            // Coordenadas del centro de la pantalla
            Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);

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

    void LookAtEnemy()
    {
        // Calculamos la dirección hacia el enemigo
        Vector3 directionToEnemy = enemyLocked.transform.position - transform.position;

        // Calculamos la rotación deseada para mirar al enemigo
        Quaternion targetRotation = Quaternion.LookRotation(directionToEnemy);

        // Realizamos una rotación suave desde la rotación actual hacia la rotación deseada
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, cameraLockSpeed * Time.deltaTime);
    }

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

    public void OnLook(InputAction.CallbackContext context)
    {
        // Leer input de cámara
        lookInput = context.ReadValue<Vector2>();
    }

    public void OnLockCam(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            // Leer input de bloqueo de cámara en enemigo
            DetectEnemy();
        }
    }

    public void OnDeviceChange()
    {
        // Verificar si el control es mediante gamepad
        isGamepad = playerInput.currentControlScheme.Equals("Gamepad");
    }
}
