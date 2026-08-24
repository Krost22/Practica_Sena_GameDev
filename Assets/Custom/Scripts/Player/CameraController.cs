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
        if (autoAssignActiveCamera && playerCamera == null)
        {
            AssignActiveCamera();
        }
    }

    void Update()
    {
        // Ya no buscamos todas las cámaras cada frame.
        // El CameraManager notifica vía SetPlayerCamera() cuando cambia la cámara activa.
        // Solo reasignamos si perdimos la referencia (cámara destruida/desactivada)
        if (playerCamera != null && !playerCamera.gameObject.activeInHierarchy)
        {
            playerCamera = null;
            if (autoAssignActiveCamera)
            {
                AssignActiveCamera();
            }
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
        // Usar la referencia directa (asignada por CameraManager o auto-detección)
        if (playerCamera != null && playerCamera.gameObject.activeInHierarchy && playerCamera.enabled)
        {
            return playerCamera;
        }

        // Fallback: Camera.main (cacheado por Unity)
        return Camera.main;
    }

    /// <summary>
    /// Asigna automáticamente la cámara activa a la referencia del jugador
    /// </summary>
    private void AssignActiveCamera()
    {
        // Buscar la cámara activa en la escena (solo al inicio o tras perder la referencia)
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
    /// Encuentra la cámara actualmente activa en la escena (solo al inicio, no cada frame)
    /// </summary>
    private Camera FindActiveCamera()
    {
        // Prioridad 1: Camera.main (Unity cachea la cámara con tag MainCamera)
        if (Camera.main != null) return Camera.main;

        // Prioridad 2: Buscar la primera cámara activa (Unity 6 API sin FindObjectsSortMode)
        Camera[] allCameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Exclude);
        foreach (Camera cam in allCameras)
        {
            if (cam.gameObject.activeInHierarchy && cam.enabled)
            {
                return cam;
            }
        }

        return null;
    }

    /// <summary>
    /// Método público para forzar la asignación de cámara activa.
    /// Llamado por CameraManager cuando una cámara se activa vía trigger.
    /// </summary>
    public void SetPlayerCamera(Camera newCamera)
    {
        if (newCamera != null)
        {
            playerCamera = newCamera;
        }
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

