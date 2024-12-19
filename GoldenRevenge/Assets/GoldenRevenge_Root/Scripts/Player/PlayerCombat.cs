using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static IAnimationEvents;

public class PlayerCombat : MonoBehaviour, IAnimationEvents, IGeneralStatesEvents, IAttackEvents, IRollEvents, IHealEvents // Implementar interfaz de eventos de animaci�n
{
    [Header("External References")]
    [SerializeField] GameObject weaponCollider; // Hitbox del arma del Jugador
    [SerializeField] Transform weapon; // Posici�n del arma en el Rig
    PlayerMovement move; // Script de control de movimiento
    PlayerAnimations anim; // Script de control de animaciones

    [Header("Core Stats")]
    [SerializeField] float recoverStamTime; // Tiempo en el que se comienza a recuperar stamina
    [SerializeField] float increaseStamSpeed; // Velocidad de recuperaci�n de stamina
    int maxHealth; // Salud m�xima
    int maxStamina; // Stamina m�xima
    int maxHealItems; // N�mero m�ximo de curativos
    int health; // Salud del Jugador
    int stamina; // Stamina del Jugador
    int healItems; // N�mero de curativos

    [Header("Combat Stats")]
    [SerializeField] int maxComboAttacks; // N�mero m�ximo de ataques en un mismo combo
    [SerializeField] int[] comboDamages; // Da�os para cada ataque del combo
    [SerializeField] float attackRate; // Tiempo en que se nos permite accionar otro combo de ataques
    [SerializeField] int healthRestored; // Cantidad de salud restablecida al curarse
    [SerializeField] int attackStamCost; // Coste de stamina al realizar un ataque
    [SerializeField] int rollStamCost; // Coste de stamina al realizar un esquive

    [Header("Scene Settings")]
    [SerializeField] float deathTransitionDuration; // Tiempo que dura la transici�n de muerte

    [Header("States")]
    public bool isAttacking; // Estado de atacando
    public bool isRolling; // Estado de esquivar
    public bool isHealing; // Estado de curaci�n
    public bool rollImpulse; // Estado de impulso durante el esquive
    bool isInvincible; // Estado de invencibilidad durante el esquive
    bool isDead; // Estado de muerte
    bool colliderActive; // Valor que indica que la HitBox del arma est� activa
    bool canNextAction; // Se permite ejecutar la pr�xima acci�n le�da por el input
    bool canAttack; // Se permite ejecutar ataque de nuevo al terminar el combo
    bool canDealDamage; // Evitamos ejercer da�o m�s de una vez por ataque ejecutado
    bool canRecovStam; // Permitir la recuperaci�n de Stamina
    bool canInteract; // Puede interactuar con objeto interactuable cercano
    int currentAttack; // El ataque que se ejecutar�, mandado por el input

    [Header("Modifier Values")]
    int previousStamina; // Detecci�n de decrementos de stamina

    [Header("Initialization States")]
    bool isInitialized = false; // Indica que el Jugador se acaba de activar para recoger informaci�n inicial del Singleton

    [Header("Global Positions")]
    [SerializeField] Transform initialSavePoint; // Punto de guardado establecido al inicio
    [SerializeField] Vector3 savePointPos; // Punto de guardado establecido
    [SerializeField] Vector3 bonfirePos; // Punto de la hoguera con la que podemos interactuar
    Vector3 colliderInitialPos;
    Quaternion colliderInitialRot;

    void Start()
    {
        // Set inicial de todos los valores
        ResetValues();
    }

    void Update()
    {
        // Stamina infinita para debug
        //stamina = maxStamina;

        // Mantener el collider siguiendo al arma en el Rig
        FollowWeapon();

        // Manejo de la salud constante
        HealthManagement();

        // Manejo de la stamina constante
        StaminaManagement();

        // Comunicaci�n constante con el Singleton
        SingletonUpdate();
    }

    #region ResetsManagement

    // Al respawnear
    void OnEnable()
    {
        // Se ejecutan los reseteos necesarios
        ResetValues();
    }

