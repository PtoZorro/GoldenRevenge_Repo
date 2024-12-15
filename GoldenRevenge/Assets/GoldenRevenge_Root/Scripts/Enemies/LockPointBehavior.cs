using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockPointBehavior : MonoBehaviour
{
    // En este script manejamos la altura del "punto de marcado del enemigo"
    // El punto de marcado es en el que la c�mara se fijar� autom�ticamente cuando fijemos un enemigo
    // La altura del punto variar� seg�n lo cercano que est� el Jugador del enemigo,
    // ya que as� ajustamos desde que �ngulo se ver� al enemigo seg�n la distancia, para que el Jugador no lo tape

    [Header("References")]
    Transform playerPos;

    [Header("Settings")]
    [SerializeField] float farPointHeight; // Altura del punto de marcado cuando el jugador est� lejos 
    [SerializeField] float riseHeight; // Altura que se suma a la variable anterior seg�n el Jugador se acerque
    [SerializeField] float minDistance; // Distancia m�nima del Jugador al enemigo a la que el punto comienza a subir
    [SerializeField] float riseSpeed; // Velocidad a la que el punto se mover�

    void Start()
    {
        // Encontramos al jugador en la escena 
        playerPos = GameObject.Find("Player")?.transform;
    }

    void Update()
    {
        // Calculamos la distancia entre el objeto y el jugador
        float distanceToPlayer = Vector3.Distance(transform.position, playerPos.position);

        // Si la distancia es menor que la m�nima
        if (distanceToPlayer < minDistance)
        {
            // Proporcionalidad de la subida, m�s r�pido conforme est� m�s cerca
            // Esta funci�n nos da un valor entre 0 (fuera de rango o en el punto m�s cercano) y 1 (justo al lado del jugador)
            float t = 1 - Mathf.Clamp01(distanceToPlayer / minDistance);

            // Calculamos la nueva posici�n vertical del objeto interpolando entre la posici�n m�s baja y la m�s alta posible seg�n t (cercania del Jugador entre 0-1)
            float newYPosition = Mathf.Lerp(farPointHeight, farPointHeight + riseHeight, t);

            // Subimos el objeto con una velocidad limitada
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, new Vector3(transform.localPosition.x, newYPosition, transform.localPosition.z), riseSpeed * Time.deltaTime);
        }
        else
        {
            // Recuperamos solo la altura inicial
            Vector3 desiredHeight = new Vector3(transform.localPosition.x, farPointHeight, transform.localPosition.z);

            // Si el jugador se aleja m�s de la distancia m�nima, el objeto vuelve a su posici�n inicial
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, desiredHeight, riseSpeed * Time.deltaTime);
        }
    }
}
