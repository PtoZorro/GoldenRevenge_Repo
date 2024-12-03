using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockPointBehavior : MonoBehaviour
{
    [Header("References")]
    Transform playerPos;

    [Header("Settings")]
    [SerializeField] float riseHeight;
    [SerializeField] float minDistance;
    [SerializeField] float maxSpeed;
    [SerializeField] float farPointY;

    private float initialHeight;

    void Start()
    {
        // Movemos inicialmente al punto deseado
        transform.localPosition = new Vector3(0, farPointY, 0);

        // Guardamos la posición inicial del objeto
        initialHeight = transform.position.y;

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

            // Calculamos la nueva posición vertical del objeto de manera proporcional
            float newYPosition = Mathf.Lerp(initialHeight, initialHeight + riseHeight, t);

            // Subimos el objeto con una velocidad limitada
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, newYPosition, transform.position.z), maxSpeed * Time.deltaTime);
        }
        else
        {
            // Recuperamos solo la altura inicial
            Vector3 desiredHeight = new Vector3(transform.position.x, initialHeight, transform.position.z);

            // Si el jugador se aleja más de la distancia mínima, el objeto vuelve a su posición inicial
            transform.position = Vector3.MoveTowards(transform.position, desiredHeight, maxSpeed * Time.deltaTime);
        }
    }
}