    // Reseteo de Valores 
    void ResetValues()
    {
        // El objeto no est� inicializado
        isInitialized = false;

        // Comunicaci�n inicial con el Singleton
        SingletonUpdate();

        // Referencias
        move = GetComponent<PlayerMovement>();
        anim = GetComponent<PlayerAnimations>();

        // Valores de inicio
        isDead = false;
        previousStamina = stamina;
        currentAttack = 0;
        canNextAction = true;
        canAttack = true;

        // Movimiento desbloqueado
        ManageMovement("moveUnlock");
        ManageMovement("rotUnlock");
        ManageMovement("markUnlock");

        // En el inicio los colliders de armas empiezan apagados
        colliderInitialPos = weaponCollider.transform.localPosition;
        colliderInitialRot = weaponCollider.transform.localRotation;
        weaponCollider.SetActive(false);

        // Indicamos que ya se ha inicializado el objeto con sus valores iniciales
        isInitialized = true;
    }

    #endregion

    #region HealthManagement

    void HealthManagement()
    {
        if (health >= maxHealth)
        {
            // No dejamos la salud suba de su valor m�ximo
            health = maxHealth;
        }
        else if (health <= 0)
        {
            // No dejamos que la salud baje de 0
            health = 0;

            // Estado de muerte
            DeathReset();
        }
    }

    // Recibir da�o
    public void TakeDamage(int damageRecived)
    {
        // En estado de invencivilidad no nos quitan salud
        if (isInvincible) return;

        // Restamos da�o especificado
        health -= damageRecived;
    }

    // En estado de muerte paramos la actividad
    void DeathReset()
    {
        // Solo activamos una vez al morir
        if (isDead) return;

        // Estado de muerte
        isDead = true;

        // Comunicamos muerte al Singleton
        GameManager.Instance.dead = true;

        // Comunicamos el �ltimo punto de guardado al Singleton
        GameManager.Instance.savePointPos = savePointPos;

        // Mandamos al Singleton el aviso de Respawn
        Invoke(nameof(Respawn), deathTransitionDuration);

        // Movimiento bloqueado
        ManageMovement("moveLock");
        ManageMovement("rotLock");
        ManageMovement("markLock");

        // Animaci�n de muerte
        anim.DeathAnimation();
    }

    #endregion

    #region StaminaManagement

    // Manejo de la Stamina del jugador
    void StaminaManagement()
    {
        // En estado de muerte no ejecutamos
        if (isDead) return;

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

    // Consumir stamina seg�n la acci�n
    public void ConsumeStamina(int stamConsumed)
    {
        // Restamos da�o especificado
        stamina -= stamConsumed;
    }

    #endregion

    #region AttackManagement

    // Gestiona cual es el pr�ximo ataque y lo ejecuta
    void Attack() 
    {
        // En estado de muerte no ejecutamos
        if (isDead) return;

        // Solo accionamos si tenemos permiso de atacar y no ha terminado el combo
        if (!canNextAction || !canAttack || currentAttack >= maxComboAttacks) return;

        // Solo ejecutamos si tenemos stamina suficiente
        if (stamina <= 0) return;

        // Actualizamos estados
        SetStates(true, false, false);

        // Permitomos al arma ejercer da�o
        canDealDamage = true;

        // Negamos m�s acciones
        CanInterrupt("can't");
        canAttack = false;

        // Indicamos el ataque que toca ejecutar
        currentAttack++;

        // Consumo de stamina
        ConsumeStamina(attackStamCost);

        // Reproducimos la animaci�n de ataque correspondiente
        anim.AttackAnimations(currentAttack);
    }

    // El ataque que termine de reproducirse ser� el �ltimo del combo y llamar� a la funci�n
    public void OnEndAttack() 
    {
        // Estado de ataque apagado
        isAttacking = false;

        // Permitimos m�s acciones
        CanInterrupt("can");

        // Reseteamos contador de ataques
        currentAttack = 0;

        // Pasado un tiempo configurable se podr� iniciar otro ataque
        Invoke(nameof(AllowAttack), attackRate); 
    }

    // Nos permite volver a iniciar un combo pasado un timepo "attackRate"
    public void AllowAttack()
    {
        canAttack = true;
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
            int damage = comboDamages[currentAttack - 1];

            // El enemigo recibe da�o
            enemy.TakeDamage(damage);
        }
    }

