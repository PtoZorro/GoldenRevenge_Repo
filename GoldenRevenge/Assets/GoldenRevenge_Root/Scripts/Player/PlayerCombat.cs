using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("External References")]
    [SerializeField] GameObject weaponCollider; // Hitbox del arma del jugador
    [SerializeField] Transform weapon; // Posici�n del arma en el Rig

    [Header("Conditional Values")]
    public bool isAttacking; // Valor que indica que estamos atacando
    public bool rotationLocked; // Valor que indica que nos se puede rotar
    bool colliderActive; // Valor que indica que la HitBox del arma est� activa
    public int maxAttacksInCombo; // N�mero m�ximo de ataques en un mismo combo
    public int attackCount; // N�mero de ataques que realizar� del combo

    // Posiciones
    Vector3 colliderInitialPos;
    Quaternion colliderInitialRot;

    void Start()
    {
        // Valores de inicio
        isAttacking = false;
        attackCount = 0;
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
        // Sumamos un ataque m�s del combo si no hemos llegado al m�ximo
        if (attackCount < maxAttacksInCombo)
        {
            attackCount++; // Sumamos n�mero de ataque
            isAttacking = true; // Estado de ataque
        }

        // Nunca menor que 0 el numero de ataques
        if (attackCount <= 0) attackCount = 0;
    }

    public void EndAttack(int attackNum) // Al acabar animaci�n quitamos estado de ataque o pasamos al siguiente del combo
    {
        // Si es el �ltimo ataque o hemos terminado de atacar, liberamos todo
        if (attackNum >= attackCount || attackNum >= maxAttacksInCombo) 
        {
            isAttacking = false; // Se libera estado de ataque
            attackCount = 0; // Se resetea en contador de ataques
        }

        rotationLocked = false; // se libera la rotaci�n de personaje
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
