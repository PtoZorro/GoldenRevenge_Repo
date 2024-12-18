using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static IAnimationEvents;

public class EnemyCombat : MonoBehaviour, IAnimationEvents, IGeneralStatesEvents, IAttackEvents // Implementar interfaz de eventos de animación
{
    [Header("References")]
    EnemyMovement move; // Script de control de movimiento
    EnemyAnimations anim; // Script de control de animaciones
    [SerializeField] GameObject lockCamPoint; // Referencia al punto de fijado en cámara del enemigo
    [SerializeField] GameObject weaponCollider; // Hitbox del arma del enemigo
    [SerializeField] Transform weapon; // Posición del arma en el Rig

    [Header("Stats")]
    [SerializeField] int health; // Salud del enemigo
    [SerializeField] int maxHealth; // Salud máxima
    [SerializeField] int[] comboDamages; // Daños para cada ataque del combo
    [SerializeField] int maxComboAttacks; // Número máximo de ataques en un mismo combo
    [SerializeField] float attackRate; // Tiempo en que se permite accionar otro combo de ataques

    [Header("Conditional Values")]
    public bool isAttacking; // Estado de atacando
    bool colliderActive; // Valor que indica que la HitBox del arma está activa
    [SerializeField] bool canNextAction; // Se permite ejecutar la próxima acción
    bool canDealDamage; // Evitamos ejercer daño más de una vez por ataque ejecutado
    [SerializeField] int currentAttack; // El ataque que se ejecutará, mandado por el input

    // Posiciones
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
        // Referencias
        move = GetComponent<EnemyMovement>();
        anim = GetComponent<EnemyAnimations>();

        // Valores de inicio
        health = maxHealth;
        canNextAction = true;
        currentAttack = 0;

        // En el inicio los colliders de armas empiezan apagados
        weaponCollider.SetActive(false);
    }

    void Update()
    {
        // Gestión de la salud
        HealthManagement();

        // Gestión de ataques
        Attack();

        // Mantener el collider siguiendo al arma en el Rig
        FollowWeapon();
    }

    #region HealthManagement

    // Gestión de la salud
    void HealthManagement()
    {
        // Si se queda sin salud
        if (health <= 0)
        {
            // Desactivamos (provisional)
            lockCamPoint.SetActive(false);
            gameObject.SetActive(false);
        }
    }

    // Recibir daño
    public void TakeDamage(int damageRecived)
    {
        // Restamos daño especificado
        health -= damageRecived;
    }

    #endregion

    #region AttackManagement

    // Gestiona cual es el próximo ataque y lo ejecuta
    void Attack()
    {
        // Solo accionamos si tenemos permiso de atacar y no ha terminado el combo
        if (!isAttacking || !canNextAction || currentAttack >= maxComboAttacks) return;

        // Puede hacer daño
        canDealDamage = true;

        // Negamos más acciones
        CanInterrupt("can't");

        // Indicamos el ataque que toca ejecutar
        currentAttack++;

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

    // Permite volver a iniciar un combo pasado un timepo "attackRate"
    public void AllowAttack()
    {
        canNextAction = true;
    }

    // Hacemos daño al Jugador mediante la colisión
    public void InflictDamage(PlayerCombat player)
    {
        // Solo si se permite ejercer daño
        if (canDealDamage)
        {
            // Evitamos ejercer daño más de una vez por ataque
            canDealDamage = false;

            // Seleccionamos el daño del ataque que se está ejecutando
            int damage = comboDamages[currentAttack - 1];

            // El enemigo recibe daño
            player.TakeDamage(damage);
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
        }
    }

    // Permite leer el siguiente input en cierto punto de la animación
    public void CanInterrupt(string canInterrupt)
    {
        canNextAction = canInterrupt == "can" ? true : false;
    }

    // Notifica el fin de una animación específica
    public void OnEndAnimation(string animName)
    {
        // Comprueba la animación que ha finalizado
        switch (animName)
        {
            case "attack":
                OnEndAttack(); break; // Fin de animación de ataque
        }
    }

    #endregion
}
