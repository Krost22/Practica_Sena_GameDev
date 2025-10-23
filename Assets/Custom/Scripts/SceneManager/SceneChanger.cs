using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public string sceneName;
    
    public void StartGame()
    {
        // Ahora podemos usar SceneManager sin conflicto
        SceneManager.LoadScene(sceneName);
    }
}
