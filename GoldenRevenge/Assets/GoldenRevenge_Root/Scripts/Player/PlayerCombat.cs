using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static IAnimationEvents;

public class PlayerCombat : MonoBehaviour, IAnimationEvents, IGeneralStatesEvents, IAttackEvents, IRollEvents, IHealEvents // Implementar interfaz de eventos de animación
{
    [Header("External References")]
    [SerializeField] GameObject weaponCollider; // Hitbox del arma del Jugador
    [SerializeField] Transform weapon; // Posición del arma en el Rig
    PlayerMovement move; // Script de control de movimiento
    PlayerAnimations anim; // Script de control de animaciones

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
    [SerializeField] int healthRestored; // Cantidad de salud restablecida al curarse
    [SerializeField] int attackStamCost; // Coste de stamina al realizar un ataque
    [SerializeField] int rollStamCost; // Coste de stamina al realizar un esquive

    [Header("States")]
    public bool isAttacking; // Estado de atacando
    public bool isRolling; // Estado de esquivar
    public bool isHealing; // Estado de curación
    public bool rollImpulse; // Estado de impulso durante el esquive
    bool isInvincible; // Estado de invencibilidad durante el esquive
    bool colliderActive; // Valor que indica que la HitBox del arma está activa
    bool canNextAction; // Se permite ejecutar la próxima acción leída por el input
    bool canDealDamage; // Evitamos ejercer daño más de una vez por ataque ejecutado
    bool canRecovStam; // Permitir la recuperación de Stamina
    int currentAttack; // El ataque que se ejecutará, mandado por el input

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
        move = GetComponent<PlayerMovement>();
        anim = GetComponent<PlayerAnimations>();

        // Valores de inicio
        previousStamina = stamina;
        currentAttack = 0;
        canNextAction = true;

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
        // En estado de invencivilidad no nos quitan salud
        if (isInvincible) return;

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

    // Consumir stamina según la acción
    public void ConsumeStamina(int stamConsumed)
    {
        // Restamos daño especificado
        stamina -= stamConsumed;
    }

    #endregion

    #region AttackManagement

    // Gestiona cual es el próximo ataque y lo ejecuta
    void Attack() 
    {
        // Solo accionamos si tenemos permiso de atacar y no ha terminado el combo
        if (!canNextAction || currentAttack >= maxComboAttacks) return;

        // Solo ejecutamos si tenemos stamina suficiente
        if (stamina < attackStamCost) return;

        // Estado de ataque y poder hacer daño
        isAttacking = true;
        canDealDamage = true;

        // Negamos más acciones
        canNextAction = false;

        // Indicamos el ataque que toca ejecutar
        currentAttack++;

        // Consumo de stamina
        ConsumeStamina(attackStamCost);

        // Cancelar fuerzas residuales del rb al realizar paradas
        move.CancelResidualMove();

        // Reproducimos la animación de ataque correspondiente
        anim.AttackAnimations(currentAttack);
    }

    // El ataque que termine de reproducirse será el último del combo y llamará a la función
    public void OnEndAttack() 
    {
        // Se establecen parametros
        isAttacking = false; 
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
            int damage = comboDamages[currentAttack - 1];

            // El enemigo recibe daño
            enemy.TakeDamage(damage);
        }
    }

    #endregion

    #region RollManagement

    // Ejecutar Esquive
    void Roll()
    {
        // Solo ejecutamos si tenemos permiso
        if (!canNextAction) return;
        
        // Solo ejecutamos si tenemos stamina suficiente
        if (stamina < rollStamCost) return;

        // Estado de esquivar
        isRolling = true;

        // Negamos más acciones
        canNextAction = false;

        // Consumo de stamina
        ConsumeStamina(rollStamCost);

        // Reproducimos la animación de esquivar
        anim.RollAnimation();
    }
    
    // Al final de la animación de esquivar
    void OnEndRoll()
    {
        // Estado de esquivar apagado
        isRolling = false;

        CanInterrupt(); // Considerar mover
    }

    // Habilitar y deshabilitar la Hitbox del jugador
    public void ManageHitBox(string state)
    {
        // El estado de invencibilidad dependerá de la actividad teórica de la Hitbox
        isInvincible = state == "off" ? true : false;
    }

    // Habilitar o deshabilitar estado de impulso durante el esquive
    public void ManageImpulse(string state)
    {
        rollImpulse = state == "on" ? true : false;
    }

    #endregion

    #region HealManagement

    // Ejecutar curación
    void Heal()
    {
        // Solo ejecutamos si tenemos permiso
        if (!canNextAction) return;

        // Estado de curación
        isHealing = true;

        // Negamos más acciones
        canNextAction = false;

        // Cancelar fuerzas residuales del rb al realizar paradas
        move.CancelResidualMove();
        move.CancelResidualRot();

        // Reproducimos la animación de curación
        anim.HealAnimation();
    }

    // Restablecer la salud
    public void RestoreHealth()
    {
        health += healthRestored;
    }

    // Al acabar la animación de Curación
    void OnEndHeal()
    {
        // Estado de curación apagado
        isHealing = false;

        CanInterrupt(); // Considerar mover
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

    // Deshabilita o habilita estados de movimiento 
    public void ManageMovement(string lockState)
    {
        // Comprueba la animación que ha finalizado
        switch (lockState)
        {
            case "moveLock":
                move.moveLocked = true; // Bloquear movimiento
                break;
            case "moveUnlock":
                move.moveLocked = false; // Desbloquear movimiento
                break;
            case "rotLock":
                move.rotationLocked = true; // Bloquear rotación
                break;
            case "rotUnlock":
                move.rotationLocked = false; // Desbloquear rotación
                break;
            case "markLock":
                move.markEnemyLocked = true; // Bloquear marcado enemigo
                break;
            case "markUnlock":
                move.markEnemyLocked = false; // Desbloquear marcado enemigo
                break;
        }
    }

    // Permite leer el siguiente input en cierto punto de la animación
    public void CanInterrupt()
    {
        canNextAction = true;
    }

    // Notifica el fin de una animación específica
    public void OnEndAnimation(string animName)
    {
        // Comprueba la animación que ha finalizado
        switch (animName)
        {
            case "attack":
                OnEndAttack(); break; // Fin de animación de ataque
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

    // Lectura de input de curación
    public void OnHeal(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Heal();
        }
    }

    #endregion
}
