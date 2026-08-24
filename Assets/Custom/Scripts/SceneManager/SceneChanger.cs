using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    // Inicia el juego cargando el Level 1 via GameManager (si existe) o directamente
    public void StartGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadLevel(GameManager.LevelId.Level1);
        }
        else
        {
            SceneManager.LoadScene(1);
        }
    }

    public void ExitGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
        else
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    // Carga un nivel específico por ID
    public void LoadLevel(int levelIndex)
    {
        if (GameManager.Instance != null && System.Enum.IsDefined(typeof(GameManager.LevelId), levelIndex))
        {
            GameManager.Instance.LoadLevel((GameManager.LevelId)levelIndex);
        }
        else
        {
            SceneManager.LoadScene(levelIndex);
        }
    }
}
