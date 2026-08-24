using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CameraManager : MonoBehaviour
{
    [Header("Lista de Triggers y Cámaras")]
    [SerializeField] private List<CameraTriggerData> cameraTriggers = new List<CameraTriggerData>();
    
    [Header("Configuración")]
    [SerializeField] private bool debugMode = true;

    [Header("Input")]
    [SerializeField] private InputReader inputReader;
    
    [Header("Cámara Inicial")]
    [SerializeField] private Camera initialCamera; // Cámara que será activa al inicio
    [SerializeField] private string initialTriggerName = ""; // Nombre del trigger que activará la cámara inicial
    [SerializeField] private bool useInitialCamera = true; // Si usar cámara inicial o no
    
    [System.Serializable]
    public class CameraTriggerData
    {
        [Header("Información del Trigger")]
        public string triggerName;
        public CameraTrigger trigger;
        public Camera assignedCamera;
        
        [Header("Estados")]
        public bool isActive = false;
        public bool hasBeenUsed = false;
        
        public CameraTriggerData(string name, CameraTrigger camTrigger, Camera camera)
        {
            triggerName = name;
            trigger = camTrigger;
            assignedCamera = camera;
        }
    }
    
    void Start()
    {
        RefreshCameraTriggers();
        SetupInitialCamera();

        if (inputReader == null)
        {
            inputReader = FindAnyObjectByType<InputReader>() as InputReader;
        }
    }

    void Update()
    {
        // Debug de triggers se movió a un menú contextual para no depender de tecla fija
        // (anteriormente Input.GetKeyDown(debugKey))
    }
    
    [ContextMenu("Actualizar Lista de Triggers")]
    public void RefreshCameraTriggers()
    {
        cameraTriggers.Clear();

        // Buscar todos los CameraTrigger en la escena (Unity 6 API sin FindObjectsSortMode)
        CameraTrigger[] allTriggers = Object.FindObjectsByType<CameraTrigger>(FindObjectsInactive.Exclude);

        foreach (CameraTrigger trigger in allTriggers)
        {
            // Obtener la cámara asignada al trigger usando el getter público (sin reflexión)
            Camera assignedCamera = trigger.TargetCamera;

            CameraTriggerData data = new CameraTriggerData(
                trigger.gameObject.name,
                trigger,
                assignedCamera
            );

            cameraTriggers.Add(data);

            // Suscribirse al evento de activación para notificar al CameraController
            trigger.OnCameraActivated.RemoveListener(OnTriggerCameraActivated);
            trigger.OnCameraActivated.AddListener(OnTriggerCameraActivated);

            if (debugMode)
            {
                Debug.Log($"Trigger encontrado: {trigger.gameObject.name} -> {data.assignedCamera?.name ?? "Sin cámara"}");
            }
        }

        Debug.Log($"Se encontraron {cameraTriggers.Count} triggers de cámara en la escena.");
    }

    // Notificar al CameraController del player cuál es la cámara activa
    private void OnTriggerCameraActivated(Camera activatedCamera)
    {
        if (activatedCamera == null) return;

        // Buscar el CameraController del player y asignarle la cámara directamente
        // (evita que CameraController busque todas las cámaras cada frame)
        CameraController[] controllers = FindObjectsByType<CameraController>(FindObjectsInactive.Exclude);
        foreach (var cc in controllers)
        {
            cc.SetPlayerCamera(activatedCamera);
        }
    }

    private Camera GetTriggerCamera(CameraTrigger trigger)
    {
        // Usar el getter público (sin reflexión)
        return trigger.TargetCamera;
    }
    
    public void ShowTriggersInfo()
    {
        Debug.Log("=== LISTA DE TRIGGERS DE CÁMARA ===");
        
        for (int i = 0; i < cameraTriggers.Count; i++)
        {
            var triggerData = cameraTriggers[i];
            string status = triggerData.hasBeenUsed ? "USADO" : "DISPONIBLE";
            
            Debug.Log($"{i + 1}. {triggerData.triggerName} -> {triggerData.assignedCamera?.name ?? "Sin cámara"} [{status}]");
        }
        
        Debug.Log("=== FIN DE LA LISTA ===");
    }
    
    // Activar una cámara específica por nombre del trigger
    public void ActivateCameraByTriggerName(string triggerName)
    {
        CameraTriggerData triggerData = cameraTriggers.FirstOrDefault(t => t.triggerName == triggerName);
        
        if (triggerData != null && triggerData.trigger != null)
        {
            triggerData.trigger.ActivateCamera();
            triggerData.hasBeenUsed = true;
            Debug.Log($"Cámara activada mediante trigger: {triggerName}");
        }
        else
        {
            Debug.LogWarning($"No se encontró trigger con nombre: {triggerName}");
        }
    }
    
    // Obtener lista de nombres de triggers disponibles
    public List<string> GetAvailableTriggerNames()
    {
        return cameraTriggers.Select(t => t.triggerName).ToList();
    }
    
    // Verificar si un trigger específico existe
    public bool TriggerExists(string triggerName)
    {
        return cameraTriggers.Any(t => t.triggerName == triggerName);
    }
    
    // Obtener información de un trigger específico
    public CameraTriggerData GetTriggerData(string triggerName)
    {
        return cameraTriggers.FirstOrDefault(t => t.triggerName == triggerName);
    }
    
    // Método para uso desde otros scripts
    [ContextMenu("Activar Trigger 1")]
    public void TestActivateTrigger1()
    {
        if (cameraTriggers.Count > 0)
        {
            ActivateCameraByTriggerName(cameraTriggers[0].triggerName);
        }
    }
    
    [ContextMenu("Activar Trigger 2")]
    public void TestActivateTrigger2()
    {
        if (cameraTriggers.Count > 1)
        {
            ActivateCameraByTriggerName(cameraTriggers[1].triggerName);
        }
    }
    
    // Configurar la cámara inicial
    private void SetupInitialCamera()
    {
        if (!useInitialCamera) return;
        
        // Método 1: Usar cámara directa si está asignada
        if (initialCamera != null)
        {
            SetInitialCameraDirect(initialCamera);
            return;
        }
        
        // Método 2: Usar trigger por nombre si está especificado
        if (!string.IsNullOrEmpty(initialTriggerName))
        {
            CameraTriggerData triggerData = cameraTriggers.FirstOrDefault(t => t.triggerName == initialTriggerName);
            if (triggerData != null && triggerData.assignedCamera != null)
            {
                SetInitialCameraDirect(triggerData.assignedCamera);
                triggerData.isActive = true;
                if (debugMode)
                {
                    Debug.Log($"Cámara inicial activada mediante trigger: {initialTriggerName}");
                }
                return;
            }
        }
        
        // Método 3: Usar la primera cámara disponible si no se especificó nada
        if (cameraTriggers.Count > 0)
        {
            var firstTrigger = cameraTriggers.FirstOrDefault(t => t.assignedCamera != null);
            if (firstTrigger != null)
            {
                SetInitialCameraDirect(firstTrigger.assignedCamera);
                firstTrigger.isActive = true;
                if (debugMode)
                {
                    Debug.Log($"Cámara inicial activada automáticamente: {firstTrigger.triggerName}");
                }
            }
        }
    }
    
    // Activar una cámara directamente al inicio
    private void SetInitialCameraDirect(Camera camera)
    {
        if (camera == null) return;
        
        // Desactivar todas las cámaras primero
        DeactivateAllCameras();
        
        // Activar la cámara inicial
        camera.gameObject.SetActive(true);
        camera.enabled = true;
        
        if (debugMode)
        {
            Debug.Log($"Cámara inicial activada: {camera.name}");
        }
    }
    
    private void DeactivateAllCameras()
    {
        // Desactivar SOLO las cámaras gestionadas por triggers (no todas las de la escena)
        // Esto evita desactivar cámaras de UI, menú, etc.
        foreach (var triggerData in cameraTriggers)
        {
            if (triggerData.assignedCamera != null)
            {
                triggerData.assignedCamera.gameObject.SetActive(false);
                triggerData.assignedCamera.enabled = false;
            }
        }
    }
    
    // Método público para cambiar la cámara inicial en tiempo de ejecución
    [ContextMenu("Configurar Cámara Inicial")]
    public void SetInitialCameraManually()
    {
        SetupInitialCamera();
    }
}