using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    // Components
    NavMeshAgent agent;

    [Header("References")]
    Transform player; 

    [Header("Stats")]
    [SerializeField] float chasingSpeed; // Velocidad a la que el enemigo persigue al Player
    [SerializeField] float patrolSpeed; // Velocidad a la que el enemigo patrulla
    [SerializeField] float attackRadius; // Distancia a la cual el enemigo ataca al jugador
    [SerializeField] float detectRadius; // Distancia a la cual el enemigo detecta al jugador
    [SerializeField] float lostRadius; // Distancia a la cual el enemigo pierde de vista al jugador

    [Header("Conditional Values")]
    public bool isChasing; // Estado de persecución del jugador
    public bool isPatrolling; // Estado de patrullar

    void Start()
    {
        // Obtenemos referencias
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.Find("Player")?.transform;
    }

    void Update()
    {
        // Monitoreamos la distancia al jugador constantemente
        DetectPlayer();

        // Sigue al jugador cuando se dan las condiciones
        FollowPlayer();
        
        // Patrulla cuando no hay detección
        Patrolling();
    }

    void DetectPlayer()
    {
        // Obtenemos a que distancia está el jugador
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Si el jugador está en el area de detección
        if (distanceToPlayer <= detectRadius)
        {
            isChasing = true;
            isPatrolling = false;
        }
        else if (distanceToPlayer >= lostRadius) // Si el jugador se ha alejado del rango de pérdida
        {
            isChasing = false;
            isPatrolling = true;
        }
    }

    // Función de perseguir al jugador
    void FollowPlayer()
    {
        // Solo ejecutamos cuando esté en estado de perseguir
        if (!isChasing) return;

        // Establecemos al jugador como el destino
        agent.SetDestination(player.position);

        // Establecemos la velocidad de persecución
        agent.speed = chasingSpeed;
    }

    void Patrolling()
    {
        // Solo ejecutamos cuando esté en estado de perseguir
        if (!isPatrolling) return;

        // Establecemos al jugador como el destino
        agent.SetDestination(transform.position); // ¡¡¡Provisional!!!

        // Establecemos la velocidad de persecución
        agent.speed = chasingSpeed;
    }

    #region Depuration

    // Los radios de detección se visualizan en el editor a modo de depuración
    void OnDrawGizmosSelected()
    {
        // Radio de ataque
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);

        // Radio de detección
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRadius);

        // Radio de pérdida
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, lostRadius);
    }

    #endregion
}
