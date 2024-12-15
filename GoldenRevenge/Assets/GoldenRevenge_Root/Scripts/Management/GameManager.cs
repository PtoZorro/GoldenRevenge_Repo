using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player Stats")]
    public int maxStamina; // Stamina máxima
    public int maxHealth; // Salud máxima
    public int health; // Salud del jugador
    public int stamina; // Stamina del jugador

    void Awake()
    {
        // Asegura que solo exista una instancia
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Persiste entre escenas
    }

    void Start()
    {
        // Se establecen los valores del Player al inicio
        PlayerInitialStates();
    }

    void Update()
    {

    }

    void PlayerInitialStates()
    {
        // Valores iniciales
        health = maxHealth;
        stamina = maxStamina;
    }
}
