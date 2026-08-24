using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Menú de pausa. Se activa/desactiva según el estado del GameManager.
/// Botones: Reanudar, Reiniciar nivel, Menú principal, Salir.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private GameObject winPanel;

    [Header("Botones (asignar en Inspector)")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    [Header("Texto de vidas (opcional)")]
    [SerializeField] private TextMeshProUGUI livesText;

    void Start()
    {
        HideAllPanels();

        if (resumeButton != null) resumeButton.onClick.AddListener(OnResume);
        if (restartButton != null) restartButton.onClick.AddListener(OnRestart);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenu);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuit);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged.AddListener(OnGameStateChanged);
            GameManager.Instance.OnLivesChanged.AddListener(OnLivesChanged);
        }
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged.RemoveListener(OnGameStateChanged);
            GameManager.Instance.OnLivesChanged.RemoveListener(OnLivesChanged);
        }
    }

    private void OnGameStateChanged(GameManager.GameState state)
    {
        HideAllPanels();

        switch (state)
        {
            case GameManager.GameState.Paused:
                if (pausePanel != null) pausePanel.SetActive(true);
                break;
            case GameManager.GameState.Lose:
                if (deathPanel != null) deathPanel.SetActive(true);
                break;
            case GameManager.GameState.Win:
                if (winPanel != null) winPanel.SetActive(true);
                break;
        }
    }

    private void OnLivesChanged(int lives)
    {
        if (livesText != null)
        {
            livesText.text = $"Vidas: {lives}";
        }
    }

    private void HideAllPanels()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (deathPanel != null) deathPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
    }

    // ---- Callbacks de botones ----
    private void OnResume()
    {
        GameManager.Instance?.Resume();
    }

    private void OnRestart()
    {
        GameManager.Instance?.RestartCurrentLevel();
    }

    private void OnMainMenu()
    {
        GameManager.Instance?.ReturnToMainMenu();
    }

    private void OnQuit()
    {
        GameManager.Instance?.QuitGame();
    }
}
