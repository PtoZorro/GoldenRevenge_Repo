using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("External References")]
    [SerializeField] GameObject weaponCollider;
    [SerializeField] Transform weapon;

    // Valores condicionales
    bool colliderActive;

    // Posiciones
    Vector3 colliderInitialPos;
    Quaternion colliderInitialRot;

    void Start()
    {
        // Valores de inicio
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

    void EnableCollider() // Método para habilitar el colisionador de las armas mediante la animación
    {
        weaponCollider.SetActive(true);
        colliderActive = true;
    }

    void DisableCollider() // Método para deshabilitar el colisionador de las armas mediante la animación
    {
        weaponCollider.SetActive(false);
        colliderActive = false;

        // Lo llevamos al punto de inicio
        weaponCollider.transform.position = colliderInitialPos;
        weaponCollider.transform.rotation = colliderInitialRot;
    }
}
