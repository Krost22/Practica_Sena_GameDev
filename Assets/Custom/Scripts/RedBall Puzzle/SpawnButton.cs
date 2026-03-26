using UnityEngine;

public class SpawnButton : MonoBehaviour
{
    [Header("Configuración del Spawn")]
    [SerializeField] private GameObject prefabToSpawn; // Prefab que se va a generar
    [SerializeField] private Vector3 spawnPosition; // Posición donde se va a spawnear
    [SerializeField] private float objectLifetime = 5f; // Tiempo que dura el objeto antes de desaparecer
    
    [Header("Configuración del Botón")]
    [SerializeField] private float cooldownTime = 1f; // Tiempo de espera entre activaciones
    [SerializeField] private string triggerTag = "ButtonTrigger"; // Tag del trigger que activa el botón
    
    private ConfigurableJoint buttonJoint;
    private GameObject currentSpawnedObject;
    private bool canActivate = true;
    private bool isPressed = false;
    private float lastActivationTime;
    private bool isPermanentlyDisabled = false;

    //On Puzzle Completed
    private MeshRenderer meshRenderer;
    public Material disableMaterial;

    public AudioSource audioCompleted;

    
    void Start()
    {
        // Obtener el ConfigurableJoint del botón
        buttonJoint = GetComponent<ConfigurableJoint>();

        //On Puzzle Completed
        meshRenderer = GetComponent<MeshRenderer>();
        
        if (buttonJoint == null)
        {
            Debug.LogError("SpawnButton: No se encontró ConfigurableJoint en este GameObject");
        }
        
        if (meshRenderer == null)
        {
            Debug.LogError("SpawnButton: No se encontró MeshRenderer en este GameObject");
        }
    }

    // Método llamado cuando el botón entra en contacto con el trigger
    private void OnTriggerEnter(Collider other)
    {
        // Si el botón está desactivado, no procesar nada
        if (!canActivate) return;

        // Verificar si el objeto que toca el trigger tiene el tag correcto
        if (other.CompareTag(triggerTag))
        {
            // Destruir el objeto actual si existe
            if (currentSpawnedObject != null)
            {
                Destroy(currentSpawnedObject);
            }
            
            if (!isPressed)
            {
                isPressed = true;
                ActivateButton();
            }
        }
    }
    
    // Método llamado cuando el botón sale del trigger
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(triggerTag))
        {
            isPressed = false;
        }
    }
    
    private void ActivateButton()
    {
        // Verificar cooldown
        if (Time.time - lastActivationTime < cooldownTime) return;
        
        // Destruir objeto anterior si existe
        if (currentSpawnedObject != null)
        {
            Destroy(currentSpawnedObject);
        }
        
        // Spawnear nuevo objeto
        SpawnObject();
        
        // Actualizar tiempo de última activación
        lastActivationTime = Time.time;
        
        // Iniciar cooldown
        StartCoroutine(CooldownCoroutine());
    }
    
    private void SpawnObject()
    {
        if (prefabToSpawn == null)
        {
            Debug.LogWarning("SpawnButton: No hay prefab asignado para spawnear");
            return;
        }
        
        // Instanciar el prefab en la posición especificada
        currentSpawnedObject = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
        
        // Programar la destrucción del objeto después del tiempo especificado
        Destroy(currentSpawnedObject, objectLifetime);
        
        Debug.Log($"SpawnButton: Objeto spawneado en posición {spawnPosition}");
    }
    
    private System.Collections.IEnumerator CooldownCoroutine()
    {
        canActivate = false;
        yield return new WaitForSeconds(cooldownTime);
        canActivate = true;
        isPressed = false; // Resetear el estado del botón después del cooldown
    }
    
    // Método público para activar el botón manualmente (útil para testing)
    public void ManualActivate()
    {
        ActivateButton();
    }
    
    // Método para cambiar el prefab en tiempo de ejecución
    public void SetPrefab(GameObject newPrefab)
    {
        prefabToSpawn = newPrefab;
    }
    
    // Método para cambiar la posición de spawn en tiempo de ejecución
    public void SetSpawnPosition(Vector3 newPosition)
    {
        spawnPosition = newPosition;
    }
    
    // Método para cambiar el tiempo de vida del objeto
    public void SetObjectLifetime(float newLifetime)
    {
        objectLifetime = newLifetime;
    }
    
    // Método para cambiar el tag del trigger
    public void SetTriggerTag(string newTag)
    {
        triggerTag = newTag;
    }
    
    // Método para verificar si el botón está actualmente presionado
    public bool IsPressed()
    {
        return isPressed;
    }
    
    // Método para resetear el estado del botón
    public void ResetButton()
    {
        if (isPermanentlyDisabled) return;
        isPressed = false;
        canActivate = true;
    }

    public void OnPuzzleCompleted()
    {
        DisableButton();
    }

    public void DisableButton()
    {
        // Evitar ejecuciones múltiples si ya está desactivado
        if (isPermanentlyDisabled) return;
        isPermanentlyDisabled = true;

        // Detener cualquier cooldown en curso para evitar que reactive el botón
        StopAllCoroutines();

        canActivate = false;
        isPressed = false;

        // 1. Bloqueo Físico: Congelar el botón en su posición actual
        // Los Joints no tienen propiedad 'enabled', al hacer el Rigidbody isKinematic es suficiente para bloquearlo.
        if (buttonJoint != null)
        {
            Destroy(buttonJoint);
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 2. Feedback Visual: Cambiar al material de desactivado
        if (meshRenderer != null && disableMaterial != null)
        {
            meshRenderer.material = disableMaterial;
        }
        else if (disableMaterial == null)
        {
            Debug.LogWarning($"SpawnButton en {gameObject.name}: No se ha asignado el material de desactivado.");
        }

        // 3. Feedback Auditivo: Reproducir sonido de éxito/completado
        if (audioCompleted != null)
        {
            if (!audioCompleted.isPlaying)
                audioCompleted.Play();
        }
    }
}
