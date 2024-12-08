using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("External References")]
    [SerializeField] GameObject weaponCollider; // Hitbox del arma del jugador
    [SerializeField] Transform weapon; // Posición del arma en el Rig

    // Valores condicionales
    public bool isAttacking; // Valor que indica que estamos atacando
    public bool rotationLocked; // Valor que indica que nos se puede rotar
    bool colliderActive; // Valor que indica que la HitBox del arma está activa

    // Posiciones
    Vector3 colliderInitialPos;
    Quaternion colliderInitialRot;

    void Start()
    {
        // Valores de inicio
        isAttacking = false;
        rotationLocked = false;
        colliderActive = false;
        colliderInitialPos = transform.position;
        colliderInitialRot = transform.rotation;

        // En el inicio los colliders de armas empiezan apagados
        weaponCollider.SetActive(false);
    }

    void Update()
    {
        // Mantener el collider siguiendo al arma en el Rig
        FollowWeapon();
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
        weaponCollider.transform.position = colliderInitialPos;
        weaponCollider.transform.rotation = colliderInitialRot;
    }

    public void LockRotation() // Deshabilita la rotación en el momento de inpacto
    {
        rotationLocked = true;
    }

    public void OnAttack(InputAction.CallbackContext context) // Lectura de input de ataque
    {
        if (context.started)
        {
            isAttacking = true;
        }
    }
}
