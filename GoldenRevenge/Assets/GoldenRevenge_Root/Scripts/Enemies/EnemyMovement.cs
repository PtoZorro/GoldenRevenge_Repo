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
    [SerializeField] float rotationSmooth; // Suavizado de la rotación hacia el Player
    [SerializeField] float attackRadius; // Distancia a la cual el enemigo ataca al jugador
    [SerializeField] float detectRadius; // Distancia a la cual el enemigo detecta al jugador
    [SerializeField] float lostRadius; // Distancia a la cual el enemigo pierde de vista al jugador

    [Header("Conditional Values")]
    public bool isChasing; // Estado de persecución del jugador
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
        
        // Patrulla cuando no hay detección
        Patrolling();

        // Funciones cuando el Player está en el area de ataque
        InAttackArea();

        // Mira hacia el Player cuando lo requiere
        LookAtPlayer();
    }

    void DetectPlayer()
    {
        // Obtenemos a que distancia está el jugador
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Si el jugador está en el area de detección
        if (distanceToPlayer <= detectRadius && distanceToPlayer > attackRadius)
        {
            isChasing = true;
            isPatrolling = false;
            combat.isAttacking = false;
        }
        else if (distanceToPlayer >= lostRadius) // Si el jugador se ha alejado hasta el rango de pérdida
        {
            isChasing = false;
            isPatrolling = true;
            combat.isAttacking = false;
        }
        else if (distanceToPlayer <= attackRadius) // Si el jugador está en el area de ataque
        {
            isChasing = false;
            isPatrolling = false;
            combat.isAttacking = true;
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
        // Si negamos la rotación no ejecutamos
        if (combat.rotationLocked) return;

        // Calculamos la dirección hacia el jugador
        Vector3 directionToPlayer = player.position - transform.position;

        // Mantener solo la dirección en el plano X-Z, eliminando cualquier inclinación en Y
        directionToPlayer.y = 0;

        // Si la dirección es válida (evitar errores al normalizar un vector cero)
        if (directionToPlayer.sqrMagnitude > 0)
        {
            // Calculamos la rotación deseada solo en el eje Y
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);

            // Suavizamos la rotación
            Quaternion smoothedRotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmooth * Time.deltaTime);

            // Aplicamos la rotación suavizada
            transform.rotation = smoothedRotation;
        }
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
