using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Instancia pública del Singleton

    [Header("References")]
    GameObject Player; // Referencia del jugador

    [Header("Player Stats")]
    public int maxStamina; // Stamina máxima
    public int maxHealth; // Salud máxima
    public int maxHealItems; // Número máximo de curativos
    public int health; // Salud del jugador
    public int stamina; // Stamina del jugador
    public int healItems; // Número de curativos

    [Header("States")]
    public bool dead; // Estado de muerte del jugador

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

        // Suscribirse al evento de cambio de escena
        SceneManager.sceneLoaded += OnSceneChanged;

        // Se establecen los valores del Player al inicio (¡¡¡Mover al Start!!!)
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
        healItems = maxHealItems;
    }

    #region SceneManagement

    // Función que se llama automáticamente al cambiar de escena
    void OnSceneChanged(Scene scene, LoadSceneMode mode)
    {
        // Buscar al jugador
        Player = GameObject.Find("Player");
    }

    #endregion
}
