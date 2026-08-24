using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Trigger que carga el siguiente nivel cuando el jugador lo toca.
/// Se coloca en la salida de cada nivel.
/// </summary>
[RequireComponent(typeof(Collider))]
public class LevelExitTrigger : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool loadNextLevel = true;
    [SerializeField] private int specificLevelIndex = -1; // Si loadNextLevel=false, carga este índice

    [Header("Feedback")]
    [SerializeField] private GameObject completeEffect;
    [SerializeField] private AudioClip completeSound;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        // Feedback
        if (completeEffect != null)
        {
            Instantiate(completeEffect, transform.position, Quaternion.identity);
        }
        if (completeSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(completeSound);
        }

        // Marcar puzzle/nivel como completado en LevelProgression (si existe)
        LevelProgression lp = FindAnyObjectByType<LevelProgression>();
        if (lp != null && !lp.IsLevelComplete())
        {
            // Para niveles sin PuzzleManager (plataformas), el exit trigger
            // marca el nivel como completado automáticamente
            lp.RegisterPuzzleComplete("LevelExit");
        }

        // Guardar progreso
        if (GameManager.Instance != null)
        {
            if (loadNextLevel)
            {
                GameManager.Instance.LoadNextLevel();
            }
            else if (specificLevelIndex >= 0)
            {
                if (System.Enum.IsDefined(typeof(GameManager.LevelId), specificLevelIndex))
                {
                    GameManager.Instance.LoadLevel((GameManager.LevelId)specificLevelIndex);
                }
            }
        }
        else
        {
            int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (nextIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextIndex);
            }
        }
    }
}
