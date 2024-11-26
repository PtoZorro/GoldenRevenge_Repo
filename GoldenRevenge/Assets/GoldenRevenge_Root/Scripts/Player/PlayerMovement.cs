using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Object Components")]
    Rigidbody rb;

    [Header("External Components")]
    [SerializeField] Transform cameraPos;
    [SerializeField] Animator animator;

    [Header("Movement values")]
    [SerializeField] float speed;
    [SerializeField] float rotationSmooth;

    [Header("Animation States")]
    [SerializeField] bool isWalk;
    [SerializeField] bool isRun;

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

        // Calculamos el movimiento multiplicando con la velocidad
        Vector3 movement = desiredMoveDirection.normalized * speed * inputMagnitude * Time.fixedDeltaTime;

        // Calculamos cual será la próxima posición sumando el movimiento a la posición actual del RB
        Vector3 newPosition = rb.position + movement;

        // Nos movemos mediante el RB
        rb.MovePosition(newPosition);
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
        // Según la velocidad activamos animación de caminar o trotar
        if (moveInput.magnitude > 0 && moveInput.magnitude < 0.5) { isWalk = true; isRun = false; }
        else if (moveInput.magnitude >= 0.5) { isWalk = false; isRun = true; }
        else { isWalk = false; isRun = false; }

        animator.SetBool("walk", isWalk ? true : false);
        animator.SetBool("run", isRun ? true : false);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
}
