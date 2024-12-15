using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour, ICombatEvents // Implementar interfaz de eventos de animaci�n
{
    [Header("External References")]
    [SerializeField] PlayerAnimations anim; // Script de control de animaciones
    [SerializeField] GameObject weaponCollider; // Hitbox del arma del Jugador
    [SerializeField] Transform weapon; // Posici�n del arma en el Rig

    [Header("Core Stats")]
    [SerializeField] float recoverStamTime; // Tiempo en el que se comienza a recuperar stamina
    [SerializeField] float increaseStamSpeed; // Velocidad de recuperaci�n de stamina
    int maxHealth; // Salud m�xima
    int maxStamina; // Stamina m�xima
    int health; // Salud del Jugador
    int stamina; // Stamina del Jugador
    int previousStamina; // Detecci�n de decrementos de stamina

    [Header("Combat Settings")]
    [SerializeField] int maxComboAttacks; // N�mero m�ximo de ataques en un mismo combo
    [SerializeField] int[] comboDamages; // Da�os para cada ataque del combo
    [SerializeField] float attackRate; // Tiempo en que se nos permite accionar otro combo de ataques

    [Header("States")]
    public bool isAttacking; // Estado de atacando
    public bool rotationLocked; // Negaci�n de rotaci�n
    bool colliderActive; // Valor que indica que la HitBox del arma est� activa
    bool canNextAction; // Se permite ejecutar la pr�xima acci�n le�da por el input
    bool canDealDamage; // Evitamos ejercer da�o m�s de una vez por ataque ejecutado
    bool canRecovStam; // Permitir la recuperaci�n de Stamina
    int currentAttack; // El ataque que se ejecutar�, mandado por el input
    int attackNum; // El ataque que se est� ejecutando, indicado por su animaci�n

    [Header("Initialization States")]
    bool isInitialized = false; // Indica que el Jugador se acaba de activar para recoger informaci�n inicial del Singleton

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
        // Comunicaci�n inicial con el Singleton
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

        // Comunicaci�n constante con el Singleton
        SingletonUpdate();
    }

    #region HealthManagement

    void HealthManagement()
    {
        if (health >= maxHealth)
        {
            // No dejamos la salud suba de su valor m�ximo
            health = maxHealth;
        }
        else if (health < 0)
        {
            // No dejamos que la salud baje de 0
            health = 0;
        }
    }

    // Recibir da�o
    public void TakeDamage(int damageRecived)
    {
        // Restamos da�o especificado
        health -= damageRecived;
    }

    #endregion

    #region StaminaManagement

    // Manejo de la Stamina del jugador
    void StaminaManagement()
    {
        if (stamina >= maxStamina)
        {
            // No dejamos Stamina suba de su valor m�ximo
            stamina = maxStamina;
        }
        else if (stamina >= 0 && stamina < maxStamina)
        {
            // Recuperaci�n de la Stamina
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

    // Recuperaci�n de la Stamina cuando se nos permite
    void StaminaRecovery()
    {
        // Detectar precisamante en que momento hay un decremento de stamina
        if (stamina < previousStamina)
        {
            // Cancelamos el permiso anterior de recuperaci�n si lo hab�a
            CancelInvoke(nameof(AllowStaminaRecovery));

            // Damos permiso de recuperaci�n pasado el tiempo indicado
            Invoke(nameof(AllowStaminaRecovery), recoverStamTime);

            // Negamos el permiso para cancelar la recuperaci�n en marcha si volvemos a consumir stamina en el acto
            canRecovStam = false;
        }

        // Si no se permite la recuperaci�n, salimos
        if (!canRecovStam) return;

        // Creamos un valor float de Stamina para poder calcular un incremento con el tiempo
        float staminaModified = stamina;

        // Incrementamos con el tiempo
        staminaModified += (increaseStamSpeed * Time.deltaTime);

        // Devolvemos el valor modificado a la variable de stamina
        stamina = Mathf.FloorToInt(staminaModified);
    }

    // Permitir la recuperaci�n de Stamina
    void AllowStaminaRecovery()
    {
        canRecovStam = true;
    }

    #endregion

    #region AttackManagement

    // Gestiona cual es el pr�ximo ataque y lo ejecuta
    void Attack() 
    {
        // Solo accionamos si tenemos permiso de atacar y no ha terminado el combo
        if (canNextAction && currentAttack < maxComboAttacks)
        {
            // Se establecen parametros
            isAttacking = true;
            canNextAction = false;
            currentAttack++;

            // Reproducimos la animaci�n de ataque correspondiente
            anim.AttackAnimations(currentAttack);
        }
    }

    // Al comenzar la animaci�n se establecen parametros
    public void OnStartAttack(int attackAnimNum) 
    {
        // Permitimos ejercer da�o
        canDealDamage = true;

        // Indicamos que n�mero de ataque se est� ejecutando
        attackNum = attackAnimNum;

        // Permitimos rotaci�n hasta que se ejecute parte del ataque
        rotationLocked = false; 
    }

    // El ataque que termine de reproducirse ser� el �ltimo del combo y llamar� a la funci�n
    public void OnEndAttack() 
    {
        // Se establecen parametros
        isAttacking = false; 
        rotationLocked = false; 
        canNextAction = false;
        currentAttack = 0;

        // Pasado un tiempo configurable se podr� iniciar otro ataque
        Invoke(nameof(AllowAttack), attackRate); 
    }

    // Nos permite volver a iniciar un combo pasado un timepo "attackRate"
    public void AllowAttack()
    {
        canNextAction = true;
    }

    // Hacemos da�o al enemigo mediante la colisi�n
    public void InflictDamage(EnemyCombat enemy)
    {
        // Solo si se permite ejercer da�o
        if (canDealDamage)
        {
            // Evitamos ejercer da�o m�s de una vez por ataque
            canDealDamage = false;

            // Seleccionamos el da�o del ataque que se est� ejecutando
            int damage = comboDamages[attackNum - 1];

            // El enemigo recibe da�o
            enemy.TakeDamage(damage);
        }
    }

    #endregion

    #region WeaponHitboxManagement

    // Mantener el collider siguiendo al arma en el Rig
    void FollowWeapon()
    {
        // Seguir� al Rig solo cuando el collider est� activo
        if (!colliderActive) return;

        // Seguimiento del arma en el rig
        weaponCollider.transform.position = weapon.position;
        weaponCollider.transform.rotation = weapon.rotation;
    }

    // Habilitar el collider de las armas mediante la animaci�n
    public void EnableCollider() 
    {
        weaponCollider.SetActive(true);
        colliderActive = true;
    }

    // Deshabilitar el collider de las armas mediante la animaci�n
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

    // Permite leer el siguiente input en cierto punto de la animaci�n
    public void CanInterrupt() 
    {
        canNextAction = true;
    }

    // Deshabilita la rotaci�n en cierto punto de la animaci�n
    public void LockRotation() 
    {
        rotationLocked = true;
    }

    #endregion

    #region SingletonComunication

    void SingletonUpdate()
    {
        // Si el objeto se est� activando, recogemos los datos necesarios del Singleton
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
