using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    // Components
    Rigidbody rb;
    NavMeshAgent agent;

    [Header("References")]
    [SerializeField] EnemyCombat combat;
    Transform player;
    
    [Header("Stats")]
    [SerializeField] float chasingSpeed; // Velocidad a la que el enemigo persigue al Player
    [SerializeField] float patrolSpeed; // Velocidad a la que el enemigo patrulla
    [SerializeField] float rotationSmooth; // Suavizado de la rotaci�n hacia el Player
    [SerializeField] float attackRadius; // Distancia a la cual el enemigo ataca al jugador
    [SerializeField] float detectRadius; // Distancia a la cual el enemigo detecta al jugador
    [SerializeField] float lostRadius; // Distancia a la cual el enemigo pierde de vista al jugador

    [Header("Conditional Values")]
    public bool isChasing; // Estado de persecuci�n del jugador
    public bool isPatrolling; // Estado de patrullar

    void Start()
    {
        // Obtenemos referencias
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.Find("Player")?.transform;

        // Valores de inicio
        agent.updateRotation = false;
    }

    void Update()
    {
        // Monitoreamos la distancia al jugador constantemente
        DetectPlayer();

        // Sigue al jugador cuando se dan las condiciones
        FollowPlayer();
        
        // Patrulla cuando no hay detecci�n
        Patrolling();

        // Funciones cuando el Player est� en el area de ataque
        InAttackArea();

        // Mira hacia el Player cuando lo requiere
        LookAtPlayer();
    }

    void DetectPlayer()
    {
        // Obtenemos a que distancia est� el jugador
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Si el jugador est� en el area de detecci�n
        if (distanceToPlayer <= detectRadius && distanceToPlayer > attackRadius)
        {
            isChasing = true;
            isPatrolling = false;
            combat.isAttacking = false;
        }
        else if (distanceToPlayer >= lostRadius) // Si el jugador se ha alejado hasta el rango de p�rdida
        {
            isChasing = false;
            isPatrolling = true;
            combat.isAttacking = false;
        }
        else if (distanceToPlayer <= attackRadius) // Si el jugador est� en el area de ataque
        {
            isChasing = false;
            isPatrolling = false;
            combat.isAttacking = true;
        }
    }

    // Funci�n de perseguir al jugador
    void FollowPlayer()
    {
        // Solo ejecutamos cuando est� en estado de perseguir
        if (!isChasing) return;

        // Establecemos al jugador como el destino
        agent.SetDestination(player.position);

        // Establecemos la velocidad de persecuci�n
        agent.speed = chasingSpeed;
    }

    void Patrolling()
    {
        // Solo ejecutamos cuando est� en estado de perseguir
        if (!isPatrolling) return;

        // Establecemos al jugador como el destino
        agent.SetDestination(transform.position); // ���Provisional!!!

        // Establecemos la velocidad de persecuci�n
        agent.speed = patrolSpeed;
    }

    void InAttackArea()
    {
        // Solo ejecutamos en estado de ataque
        if (!combat.isAttacking) return;

        // Establecemos al jugador como el destino
        agent.SetDestination(player.position);

        // Negamos la velocidad para que no avance, pero puede seguir girando para mirar hacia el Player
        agent.speed = 0;
    }

    void LookAtPlayer()
    {
        // Si negamos la rotaci�n no ejecutamos
        if (combat.rotationLocked) return;

        // Calculamos la direcci�n hacia el jugador
        Vector3 directionToPlayer = player.position - transform.position;

        // Mantener solo la direcci�n en el plano X-Z, eliminando cualquier inclinaci�n en Y
        directionToPlayer.y = 0;

        // Si la direcci�n es v�lida (evitar errores al normalizar un vector cero)
        if (directionToPlayer.sqrMagnitude > 0)
        {
            // Calculamos la rotaci�n deseada solo en el eje Y
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);

            // Suavizamos la rotaci�n
            Quaternion smoothedRotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmooth * Time.deltaTime);

            // Aplicamos la rotaci�n suavizada
            transform.rotation = smoothedRotation;
        }
    }


    #region Depuration

    // Los radios de detecci�n se visualizan en el editor a modo de depuraci�n
    void OnDrawGizmosSelected()
    {
        // Radio de ataque
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);

        // Radio de detecci�n
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRadius);

        // Radio de p�rdida
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, lostRadius);
    }

    #endregion

}
