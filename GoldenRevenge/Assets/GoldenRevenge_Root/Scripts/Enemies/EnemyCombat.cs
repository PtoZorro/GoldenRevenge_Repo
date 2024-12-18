using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static IAnimationEvents;

public class EnemyCombat : MonoBehaviour, IAnimationEvents, IGeneralStatesEvents, IAttackEvents // Implementar interfaz de eventos de animaci�n
{
    [Header("References")]
    EnemyMovement move; // Script de control de movimiento
    EnemyAnimations anim; // Script de control de animaciones
    [SerializeField] GameObject lockCamPoint; // Referencia al punto de fijado en c�mara del enemigo
    [SerializeField] GameObject weaponCollider; // Hitbox del arma del enemigo
    [SerializeField] Transform weapon; // Posici�n del arma en el Rig

    [Header("Stats")]
    [SerializeField] int health; // Salud del enemigo
    [SerializeField] int maxHealth; // Salud m�xima
    [SerializeField] int[] comboDamages; // Da�os para cada ataque del combo
    [SerializeField] int maxComboAttacks; // N�mero m�ximo de ataques en un mismo combo
    [SerializeField] float attackRate; // Tiempo en que se permite accionar otro combo de ataques

    [Header("Conditional Values")]
    public bool isAttacking; // Estado de atacando
    bool colliderActive; // Valor que indica que la HitBox del arma est� activa
    [SerializeField] bool canNextAction; // Se permite ejecutar la pr�xima acci�n
    bool canDealDamage; // Evitamos ejercer da�o m�s de una vez por ataque ejecutado
    [SerializeField] int currentAttack; // El ataque que se ejecutar�, mandado por el input

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
        // Gesti�n de la salud
        HealthManagement();

        // Gesti�n de ataques
        Attack();

        // Mantener el collider siguiendo al arma en el Rig
        FollowWeapon();
    }

    #region HealthManagement

    // Gesti�n de la salud
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

    // Recibir da�o
    public void TakeDamage(int damageRecived)
    {
        // Restamos da�o especificado
        health -= damageRecived;
    }

    #endregion

    #region AttackManagement

    // Gestiona cual es el pr�ximo ataque y lo ejecuta
    void Attack()
    {
        // Solo accionamos si tenemos permiso de atacar y no ha terminado el combo
        if (!isAttacking || !canNextAction || currentAttack >= maxComboAttacks) return;

        // Puede hacer da�o
        canDealDamage = true;

        // Negamos m�s acciones
        CanInterrupt("can't");

        // Indicamos el ataque que toca ejecutar
        currentAttack++;

        // Reproducimos la animaci�n de ataque correspondiente
        anim.AttackAnimations(currentAttack);
    }

    // El ataque que termine de reproducirse ser� el �ltimo del combo y llamar� a la funci�n
    public void OnEndAttack()
    {
        // Se establecen parametros
        isAttacking = false;
        canNextAction = false;
        currentAttack = 0;

        // Pasado un tiempo configurable se podr� iniciar otro ataque
        Invoke(nameof(AllowAttack), attackRate);
    }

    // Permite volver a iniciar un combo pasado un timepo "attackRate"
    public void AllowAttack()
    {
        canNextAction = true;
    }

    // Hacemos da�o al Jugador mediante la colisi�n
    public void InflictDamage(PlayerCombat player)
    {
        // Solo si se permite ejercer da�o
        if (canDealDamage)
        {
            // Evitamos ejercer da�o m�s de una vez por ataque
            canDealDamage = false;

            // Seleccionamos el da�o del ataque que se est� ejecutando
            int damage = comboDamages[currentAttack - 1];

            // El enemigo recibe da�o
            player.TakeDamage(damage);
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

    // Deshabilita o habilita estados de movimiento 
    public void ManageMovement(string lockState)
    {
        // Comprueba la animaci�n que ha finalizado
        switch (lockState)
        {
            case "moveLock":
                move.moveLocked = true; // Bloquear movimiento
                break;
            case "moveUnlock":
                move.moveLocked = false; // Desbloquear movimiento
                break;
            case "rotLock":
                move.rotationLocked = true; // Bloquear rotaci�n
                break;
            case "rotUnlock":
                move.rotationLocked = false; // Desbloquear rotaci�n
                break;
        }
    }

    // Permite leer el siguiente input en cierto punto de la animaci�n
    public void CanInterrupt(string canInterrupt)
    {
        canNextAction = canInterrupt == "can" ? true : false;
    }

    // Notifica el fin de una animaci�n espec�fica
    public void OnEndAnimation(string animName)
    {
        // Comprueba la animaci�n que ha finalizado
        switch (animName)
        {
            case "attack":
                OnEndAttack(); break; // Fin de animaci�n de ataque
        }
    }

    #endregion
}
