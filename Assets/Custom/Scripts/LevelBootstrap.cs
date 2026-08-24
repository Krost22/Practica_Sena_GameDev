using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Bootstrap de nivel: auto-crea todos los sistemas core (GameManager, AudioManager,
/// InputReader, EventSystem, Player, CameraManager) si no existen en la escena.
///
/// Esto permite que cada escena de nivel funcione standalone al editar en el Editor
/// sin necesidad de arrastrar manualmente todos los managers.
///
/// Colocar este componente en un GameObject vacío llamado "LevelBootstrap" en cada escena.
/// Asignar el prefab del Player y el spawnPoint.
/// </summary>
public class LevelBootstrap : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnPoint;

    [Header("Opcional: cámara por defecto")]
    [SerializeField] private Camera defaultCamera;

    [Header("Configuración de nivel")]
    [SerializeField] private GameManager.LevelId levelId = GameManager.LevelId.Level2;
    [SerializeField] private string objectiveText = "Explora y resuelve el puzzle";

    [Header("Auto-crear sistemas si faltan")]
    [SerializeField] private bool autoCreateSystems = true;

    void Awake()
    {
        if (autoCreateSystems)
        {
            EnsureGameManager();
            EnsureAudioManager();
            EnsureEventSystem();
        }
    }

    void Start()
    {
        if (autoCreateSystems)
        {
            EnsurePlayer();
            EnsureCamera();
        }

        // Configurar objetivo del nivel
        if (ObjectiveSystem.Instance != null && !string.IsNullOrEmpty(objectiveText))
        {
            ObjectiveSystem.Instance.SetObjective(objectiveText);
        }
    }

    private void EnsureGameManager()
    {
        if (GameManager.Instance != null) return;

        // Buscar en la escena
        GameManager existing = FindAnyObjectByType<GameManager>();
        if (existing != null) return;

        // Crear
        GameObject gmObj = new GameObject("GameManager");
        gmObj.AddComponent<GameManager>();
        Debug.Log("[LevelBootstrap] GameManager creado automáticamente.");
    }

    private void EnsureAudioManager()
    {
        if (AudioManager.Instance != null) return;

        AudioManager existing = FindAnyObjectByType<AudioManager>();
        if (existing != null) return;

        GameObject amObj = new GameObject("AudioManager");
        amObj.AddComponent<AudioManager>();
        Debug.Log("[LevelBootstrap] AudioManager creado automáticamente.");
    }

    private void EnsureEventSystem()
    {
        // Buscar EventSystem existente
        UnityEngine.EventSystems.EventSystem es = FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>();
        if (es != null) return;

        GameObject esObj = new GameObject("EventSystem");
        esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        Debug.Log("[LevelBootstrap] EventSystem creado automáticamente.");
    }

    private void EnsurePlayer()
    {
        // Verificar si ya hay un Player en la escena
        GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");
        if (existingPlayer != null) return;

        if (playerPrefab != null)
        {
            Vector3 pos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
            GameObject player = Instantiate(playerPrefab, pos, Quaternion.identity);
            player.name = "Player";
            Debug.Log("[LevelBootstrap] Player instanciado desde prefab.");
        }
        else
        {
            Debug.LogWarning("[LevelBootstrap] No hay prefab de Player asignado. El nivel no será jugable.");
        }
    }

    private void EnsureCamera()
    {
        // Si ya hay cámaras activas en la escena, no hacer nada
        Camera[] cameras = FindObjectsByType<Camera>(FindObjectsInactive.Exclude);
        bool hasActiveCamera = false;
        foreach (var cam in cameras)
        {
            if (cam.gameObject.activeInHierarchy && cam.enabled)
            {
                hasActiveCamera = true;
                break;
            }
        }

        if (hasActiveCamera) return;

        // Si hay una cámara por defecto asignada, activarla
        if (defaultCamera != null)
        {
            defaultCamera.gameObject.SetActive(true);
            defaultCamera.enabled = true;
            return;
        }

        // Crear una cámara básica
        GameObject camObj = new GameObject("Main Camera");
        Camera mainCam = camObj.AddComponent<Camera>();
        camObj.tag = "MainCamera";
        mainCam.transform.position = new Vector3(0, 5, -10);
        mainCam.transform.LookAt(Vector3.zero);
        Debug.Log("[LevelBootstrap] Cámara básica creada automáticamente.");
    }
}
