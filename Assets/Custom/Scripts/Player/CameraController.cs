using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Reference")]
    [SerializeField] private Camera playerCamera; // Referencia directa a la cámara
    [SerializeField] private bool autoAssignActiveCamera = true; // Asignar automáticamente la cámara activa

    private Vector3 currentMovementDirection;

    void Start()
    {
        // Asignar automáticamente la cámara activa si está habilitado
        if (autoAssignActiveCamera)
        {
            AssignActiveCamera();
        }
    }

    void Update()
    {
        // Verificar y actualizar la cámara activa si es necesario
        if (autoAssignActiveCamera && (playerCamera == null || !playerCamera.gameObject.activeInHierarchy))
        {
            AssignActiveCamera();
        }
    }

    /// <summary>
    /// Convierte input horizontal/vertical en dirección de movimiento relativa a la cámara
    /// </summary>
    public Vector3 GetMovementDirection(float horizontal, float vertical)
    {
        Camera cameraToUse = GetActiveCamera();
        
        // Si no hay cámara válida, usar dirección por defecto
        if (cameraToUse == null)
        {
            return new Vector3(horizontal, 0, vertical).normalized;
        }
        
        Vector3 cameraForward = cameraToUse.transform.forward;
        Vector3 cameraRight = cameraToUse.transform.right;
        
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();
        
        return (cameraForward * vertical + cameraRight * horizontal).normalized;
    }

    /// <summary>
    /// Obtiene la cámara activa actual
    /// </summary>
    public Camera GetActiveCamera()
    {
        // Intentar usar la referencia directa a la cámara primero
        Camera cameraToUse = playerCamera;
        
        // Verificar si la cámara asignada está activa
        if (cameraToUse != null && (!cameraToUse.gameObject.activeInHierarchy || !cameraToUse.enabled))
        {
            cameraToUse = null;
        }
        
        // Si no hay referencia válida, buscar cámara activa automáticamente
        if (cameraToUse == null && autoAssignActiveCamera)
        {
            cameraToUse = FindActiveCamera();
            if (cameraToUse != null)
            {
                playerCamera = cameraToUse; // Actualizar la referencia
            }
        }
        
        // Si aún no hay cámara válida, usar Camera.main
        if (cameraToUse == null)
        {
            cameraToUse = Camera.main;
        }
        
        return cameraToUse;
    }

    /// <summary>
    /// Asigna automáticamente la cámara activa a la referencia del jugador
    /// </summary>
    private void AssignActiveCamera()
    {
        // Buscar la cámara activa en la escena
        Camera activeCamera = FindActiveCamera();
        
        if (activeCamera != null)
        {
            playerCamera = activeCamera;
            Debug.Log($"CameraController: Cámara automáticamente asignada - {activeCamera.name}");
        }
        else
        {
            Debug.LogWarning("CameraController: No se encontró ninguna cámara activa para asignar automáticamente.");
        }
    }
    
    /// <summary>
    /// Encuentra la cámara actualmente activa en la escena
    /// </summary>
    private Camera FindActiveCamera()
    {
        // Buscar todas las cámaras en la escena
        Camera[] allCameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        
        // Prioridad 1: Buscar cámara con tag "MainCamera" que esté activa
        foreach (Camera cam in allCameras)
        {
            if (cam.CompareTag("MainCamera") && cam.gameObject.activeInHierarchy && cam.enabled)
            {
                return cam;
            }
        }
        
        // Prioridad 2: Buscar la primera cámara activa y habilitada
        foreach (Camera cam in allCameras)
        {
            if (cam.gameObject.activeInHierarchy && cam.enabled)
            {
                return cam;
            }
        }
        
        // Prioridad 3: Usar Camera.main como último recurso
        return Camera.main;
    }
    
    /// <summary>
    /// Método público para forzar la asignación de cámara activa
    /// </summary>
    [ContextMenu("Asignar Cámara Activa")]
    public void ForceAssignActiveCamera()
    {
        AssignActiveCamera();
    }

    // Propiedades públicas
    public Camera PlayerCamera => playerCamera;
}

