using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    // Components
    Rigidbody rb;
    NavMeshAgent agent;

    [Header("References")]
    [SerializeField] EnemyCombat combat; // Script de control de combate
    [SerializeField] List<Transform> patrolPoints; // Lista de puntos de patrullar
    Transform player;

    [Header("Stats")]
    [SerializeField] float chasingSpeed; // Velocidad a la que el enemigo persigue al Jugador
    [SerializeField] float patrolSpeed; // Velocidad a la que el enemigo patrulla
    [SerializeField] float rotationSmooth; // Suavizado de la rotaci�n hacia el Jugador
    [SerializeField] float attackRadius; // Distancia a la cual el enemigo ataca al jugador
    [SerializeField] float detectRadius; // Distancia a la cual el enemigo detecta al jugador
    [SerializeField] float lostRadius; // Distancia a la cual el enemigo pierde de vista al jugador
    [SerializeField] float pointReachThreshold; // Distancia desde un punto de patrulla a la cual el enemigo debe acercarse para cambiar a otro punto

    [Header("Conditional Values")]
    public bool isChasing; // Estado de persecuci�n del jugador
    public bool isPatrolling; // Estado de patrullar
    public bool returnPatrol; // Estado de volver a Patrullar
    int currentPatrolPoint; // Punto que se set� persiguiendo actualmente

    // Posiciones
    Vector3 target;

    void Start()
    {
        // Obtenemos referencias
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.Find("Player")?.transform;

        // Valores de inicio
        ChangeStates(true, false, false); // Comienza Patrullando
    }

    void Update()
    {
        // Acci�n que ejecutar� el enemigo seg�n la distancia del jugador
        DetectPlayer();

        // Sigue al jugador cuando se dan las condiciones
        FollowPlayer();

        // Mira hacia el Objetivo cuando lo requiere
        LookAtTarget();

        // Funciones durante estado de Patrullaje
        Patrolling();

        // Funciones cuando el Jugador est� en el area de ataque
        InAttackArea();
    }

    #region Detection&ChangeOfStates

    // Acci�n que ejecutar� el enemigo seg�n la distancia del jugador
    void DetectPlayer()
    {
        // Obtenemos a que distancia est� el jugador
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Si el jugador no est� en el area de detecci�n
        if (distanceToPlayer >= lostRadius && !isPatrolling)
        {
            ChangeStates(true, false, false); // Patrullando
        }
        else if (distanceToPlayer <= detectRadius && distanceToPlayer > attackRadius) // Si el jugador est� en el area de detecci�n
        {
            ChangeStates(false, true, false); // Persiguiendo
        }
        else if (distanceToPlayer <= attackRadius) // Si el jugador est� en el area de ataque
        {
            ChangeStates(false, false, true); // Atacando
        }
    }

    // Cambio de comportamiento del enemigo
    void ChangeStates(bool patrolling, bool chasing, bool attacking)
    {
        isPatrolling = patrolling;
        returnPatrol = patrolling; // Solo se activa al inicio del patrullaje
        isChasing = chasing;
        combat.isAttacking = attacking;
    }

    #endregion

    #region Patrolling

    // Funciones durante estado de Patrullaje
    void Patrolling()
    {
        // Solo ejecutamos cuando est� en estado de patrullar
        if (!isPatrolling) return;

        // Activamos la rotaci�n de NavMesh y se desactiva la manual
        agent.updateRotation = true; 

        // Establecemos la velocidad de patrullar
        agent.speed = patrolSpeed;

        // Calculamos distancia hacia el punto que debe alcanzar
        float distanceToPoint = Vector3.Distance(transform.position, patrolPoints[currentPatrolPoint].position);

        // Si llega al punto, cambia al siguiente
        if (distanceToPoint <= pointReachThreshold)
        {
            // Funci�n para cambiar al siguiente punto de la lista de puntos 
            currentPatrolPoint = (currentPatrolPoint + 1) % patrolPoints.Count;
        }

        // Al inicio buscamos punto de patrulla m�s cercano
        if (returnPatrol)
        {
            currentPatrolPoint = SearchClosestPatrol();
            returnPatrol = false;
        }

        // Establecemos el punto objetivo
        Vector3 targetPoint = patrolPoints[currentPatrolPoint].position;

        // Establecemos el jugador como Target
        SetTarget(targetPoint);
    }

    // Funci�n de b�squeda de punto de patrulla m�s cercano
    int SearchClosestPatrol()
    {
        // Variables para almacenar el punto m�s cercano
        int closestPoint = 0;
        float closestDistance = Mathf.Infinity;

        // Comparamos distancias entre enemigo y puntos
        for (int currentPoint = 0; currentPoint < patrolPoints.Count; currentPoint++)
        {
            // Obtenemos a que distancia est� el punto
            float distanceToPoint = Vector3.Distance(transform.position, patrolPoints[currentPoint].position);

            // Nos quedamos con el punto mas cercano
            if (distanceToPoint < closestDistance)
            {
                closestDistance = distanceToPoint;
                closestPoint = currentPoint;
            }
        }

        // Devolvemos el punto m�s cercano
        return closestPoint;
    }

    #endregion

    #region PlayerInteraction

    // Funci�n de perseguir al jugador
    void FollowPlayer()
    {
        // Solo ejecutamos cuando est� en estado de perseguir
        if (!isChasing) return;

        // Activamos la rotaci�n de NavMesh y se desactiva la manual
        agent.updateRotation = true;

        // Establecemos la velocidad de persecuci�n
        agent.speed = chasingSpeed;

        // Establecemos el jugador como Target
        SetTarget(player.position);
    }

    // Funciones cuando el Jugador est� en el area de ataque
    void InAttackArea()
    {
        // Solo ejecutamos en estado de ataque
        if (!combat.isAttacking) return;

        // Desactivamos la rotaci�n de NavMesh para activar la manual
        agent.updateRotation = false;

        // Negamos la velocidad para que no avance, pero puede seguir girando para mirar hacia el Jugador
        agent.speed = 0;

        // Establecemos el jugador como Target
        SetTarget(player.position);
    }

    #endregion

    #region TargetSettings
    // Se llama para establecer el objetivo del enemigo
    void SetTarget(Vector3 newTarget)
    {
        target = newTarget;
        agent.SetDestination(target);
    }

    // Mira hacia el Jugador cuando lo requiere
    void LookAtTarget()
    {
        // Rotaci�n manual hacia el target solo en estado de ataque y si no est� negada
        if (combat.rotationLocked || !combat.isAttacking) return;

        // Calculamos la direcci�n hacia el objetivo
        Vector3 directionToTarget = target - transform.position;

        // Mantener solo la direcci�n en el plano X-Z, eliminando cualquier inclinaci�n en Y
        directionToTarget.y = 0;

        // Si la direcci�n es v�lida (evitar errores al normalizar un vector cero)
        if (directionToTarget.sqrMagnitude > 0)
        {
            // Calculamos la rotaci�n deseada solo en el eje Y
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

            // Suavizamos la rotaci�n
            Quaternion smoothedRotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmooth * Time.deltaTime);

            // Aplicamos la rotaci�n suavizada
            transform.rotation = smoothedRotation;
        }
    }

    #endregion

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

        // Radio de alcance de puntos de Patrulla
        for (int currentPoint = 0; currentPoint < patrolPoints.Count; currentPoint++)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(patrolPoints[currentPoint].position, pointReachThreshold);
        }
    }

    #endregion

}
