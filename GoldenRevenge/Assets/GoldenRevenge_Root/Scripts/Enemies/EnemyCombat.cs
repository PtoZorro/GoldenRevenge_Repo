using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static IAnimationEvents;

public class EnemyCombat : MonoBehaviour, IAnimationEvents, IGeneralStatesEvents, IAttackEvents // Implementar interfaz de eventos de animación
{
    [Header("References")]
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
    public bool rotationLocked; // Negación de rotación
    bool colliderActive; // Valor que indica que la HitBox del arma está activa
    bool canNextAction; // Se permite ejecutar la próxima acción
    bool canDealDamage; // Evitamos ejercer daño más de una vez por ataque ejecutado
    int currentAttack; // El ataque que se ejecutará, mandado por el input
    int attackNum; // El ataque que se está ejecutando, indicado por su animación

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
        anim = GetComponent<EnemyAnimations>();

        // Valores de inicio
        health = maxHealth;
        isAttacking = false;
        currentAttack = 0;
        canNextAction = true;
        rotationLocked = false;
        colliderActive = false;

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
        // Solo atacamos si estamos en el estado
        if (!isAttacking) return;

        // Solo accionamos si hay permiso de atacar y no ha terminado el combo
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
            int damage = comboDamages[attackNum - 1];

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

    // Notifica el inicio de una animación específica
    public void OnStartAnimation(string animName)
    {
        
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

    // Permite pasar al siguiente estado llegado a punto de la animación
    public void CanInterrupt()
    {
        canNextAction = true;
    }

    // Deshabilita o habilita la rotación 
    public void ManageRotation(string lockState)
    {
        rotationLocked = lockState == "lock" ? true : false;
    }

    #endregion
}
