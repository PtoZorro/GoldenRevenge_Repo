using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour, ICombatEvents // Implementar interfaz de eventos de animación
{
    [Header("External References")]
    [SerializeField] PlayerAnimations anim; // Script de control de animaciones
    [SerializeField] GameObject weaponCollider; // Hitbox del arma del Jugador
    [SerializeField] Transform weapon; // Posición del arma en el Rig

    [Header("Core Stats")]
    [SerializeField] float recoverStamTime; // Tiempo en el que se comienza a recuperar stamina
    [SerializeField] float increaseStamSpeed; // Velocidad de recuperación de stamina
    int maxHealth; // Salud máxima
    int maxStamina; // Stamina máxima
    int health; // Salud del Jugador
    int stamina; // Stamina del Jugador
    int previousStamina; // Detección de decrementos de stamina

    [Header("Combat Settings")]
    [SerializeField] int maxComboAttacks; // Número máximo de ataques en un mismo combo
    [SerializeField] int[] comboDamages; // Daños para cada ataque del combo
    [SerializeField] float attackRate; // Tiempo en que se nos permite accionar otro combo de ataques

    [Header("States")]
    public bool isAttacking; // Estado de atacando
    public bool rotationLocked; // Negación de rotación
    bool colliderActive; // Valor que indica que la HitBox del arma está activa
    bool canNextAction; // Se permite ejecutar la próxima acción leída por el input
    bool canDealDamage; // Evitamos ejercer daño más de una vez por ataque ejecutado
    bool canRecovStam; // Permitir la recuperación de Stamina
    int currentAttack; // El ataque que se ejecutará, mandado por el input
    int attackNum; // El ataque que se está ejecutando, indicado por su animación

    [Header("Initialization States")]
    bool isInitialized = false; // Indica que el Jugador se acaba de activar para recoger información inicial del Singleton

    [Header("Global Positions")]
    Vector3 colliderInitialPos;
    Quaternion colliderInitialRot;


    void Awake()
    {
        // Valores de inicio prioritarios
        colliderInitialPos = weaponCollider.transform.localPosition;
        colliderInitialRot = weaponCollider.transform.localRotation;
    }

    void Start()
    {
        // Comunicación inicial con el Singleton
        SingletonUpdate();

        // Referencias
        anim = GetComponent<PlayerAnimations>();

        // Valores de inicio
        previousStamina = stamina;
        isAttacking = false;
        currentAttack = 0;
        canNextAction = true;
        rotationLocked = false;
        colliderActive = false;

        // En el inicio los colliders de armas empiezan apagados
        weaponCollider.SetActive(false);

        // Indicamos que ya se ha inicializado el objeto con sus valores iniciales
        isInitialized = true;
    }

    void Update()
    {
        // Mantener el collider siguiendo al arma en el Rig
        FollowWeapon();

        // Manejo de la salud constante
        HealthManagement();

        // Manejo de la stamina constante
        StaminaManagement();

        // Comunicación constante con el Singleton
        SingletonUpdate();
    }

    #region HealthManagement

    void HealthManagement()
    {
        if (health >= maxHealth)
        {
            // No dejamos la salud suba de su valor máximo
            health = maxHealth;
        }
        else if (health < 0)
        {
            // No dejamos que la salud baje de 0
            health = 0;
        }
    }

    // Recibir daño
    public void TakeDamage(int damageRecived)
    {
        // Restamos daño especificado
        health -= damageRecived;
    }

    #endregion

    #region StaminaManagement

    // Manejo de la Stamina del jugador
    void StaminaManagement()
    {
        if (stamina >= maxStamina)
        {
            // No dejamos Stamina suba de su valor máximo
            stamina = maxStamina;
        }
        else if (stamina >= 0 && stamina < maxStamina)
        {
            // Recuperación de la Stamina
            StaminaRecovery();
        }
        else if (stamina < 0)
        {
            // No dejamos Stamina baje de 0
            stamina = 0;
        }

        // Almacenamos el valor previo de Stamina en cada frame
        previousStamina = stamina;
    }

    // Recuperación de la Stamina cuando se nos permite
    void StaminaRecovery()
    {
        // Detectar precisamante en que momento hay un decremento de stamina
        if (stamina < previousStamina)
        {
            // Cancelamos el permiso anterior de recuperación si lo había
            CancelInvoke(nameof(AllowStaminaRecovery));

            // Damos permiso de recuperación pasado el tiempo indicado
            Invoke(nameof(AllowStaminaRecovery), recoverStamTime);

            // Negamos el permiso para cancelar la recuperación en marcha si volvemos a consumir stamina en el acto
            canRecovStam = false;
        }

        // Si no se permite la recuperación, salimos
        if (!canRecovStam) return;

        // Creamos un valor float de Stamina para poder calcular un incremento con el tiempo
        float staminaModified = stamina;

        // Incrementamos con el tiempo
        staminaModified += (increaseStamSpeed * Time.deltaTime);

        // Devolvemos el valor modificado a la variable de stamina
        stamina = Mathf.FloorToInt(staminaModified);
    }

    // Permitir la recuperación de Stamina
    void AllowStaminaRecovery()
    {
        canRecovStam = true;
    }

    #endregion

    #region AttackManagement

    // Gestiona cual es el próximo ataque y lo ejecuta
    void Attack() 
    {
        // Solo accionamos si tenemos permiso de atacar y no ha terminado el combo
        if (canNextAction && currentAttack < maxComboAttacks)
        {
            // Se establecen parametros
            isAttacking = true;
            canNextAction = false;
            currentAttack++;

            // Reproducimos la animación de ataque correspondiente
            anim.AttackAnimations(currentAttack);
        }
    }

    // Al comenzar la animación se establecen parametros
    public void OnStartAttack(int attackAnimNum) 
    {
        // Permitimos ejercer daño
        canDealDamage = true;

        // Indicamos que número de ataque se está ejecutando
        attackNum = attackAnimNum;

        // Permitimos rotación hasta que se ejecute parte del ataque
        rotationLocked = false; 
    }

    // El ataque que termine de reproducirse será el último del combo y llamará a la función
    public void OnEndAttack() 
    {
        // Se establecen parametros
        isAttacking = false; 
        rotationLocked = false; 
        canNextAction = false;
        currentAttack = 0;

        // Pasado un tiempo configurable se podrá iniciar otro ataque
        Invoke(nameof(AllowAttack), attackRate); 
    }

    // Nos permite volver a iniciar un combo pasado un timepo "attackRate"
    public void AllowAttack()
    {
        canNextAction = true;
    }

    // Hacemos daño al enemigo mediante la colisión
    public void InflictDamage(EnemyCombat enemy)
    {
        // Solo si se permite ejercer daño
        if (canDealDamage)
        {
            // Evitamos ejercer daño más de una vez por ataque
            canDealDamage = false;

            // Seleccionamos el daño del ataque que se está ejecutando
            int damage = comboDamages[attackNum - 1];

            // El enemigo recibe daño
            enemy.TakeDamage(damage);
        }
    }

    #endregion

    #region WeaponHitboxManagement

    // Mantener el collider siguiendo al arma en el Rig
    void FollowWeapon()
    {
        // Seguirá al Rig solo cuando el collider está activo
        if (!colliderActive) return;

        // Seguimiento del arma en el rig
        weaponCollider.transform.position = weapon.position;
        weaponCollider.transform.rotation = weapon.rotation;
    }

    // Habilitar el collider de las armas mediante la animación
    public void EnableCollider() 
    {
        weaponCollider.SetActive(true);
        colliderActive = true;
    }

    // Deshabilitar el collider de las armas mediante la animación
    public void DisableCollider() 
    {
        weaponCollider.SetActive(false);
        colliderActive = false;

        // Lo llevamos al punto de inicio
        weaponCollider.transform.localPosition = colliderInitialPos;
        weaponCollider.transform.localRotation = colliderInitialRot;
    }

    #endregion

    #region GeneralAnimationEvents

    // Permite leer el siguiente input en cierto punto de la animación
    public void CanInterrupt() 
    {
        canNextAction = true;
    }

    // Deshabilita la rotación en cierto punto de la animación
    public void LockRotation() 
    {
        rotationLocked = true;
    }

    #endregion

    #region SingletonComunication

    void SingletonUpdate()
    {
        // Si el objeto se está activando, recogemos los datos necesarios del Singleton
        if (!isInitialized)
        {
            maxStamina = GameManager.Instance.maxStamina;
            maxHealth = GameManager.Instance.maxHealth;
            health = GameManager.Instance.health;
            stamina = GameManager.Instance.stamina;
        }
        else
        {
            // Si el objeto ya se ha iniciado, comunicamos constantemente los datos necesarios al Singleton
            GameManager.Instance.health = health;
            GameManager.Instance.stamina = stamina;
        }
    }

    #endregion

    #region InputReading

    public void OnAttack(InputAction.CallbackContext context) // Lectura de input de ataque
    {
        if (context.started)
        {
            Attack();
        }
    }

    #endregion
}
