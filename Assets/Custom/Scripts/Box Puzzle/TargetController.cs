using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TargetController : MonoBehaviour
{
    [Header("Configuración")]
    public BoxController correctBox; 
    public Material inactiveMaterial;
    public Material activeMaterial;

    [Header("Estado (Solo lectura)")]
    public bool isOccupied;

    private MeshRenderer meshRenderer;
    private PuzzleManager puzzleManager;

    void Start()
    {
        // Nos aseguramos automáticamente que sirva para OnTrigger
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true; 

        meshRenderer = GetComponent<MeshRenderer>();
        puzzleManager = FindFirstObjectByType<PuzzleManager>(); // Compatible y optimizado para Unity 6
        
        UpdateVisual();
    }

    public void UpdateVisual()
    {
        if (meshRenderer != null && activeMaterial != null && inactiveMaterial != null)
        {
            meshRenderer.material = isOccupied ? activeMaterial : inactiveMaterial;
        }
    }

    // TriggerStay maneja eventos perfectos (sin ciclos sueltos y sin checkear Vector3.Distance global)
    private void OnTriggerStay(Collider other)
    {
        if (isOccupied) return;

        if (other.TryGetComponent(out BoxController box))
        {
            // Solo hace 'snap' si validamos la caja, y si el jugador NO la tiene en su mano
            if (box == correctBox && !box.isGrabbed)
            {
                Debug.Log($"[{gameObject.name}] Detectó su caja correcta. Haciendo Snap y verificando Puzzle.");
                isOccupied = true;
                UpdateVisual();
                
                box.PlaceOnTarget(transform);
                
                // Dispara inteligentemente el check solo cuando se ejecuta la colocación
                if (puzzleManager != null)
                {
                    puzzleManager.CheckPuzzleState();
                }
                else
                {
                    Debug.LogError("¡El puzzleManager es null dentro del TargetController! No se le puede avisar de la victoria.");
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isOccupied) return;

        if (other.TryGetComponent(out BoxController box))
        {
            if (box == correctBox)
            {
                // Si la caja es retirada de su objetivo
                isOccupied = false;
                UpdateVisual();
                
                if (puzzleManager != null)
                    puzzleManager.CheckPuzzleState();
            }
        }
    }
}