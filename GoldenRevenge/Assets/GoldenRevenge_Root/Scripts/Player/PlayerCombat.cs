using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("External References")]
    [SerializeField] GameObject weaponCollider; // Hitbox del arma del jugador
    [SerializeField] Transform weapon; // Posición del arma en el Rig

    [Header("Conditional Values")]
    public bool isAttacking; // Valor que indica que estamos atacando
    public bool rotationLocked; // Valor que indica que nos se puede rotar
    bool colliderActive; // Valor que indica que la HitBox del arma está activa
    public int maxAttacksInCombo; // Número máximo de ataques en un mismo combo
    public int attackCount; // Número de ataques que realizará del combo

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

    void Attack() // Gestiona el número de ataques y los ejecuta
    {
        // Sumamos un ataque más del combo si no hemos llegado al máximo
        if (attackCount < maxAttacksInCombo)
        {
            attackCount++; // Sumamos número de ataque
            isAttacking = true; // Estado de ataque
        }

        // Nunca menor que 0 el numero de ataques
        if (attackCount <= 0) attackCount = 0;
    }

    public void EndAttack(int attackNum) // Al acabar animación quitamos estado de ataque o pasamos al siguiente del combo
    {
        // Si es el último ataque o hemos terminado de atacar, liberamos todo
        if (attackNum >= attackCount || attackNum >= maxAttacksInCombo) 
        {
            isAttacking = false; // Se libera estado de ataque
            attackCount = 0; // Se resetea en contador de ataques
        }

        rotationLocked = false; // se libera la rotación de personaje
    }

    void FollowWeapon()
    {
        // Siempre seguirá al Rig solo cuando está activo
        if (colliderActive) 
        {
            weaponCollider.transform.position = weapon.position;
            weaponCollider.transform.rotation = weapon.rotation;
        }
    }

    public void EnableCollider() // Método para habilitar el colisionador de las armas mediante la animación
    {
        weaponCollider.SetActive(true);
        colliderActive = true;
    }

    public void DisableCollider() // Método para deshabilitar el colisionador de las armas mediante la animación
    {
        weaponCollider.SetActive(false);
        colliderActive = false;

        // Lo llevamos al punto de inicio
        weaponCollider.transform.localPosition = colliderInitialPos;
        weaponCollider.transform.localRotation = colliderInitialRot;
    }

    public void LockRotation() // Deshabilita la rotación en el momento de inpacto
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
