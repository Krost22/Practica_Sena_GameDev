using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

/// <summary>
/// Gestor central del estado del juego (singleton persistente entre escenas).
/// Controla: estado global (Menu/Playing/Paused/Win/Lose), nivel actual,
/// progresión de puzzles, y transiciones entre escenas.
/// </summary>
public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        Win,
        Lose
    }

    public enum LevelId
    {
        MainMenu = 0,
        Level1 = 1,
        Level2 = 2,
        Level3 = 3,
        Level4 = 4,
        Level5 = 5,
        Final = 6
    }

    [Header("Estado actual (Solo lectura)")]
    [SerializeField] private GameState currentState = GameState.MainMenu;
    [SerializeField] private LevelId currentLevel = LevelId.MainMenu;

    [Header("Configuración")]
    [SerializeField] private int maxLives = 3;
    [SerializeField] private int currentLives;

    [Header("Input")]
    [SerializeField] private InputReader inputReader;

    [Header("Eventos")]
    public UnityEvent<GameState> OnStateChanged;
    public UnityEvent<LevelId> OnLevelChanged;
    public UnityEvent<int> OnLivesChanged;
    public UnityEvent OnPlayerDeath;
    public UnityEvent OnGameWin;

    // Progresión de puzzles por nivel
    private HashSet<string> completedPuzzles = new HashSet<string>();

    // Singleton
    public static GameManager Instance { get; private set; }

    // Propiedades públicas
    public GameState State => currentState;
    public LevelId Level => currentLevel;
    public int CurrentLives => currentLives;
    public int MaxLives => maxLives;

    void Awake()
    {
        // Singleton persistente
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        currentLives = maxLives;
    }

    void Start()
    {
        if (inputReader == null)
        {
            inputReader = FindAnyObjectByType<InputReader>() as InputReader;
        }
        if (inputReader != null)
        {
            inputReader.PauseStarted += TogglePause;
        }

        // Detectar nivel actual según la escena activa
        DetectCurrentLevel();
    }

    void OnDestroy()
    {
        if (inputReader != null)
        {
            inputReader.PauseStarted -= TogglePause;
        }
    }

    private void DetectCurrentLevel()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        string sceneId = activeScene.name;
        int separatorIndex = sceneId.IndexOf('_');
        if (separatorIndex >= 0 && separatorIndex < sceneId.Length - 1)
        {
            sceneId = sceneId.Substring(separatorIndex + 1);
        }

        if (Enum.TryParse(sceneId, true, out LevelId parsed))
        {
            currentLevel = parsed;
            SetState(parsed == LevelId.MainMenu ? GameState.MainMenu : GameState.Playing);
        }
        else
        {
            SetState(GameState.Playing);
        }
    }

    // ---- Cambio de estado ----
    public void SetState(GameState newState)
    {
        if (currentState == newState) return;

        GameState previousState = currentState;
        currentState = newState;

        switch (newState)
        {
            case GameState.Paused:
                Time.timeScale = 0f;
                break;
            case GameState.Playing:
                Time.timeScale = 1f;
                break;
            case GameState.Lose:
                Time.timeScale = 0f;
                OnPlayerDeath?.Invoke();
                break;
            case GameState.Win:
                OnGameWin?.Invoke();
                break;
        }

        OnStateChanged?.Invoke(newState);
    }

    // ---- Pausa ----
    public void TogglePause()
    {
        if (currentState == GameState.Playing)
        {
            SetState(GameState.Paused);
        }
        else if (currentState == GameState.Paused)
        {
            SetState(GameState.Playing);
        }
    }

    public void Pause()
    {
        if (currentState == GameState.Playing) SetState(GameState.Paused);
    }

    public void Resume()
    {
        if (currentState == GameState.Paused) SetState(GameState.Playing);
    }

    // ---- Vidas ----
    public void LoseLife()
    {
        currentLives = Mathf.Max(0, currentLives - 1);
        OnLivesChanged?.Invoke(currentLives);

        if (currentLives <= 0)
        {
            SetState(GameState.Lose);
        }
    }

    public void ResetLives()
    {
        currentLives = maxLives;
        OnLivesChanged?.Invoke(currentLives);
    }

    // ---- Progresión de puzzles ----
    public void MarkPuzzleCompleted(string puzzleId)
    {
        if (!completedPuzzles.Contains(puzzleId))
        {
            completedPuzzles.Add(puzzleId);
        }
    }

    public bool IsPuzzleCompleted(string puzzleId)
    {
        return completedPuzzles.Contains(puzzleId);
    }

    public void ClearPuzzleProgress()
    {
        completedPuzzles.Clear();
    }

    /// <summary>
    /// Verifica si un nivel está desbloqueado (delegado a SaveSystem).
    /// </summary>
    public bool IsLevelUnlocked(int levelIndex)
    {
        return SaveSystem.IsLevelUnlocked(levelIndex);
    }

    /// <summary>
    /// Verifica si un nivel está completado (delegado a SaveSystem).
    /// </summary>
    public bool IsLevelCompleted(int levelIndex)
    {
        return SaveSystem.IsLevelCompleted(levelIndex);
    }

    // ---- Transiciones de nivel ----
    public void LoadLevel(LevelId level)
    {
        currentLevel = level;
        OnLevelChanged?.Invoke(level);

        if (level == LevelId.MainMenu)
        {
            SetState(GameState.MainMenu);
        }
        else
        {
            SetState(GameState.Playing);
        }

        SceneManager.LoadScene((int)level);
    }

    public void LoadNextLevel()
    {
        // Marcar el nivel actual como completado antes de avanzar
        SaveSystem.MarkLevelCompleted((int)currentLevel);

        int nextIndex = (int)currentLevel + 1;
        if (Enum.IsDefined(typeof(LevelId), nextIndex))
        {
            LoadLevel((LevelId)nextIndex);
        }
        else
        {
            // No hay más niveles: victoria
            SetState(GameState.Win);
        }
    }

    public void RestartCurrentLevel()
    {
        ResetLives();
        SetState(GameState.Playing);
        SceneManager.LoadScene((int)currentLevel);
    }

    public void ReturnToMainMenu()
    {
        ClearPuzzleProgress();
        ResetLives();
        LoadLevel(LevelId.MainMenu);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
