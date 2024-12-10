using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Components")]
    PlayerAnimations anim; // Script de control de animaciones

    [Header("External References")]
    [SerializeField] GameObject weaponCollider; // Hitbox del arma del jugador
    [SerializeField] Transform weapon; // Posición del arma en el Rig

    [Header("Stats")]
    [SerializeField] int maxComboAttacks; // Número máximo de ataques en un mismo combo
    [SerializeField] float attackRate; // Tiempo en que se nos permite accionar otro combo de ataques

    [Header("Conditional Values")]
    public bool isAttacking; // Valor que indica que estamos atacando
    public bool rotationLocked; // Valor que indica que nos se puede rotar
    bool colliderActive; // Valor que indica que la HitBox del arma está activa
    bool canNextAction; // Se permite o no acumular ataques al pulsar varias veces el input
    int currentAttack; // Número de ataques que realizará del combo

    // Posiciones
    Vector3 colliderInitialPos;
    Quaternion colliderInitialRot;

    void Awake()
    {
        // Valores de inicio prioritarios
        colliderInitialPos = transform.localPosition;
        colliderInitialRot = transform.localRotation;
    }

    void Start()
    {
        // Referencias
        anim = GetComponent<PlayerAnimations>();

        // Valores de inicio
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
        // Mantener el collider siguiendo al arma en el Rig
        FollowWeapon();
    }

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
    public void OnStartAttack() 
    {
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

    #endregion

    #region WeaponHitboxManagement

    // Mantener el collider siguiendo al arma en el Rig
    void FollowWeapon()
    {
        // Seguirá al Rig solo cuando el collider está activo
        if (colliderActive)
        {
            weaponCollider.transform.position = weapon.position;
            weaponCollider.transform.rotation = weapon.rotation;
        }
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
