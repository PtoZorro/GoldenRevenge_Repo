using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Components")]
    PlayerAnimations anim;

    [Header("External References")]
    [SerializeField] GameObject weaponCollider; // Hitbox del arma del jugador
    [SerializeField] Transform weapon; // Posici�n del arma en el Rig

    [Header("Conditional Values")]
    public bool isAttacking; // Valor que indica que estamos atacando
    public bool rotationLocked; // Valor que indica que nos se puede rotar
    bool colliderActive; // Valor que indica que la HitBox del arma est� activa
    [SerializeField] bool canNextAttack; // Se permite o no acumular ataques al pulsar varias veces el input
    public int maxComboAttacks; // N�mero m�ximo de ataques en un mismo combo
    public int currentAttack; // N�mero de ataques que realizar� del combo

    // Posiciones
    Vector3 colliderInitialPos;
    Quaternion colliderInitialRot;

    void Start()
    {
        // Referencias
        anim = GetComponent<PlayerAnimations>();

        // Valores de inicio
        isAttacking = false;
        currentAttack = 0;
        canNextAttack = true;
        rotationLocked = false;
        colliderActive = false;
        colliderInitialPos = transform.localPosition;
        colliderInitialRot = transform.localRotation;

        // En el inicio los colliders de armas empiezan apagados
        weaponCollider.SetActive(false);
    }

    void Update()
    {
        // Mantener el collider siguiendo al arma en el Rig
        FollowWeapon();
    }

    void Attack() // Gestiona el n�mero de ataques y los ejecuta
    {
        // Si es el primer ataque del combo o se nos permite activar el siguiente, asignamos ataque que se producir�
        if (currentAttack == 0 || canNextAttack)
        {
            currentAttack++; // Proximo ataque
            isAttacking = true; // Estado de ataque
            canNextAttack = false; // Negamos siguiente ataque hasta que no lo indique el actual

            // Reproducimos la animaci�n de ataque correspondiente
            anim.AttackAnimations(currentAttack);
        }
    }

    public void EndAttack(int attackNum) // Al acabar animaci�n quitamos estado de ataque o pasamos al siguiente del combo
    {
        // Si el ataque ejecutado es el �ltimo que se pretende ejecutar o es final de combo, se acaba el estado de ataque
        if (attackNum >= currentAttack || attackNum == maxComboAttacks) 
        {
            currentAttack = 0; // Contador de ataques a 0
            isAttacking = false; 
            canNextAttack = false;
        }
        // Desbloqueamos rotaci�n siempre al acabar animaci�n
        rotationLocked = false;
    }

    public void CanReadNextAction() // En cierto punto de la animaci�n se puede leer el input de la siguiente acci�n
    {
        canNextAttack = true;
    }

    void FollowWeapon()
    {
        // Siempre seguir� al Rig solo cuando est� activo
        if (colliderActive) 
        {
            weaponCollider.transform.position = weapon.position;
            weaponCollider.transform.rotation = weapon.rotation;
        }
    }

    public void EnableCollider() // M�todo para habilitar el colisionador de las armas mediante la animaci�n
    {
        weaponCollider.SetActive(true);
        colliderActive = true;
    }

    public void DisableCollider() // M�todo para deshabilitar el colisionador de las armas mediante la animaci�n
    {
        weaponCollider.SetActive(false);
        colliderActive = false;

        // Lo llevamos al punto de inicio
        weaponCollider.transform.localPosition = colliderInitialPos;
        weaponCollider.transform.localRotation = colliderInitialRot;
    }

    public void LockRotation() // Deshabilita la rotaci�n en el momento de inpacto
    {
        rotationLocked = true;
    }

    public void OnAttack(InputAction.CallbackContext context) // Lectura de input de ataque
    {
        if (context.started)
        {
            Attack();
        }
    }
}
