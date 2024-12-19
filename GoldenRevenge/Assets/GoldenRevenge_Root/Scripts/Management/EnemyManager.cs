using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance; // Instancia pública de EnemyManager

    [Header("References")]
    [SerializeField] GameObject[] enemiesInScene; // Enemigos en escena
    [SerializeField] Vector3[] enemiesPosition; // Posicion de enemigos en escena
    [SerializeField] Quaternion[] enemiesRotation; // Rotación de enemigos en escena

    private void Awake()
    {
        // Asegura que solo exista una instancia
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        // Posiciones iniciales de enemigos
        SetInitialPositions();
    }

    // Posiciones iniciales de enemigos
    void SetInitialPositions()
    {
        // Inicializamos los arrays de posiciones y rotaciones con el tamaño adecuado
        enemiesPosition = new Vector3[enemiesInScene.Length];
        enemiesRotation = new Quaternion[enemiesInScene.Length];

        // Se resetean todos los enemigos, guardando sus posiciones y rotaciones
        for (int i = 0; i < enemiesInScene.Length; i++)
        {
            // Verificamos que el enemigo no sea nulo
            if (enemiesInScene[i] != null)
            {
                // Almacenamos la posición y rotación de cada enemigo en el índice correspondiente
                enemiesPosition[i] = enemiesInScene[i].transform.position;
                enemiesRotation[i] = enemiesInScene[i].transform.rotation;
            }
        }
    }

    // Reseteo de enemigos al descansar o morir
    public void RespawnEnemies()
    {
        // Se resetean todos los enemigos
        for (int i = 0; i < enemiesInScene.Length; i++)
        {
            enemiesInScene[i].SetActive(false);

            // Reseteo de posiciones
            enemiesInScene[i].transform.position = enemiesPosition[i];
            enemiesInScene[i].transform.rotation = enemiesRotation[i];

            enemiesInScene[i].SetActive(true);
        }
    }
}