    #endregion

    #region RollManagement

    // Ejecutar Esquive
    void Roll()
    {
        // En estado de muerte no ejecutamos
        if (isDead) return;

        // Solo ejecutamos si tenemos permiso
        if (!canNextAction) return;
        
        // Solo ejecutamos si tenemos stamina suficiente
        if (stamina <= 0) return;

        // Actualizamos estados
        SetStates(false, true, false);

        // Negamos m�s acciones
        canNextAction = false;

        // Consumo de stamina
        ConsumeStamina(rollStamCost);

        // Reproducimos la animaci�n de esquivar
        anim.RollAnimation();
    }
    
    // Al final de la animaci�n de esquivar
    void OnEndRoll()
    {
        // Estado de esquivar apagado
        isRolling = false;
    }

    // Habilitar y deshabilitar la Hitbox del jugador
    public void ManageHitBox(string state)
    {
        // El estado de invencibilidad depender� de la actividad te�rica de la Hitbox
        isInvincible = state == "off" ? true : false;
    }

    // Habilitar o deshabilitar estado de impulso durante el esquive
    public void ManageImpulse(string state)
    {
        rollImpulse = state == "on" ? true : false;
    }

    #endregion

    #region HealManagement

    // Ejecutar curaci�n
    void Heal()
    {
        // En estado de muerte no ejecutamos
        if (isDead) return;

        // Solo ejecutamos si tenemos permiso
        if (!canNextAction) return;

        // Solo ejecutamos si tenemos curativos
        if (healItems < 1) return;

        // Actualizamos estados
        SetStates(false, false, true);

        // Negamos m�s acciones
        canNextAction = false;

        // Cancelar fuerzas residuales del rb al realizar paradas
        move.CancelResidualMove();
        move.CancelResidualRot();

        // Consumo de item curativo
        ConsumeHealing();

        // Reproducimos la animaci�n de curaci�n
        anim.HealAnimation();
    }

    // Manejo de la Curativos del jugador
    void ConsumeHealing()
    {
        // Consumimos item
        healItems--;

        // Control de cantidad
        if (healItems >= maxHealItems)
        {
            // No dejamos que los curativos suban de su valor m�ximo
            healItems = maxHealItems;
        }
        else if (healItems <= 0)
        {
            // No dejamos los curativos bajen de 0
            healItems = 0;
        }
    }

    // Restablecer la salud
    public void RestoreHealth()
    {
        health += healthRestored;
    }

    // Al acabar la animaci�n de Curaci�n
    void OnEndHeal()
    {
        // Estado de curaci�n apagado
        isHealing = false;
    }

    #endregion

    #region BonfireInteraction

    // Interacci�n con el punto de guardado (hoguera)
    void BonfireInteract()
    {
        // Si no podemos interactuar volvemos
        if (!canInteract) return;

        // Convertimos el punto de la hoguera en el nuevo punto de guardado
        savePointPos = bonfirePos;

        // Reseteamos el mapa
        BonfireReset();
    }

