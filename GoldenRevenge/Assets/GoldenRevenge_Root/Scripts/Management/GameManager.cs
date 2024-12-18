using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Instancia p�blica del Singleton

    [Header("References")]
    GameObject Player; // Referencia del jugador

    [Header("Player Stats")]
    public int maxStamina; // Stamina m�xima
    public int maxHealth; // Salud m�xima
    public int maxHealItems; // N�mero m�ximo de curativos
    public int health; // Salud del jugador
    public int stamina; // Stamina del jugador
    public int healItems; // N�mero de curativos

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
        healItems = maxHealItems;
    }

    #region SceneManagement

    // Funci�n que se llama autom�ticamente al cambiar de escena
    void OnSceneChanged(Scene scene, LoadSceneMode mode)
    {
        // Buscar al jugador
        Player = GameObject.Find("Player");
    }

    #endregion
}
