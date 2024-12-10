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
    [SerializeField] Transform weapon; // Posici�n del arma en el Rig

    [Header("Stats")]
    [SerializeField] int maxComboAttacks; // N�mero m�ximo de ataques en un mismo combo
    [SerializeField] float attackRate; // Tiempo en que se nos permite accionar otro combo de ataques

    [Header("Conditional Values")]
    public bool isAttacking; // Valor que indica que estamos atacando
    public bool rotationLocked; // Valor que indica que nos se puede rotar
    bool colliderActive; // Valor que indica que la HitBox del arma est� activa
    bool canNextAction; // Se permite o no acumular ataques al pulsar varias veces el input
    int currentAttack; // N�mero de ataques que realizar� del combo

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
    public void OnStartAttack() 
    {
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

    #endregion

    #region WeaponHitboxManagement

    // Mantener el collider siguiendo al arma en el Rig
    void FollowWeapon()
    {
        // Seguir� al Rig solo cuando el collider est� activo
        if (colliderActive)
        {
            weaponCollider.transform.position = weapon.position;
            weaponCollider.transform.rotation = weapon.rotation;
        }
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
