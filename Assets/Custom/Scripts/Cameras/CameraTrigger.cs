using UnityEngine;
using System.Collections.Generic;

public class CameraTrigger : MonoBehaviour
{
    [Header("Configuración de Cámara")]
    [SerializeField] private Camera targetCamera; // Cámara que se activará
    [SerializeField] private string playerTag = "Player"; // Tag del jugador
    
    [Header("Configuración de Transición")]
    [SerializeField] private bool instantTransition = false; // Transición instantánea
    
    [Header("Configuración del Trigger")]
    [SerializeField] private bool destroyAfterUse = false; // Si se destruye después de activarse
    [SerializeField] private bool multipleUses = true; // Si puede activarse múltiples veces
    
    private Camera previousCamera;
    private bool hasBeenActivated = false;
    
    // Lista estática de todos los triggers para gestión centralizada
    private static List<CameraTrigger> allTriggers = new List<CameraTrigger>();
    
    void Start()
    {
        // Agregar este trigger a la lista estática
        if (!allTriggers.Contains(this))
        {
            allTriggers.Add(this);
        }
        
        // Verificar que tenemos una cámara asignada
        if (targetCamera == null)
        {
            Debug.LogWarning($"CameraTrigger en {gameObject.name} no tiene una cámara asignada!");
        }
        
        // NO desactivar automáticamente la cámara al inicio
        // El CameraManager se encarga de manejar qué cámara debe estar activa al inicio
    }
    
    void OnDestroy()
    {
        // Remover de la lista cuando se destruya
        if (allTriggers.Contains(this))
        {
            allTriggers.Remove(this);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Verificar si es el jugador
        if (other.CompareTag(playerTag))
        {
            // Verificar si ya fue activado y no permite múltiples usos
            if (hasBeenActivated && !multipleUses)
            {
                return;
            }
            
            ActivateCamera();
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        // Opcional: desactivar cámara cuando sale del trigger
        if (other.CompareTag(playerTag) && !instantTransition)
        {
            // Aquí podrías agregar lógica para desactivar la cámara
            // si quieres que se desactive cuando sale del área
        }
    }
    
    public void ActivateCamera()
    {
        if (targetCamera == null) return;
        
        // Guardar referencia de la cámara anterior
        previousCamera = Camera.main;
        
        // Desactivar todas las otras cámaras de triggers
        DeactivateAllTriggerCameras();
        
        // Activar la cámara objetivo
        targetCamera.gameObject.SetActive(true);
        targetCamera.enabled = true;
        
        // Si no es transición instantánea, usar transición suave
        if (!instantTransition)
        {
            StartCoroutine(SmoothCameraTransition());
        }
        
        hasBeenActivated = true;
        
        Debug.Log($"Cámara activada en trigger: {gameObject.name}");
        
        // Destruir si está configurado para hacerlo
        if (destroyAfterUse)
        {
            Destroy(gameObject);
        }
    }
    
    private System.Collections.IEnumerator SmoothCameraTransition()
    {
        // Aquí puedes agregar lógica para transición suave entre cámaras
        // Por ejemplo, moviendo la posición o rotación gradualmente
        yield return new WaitForSeconds(0.1f);
    }
    
    private static void DeactivateAllTriggerCameras()
    {
        foreach (CameraTrigger trigger in allTriggers)
        {
            if (trigger != null && trigger.targetCamera != null)
            {
                trigger.targetCamera.gameObject.SetActive(false);
            }
        }
    }
    
    // Método público para configurar la cámara desde código
    public void SetTargetCamera(Camera newCamera)
    {
        targetCamera = newCamera;
    }
    
    // Método para obtener información del trigger
    public string GetTriggerInfo()
    {
        string cameraName = targetCamera != null ? targetCamera.name : "Sin cámara asignada";
        return $"Trigger: {gameObject.name}, Cámara: {cameraName}";
    }
    
    // Método estático para obtener todos los triggers
    public static List<CameraTrigger> GetAllTriggers()
    {
        return new List<CameraTrigger>(allTriggers);
    }
}
