using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Object Components")]
    Rigidbody rb;

    [Header("External Components")]
    [SerializeField] Transform cameraPos;
    [SerializeField] Animator animator;

    [Header("Movement Stats")]
    [SerializeField] float speed;
    [SerializeField] float rotationSmooth;

    [Header("Animation Config")]
    [SerializeField] bool isWalk;
    [SerializeField] bool isRun;
    [SerializeField] float walkAnimMinSpeed;
    [SerializeField] float minSpeedToRunAnim;

    [Header("Input values")]
    Vector2 moveInput;
    Vector3 desiredMoveDirection;

    // Start is called before the first frame update
    void Start()
    {
        // Obtenemos las referencias necesarias
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Manejo de animaciones
        HandleAnimations();
    }

    void FixedUpdate()
    {
        // Movimiento del personaje
        HandleMovement();

        // Rotación del personaje
        HandleRotation();
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


    void HandleAnimations()
    {
        // Obtener la magnitud del input de movimiento
        float inputMagnitude = moveInput.magnitude;

        // Según la velocidad activamos animación de caminar o trotar
        if (inputMagnitude > 0 && inputMagnitude < minSpeedToRunAnim)
        {
            isWalk = true;
            isRun = false;
            animator.SetFloat("walkSpeed", Mathf.Lerp(walkAnimMinSpeed, 1f, inputMagnitude / minSpeedToRunAnim)); // Velocidad de animación ajustada
        }
        else if (inputMagnitude >= minSpeedToRunAnim)
        {
            isWalk = false;
            isRun = true;
            animator.SetFloat("walkSpeed", 1f); // Velocidad estándar para trotar/correr
        }
        else
        {
            isWalk = false;
            isRun = false;
            animator.SetFloat("walkSpeed", 0f); // Detener la animación
        }

        animator.SetBool("walk", isWalk);
        animator.SetBool("run", isRun);
    }


    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
}