    // Reseteo de valores al descansar en hoguera (���Intentar diferenciarlo del estado de muerte!!!)
    void BonfireReset()
    {
        // Comunicamos muerte al Singleton
        GameManager.Instance.dead = true;

        // Comunicamos el �ltimo punto de guardado al Singleton
        GameManager.Instance.savePointPos = savePointPos;

        // Mandamos al Singleton el aviso de Respawn
        Invoke(nameof(Respawn), deathTransitionDuration);
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

    #region GeneralStatesManagment

    // Gestion de estados generales (Evita errores por interrupciones)
    void SetStates(bool attacking, bool rolling, bool healing)
    {
        isAttacking = attacking;
        isRolling = rolling;
        isHealing = healing;

        // Reseteo de ataques de combo
        if (!attacking) currentAttack = 0;

        // Movimiento liberado entre acciones
        ManageMovement("moveUnlock");
        ManageMovement("rotUnlock");
        ManageMovement("markUnlock");
    }

    #endregion

    #region GeneralAnimationEvents

    // Deshabilita o habilita estados de movimiento 
    public void ManageMovement(string lockState)
    {
        // Comprueba la animaci�n que ha finalizado
        switch (lockState)
        {
            case "moveLock":
                move.moveLocked = true; // Bloquear movimiento
                move.CancelResidualMove(); // Cancelar fuerzas residuales del rb al realizar paradas
                break;
            case "moveUnlock":
                move.moveLocked = false; // Desbloquear movimiento
                break;
            case "rotLock":
                move.rotationLocked = true; // Bloquear rotaci�n
                move.CancelResidualRot(); // Cancelar fuerzas residuales del rb al realizar paradas
                break;
            case "rotUnlock":
                move.rotationLocked = false; // Desbloquear rotaci�n
                break;
            case "markLock":
                move.markEnemyLocked = true; // Bloquear marcado enemigo
                break;
            case "markUnlock":
                move.markEnemyLocked = false; // Desbloquear marcado enemigo
                break;
        }
    }

    // Permite leer el siguiente input en cierto punto de la animaci�n
    public void CanInterrupt(string canInterrupt)
    {
        canNextAction = canInterrupt == "can" ? true : false;

        // Tambi�n gestionamos poder atacar
        canAttack = canInterrupt == "can" ? true : false;
    }

    // Notifica el fin de una animaci�n espec�fica
    public void OnEndAnimation(string animName)
    {
        // Comprueba la animaci�n que ha finalizado
        switch (animName)
        {
            case "attack":
                OnEndAttack(); break; // Fin de animaci�n de ataque
            case "roll":
                OnEndRoll(); break; // Inicio de esquive
            case "heal":
                OnEndHeal(); break; // Inicio de esquive
        }
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
            maxHealItems = GameManager.Instance.maxHealItems;
            health = GameManager.Instance.health;
            stamina = GameManager.Instance.stamina;
            healItems = GameManager.Instance.healItems;

            // Si todav�a no hay un punto de Respawn, lo define el jugador en escena
            if (GameManager.Instance.savePointPos == Vector3.zero)
            {
                savePointPos = initialSavePoint.position;
                GameManager.Instance.savePointPos = savePointPos;
            }
                
        }
        else
        {
            // Si el objeto ya se ha iniciado, comunicamos constantemente los datos necesarios al Singleton
            GameManager.Instance.health = health;
            GameManager.Instance.stamina = stamina;
            GameManager.Instance.healItems = healItems;
        }
    }

    // Llamar a la funci�n del Singleton de Respawneo
    void Respawn()
    {
        GameManager.Instance.RespawnPlayer();
    }

    #endregion

    #region CollisionDetection

    void OnTriggerEnter(Collider other)
    {
        // Verificar si el objeto es interactuable
        if (other.CompareTag("Bonfire"))
        {
            canInteract = true;

            // Almacenamos la posici�n de la hoguera temporalmente
            bonfirePos = other.transform.position;
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Verificar si el objeto es interactuable
        if (other.CompareTag("Bonfire"))
        {
            canInteract = false;

            // Mantenemos el punto de guardado anterior 
            bonfirePos = savePointPos;
        }
    }

    #endregion

    #region InputReading

    // Lectura de input de ataque
    public void OnAttack(InputAction.CallbackContext context) 
    {
        if (context.started)
        {
            Attack();
        }
    }

    // Lectura de input de esquivar
    public void OnRoll(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Roll();
        }
    }

    // Lectura de input de curaci�n
    public void OnHeal(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Heal();
        }
    }

    // Lectura de input de curaci�n
    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            BonfireInteract();
        }
    }

    #endregion
}
