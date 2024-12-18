using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Instancia p�blica del Singleton

    [Header("Player Stats")]
    public int maxStamina; // Stamina m�xima
    public int maxHealth; // Salud m�xima
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

        // Se establecen los valores del Player al inicio (���Mover al Start!!!)
        PlayerInitialStates();
    }

    void Start()
    {
        // Realentizar tiempo para debug
        //Time.timeScale = 0.4f;
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
