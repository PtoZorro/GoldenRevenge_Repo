using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockPointBehavior : MonoBehaviour
{
    // En este script manejamos la altura del "punto de marcado del enemigo"
    // El punto de marcado es en el que la cámara se fijará automáticamente cuando fijemos un enemigo
    // La altura del punto variará según lo cercano que esté el player del enemigo,
    // ya que así ajustamos desde que ángulo se verá al enemigo según la distancia, para que el player no lo tape

    [Header("References")]
    Transform playerPos;

    [Header("Settings")]
    [SerializeField] float farPointHeight; // Altura del punto de marcado cuando el jugador está lejos 
    [SerializeField] float riseHeight; // Altura que se suma a la variable anterior según el player se acerque
    [SerializeField] float minDistance; // Distancia mínima del player al enemigo a la que el punto comienza a subir
    [SerializeField] float riseSpeed; // Velocidad a la que el punto se moverá

    void Start()
    {
        // Encontramos al jugador en la escena 
        playerPos = GameObject.Find("Player")?.transform;
    }

    void Update()
    {
        // Calculamos la distancia entre el objeto y el jugador
        float distanceToPlayer = Vector3.Distance(transform.position, playerPos.position);

        // Si la distancia es menor que la mínima
        if (distanceToPlayer < minDistance)
        {
            // Proporcionalidad de la subida, más rápido conforme esté más cerca
            // Esta función nos da un valor entre 0 (fuera de rango o en el punto más cercano) y 1 (justo al lado del jugador)
            float t = 1 - Mathf.Clamp01(distanceToPlayer / minDistance);

            // Calculamos la nueva posición vertical del objeto interpolando entre la posición más baja y la más alta posible según t (cercania del player entre 0-1)
            float newYPosition = Mathf.Lerp(farPointHeight, farPointHeight + riseHeight, t);

            // Subimos el objeto con una velocidad limitada
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, newYPosition, transform.position.z), riseSpeed * Time.deltaTime);
        }
        else
        {
            // Recuperamos solo la altura inicial
            Vector3 desiredHeight = new Vector3(transform.position.x, farPointHeight, transform.position.z);

            // Si el jugador se aleja más de la distancia mínima, el objeto vuelve a su posición inicial
            transform.position = Vector3.MoveTowards(transform.position, desiredHeight, riseSpeed * Time.deltaTime);
        }
    }
}
