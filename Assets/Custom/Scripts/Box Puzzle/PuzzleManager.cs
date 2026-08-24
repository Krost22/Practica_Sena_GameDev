using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PuzzleManager : MonoBehaviour
{
    [Header("Elementos del Puzzle")]
    [Tooltip("Lista guardada opcional para el reset del nivel")]
    public List<BoxController> boxes;
    public List<TargetController> targets;
    
    [Header("Puerta y Salida")]
    [Tooltip("Referencia a la puerta (Door_Prefab_Opened o Door) para desbloquear")]
    public GameObject exitDoor;
    
    [Header("Eventos (Opcional y Escalable)")]
    public UnityEvent OnPuzzleComplete;
    public UnityEvent OnPuzzleIncomplete;
    
    private bool puzzleComplete = false;

    [Header("Configuración")]
    [SerializeField] private bool debugMode = false;

    [Header("Progresión")]
    [Tooltip("ID del puzzle para LevelProgression. Dejar vacío si no se usa LevelProgression.")]
    [SerializeField] private string puzzleId = "BoxPuzzle";
    [SerializeField] private LevelProgression levelProgression;

    void Start()
    {
        if (levelProgression == null)
        {
            levelProgression = FindAnyObjectByType<LevelProgression>();
        }
    }

    // Se eliminó la ineficiencia del Update() y Corrutinas.
    // Ahora es un método ligero    // Método disparado por TargetController de forma eficiente
    public void CheckPuzzleState()
    {
        bool allOccupied = true;

        if (debugMode)
        {
            Debug.Log($"Revisando Puzzle: Hay {targets.Count} bases objetivo en la lista del PuzzleManager.");
        }

        for (int i = 0; i < targets.Count; i++)
        {
            TargetController target = targets[i];
            if (target == null)
            {
                Debug.LogWarning($"El Target en el espacio {i} de la lista está vacío. Revisa el inspector del PuzzleManager.");
                continue;
            }

            if (!target.isOccupied)
            {
                if (debugMode)
                {
                    Debug.Log($"El Target {target.gameObject.name} aún NO está ocupado.");
                }
                allOccupied = false;
                break; // Hay un objetivo libre, salimos
            }
        }

        if (allOccupied && !puzzleComplete)
        {
            puzzleComplete = true;
            Debug.Log("¡TODOS LOS OBJETIVOS LLENOS! Puzzle Completado. Abriendo la puerta.");

            HandleDoorStatus(true);
            OnPuzzleComplete?.Invoke(); // Llama a cualquier cosa en el inspector

            // Notificar al LevelProgression
            if (levelProgression != null && !string.IsNullOrEmpty(puzzleId))
            {
                levelProgression.RegisterPuzzleComplete(puzzleId);
            }
        }
        else if (!allOccupied && puzzleComplete)
        {
            puzzleComplete = false;

            HandleDoorStatus(false);
            OnPuzzleIncomplete?.Invoke();

            // Notificar al LevelProgression
            if (levelProgression != null && !string.IsNullOrEmpty(puzzleId))
            {
                levelProgression.RegisterPuzzleIncomplete(puzzleId);
            }
        }
    }

    private void HandleDoorStatus(bool isComplete)
    {
        if (exitDoor == null)
        {
            Debug.LogError("La 'Exit Door' no está asignada en el PuzzleManager.");
            return;
        }

        // Inspeccionamos TODOS los hijos en busca del que realmente tenga el HingeJoint
        Rigidbody[] doorRbs = exitDoor.GetComponentsInChildren<Rigidbody>();
        Rigidbody correctRb = null;
        
        foreach (var rb in doorRbs)
        {
            if (rb.GetComponent<HingeJoint>() != null)
            {
                correctRb = rb;
                break; // Encontramos la puerta principal
            }
        }

        Animator doorAnim = exitDoor.GetComponentInChildren<Animator>();

        if (correctRb != null)
        {
            Debug.Log($"¡Puerta lógica encontrada ({correctRb.name})! Cambiando isKinematic a {!isComplete}");
            correctRb.isKinematic = !isComplete;
            if (isComplete) correctRb.WakeUp();
        }
        else if (doorAnim != null)
        {
            doorAnim.SetBool("IsOpen", isComplete);
        }
        else
        {
            exitDoor.SetActive(!isComplete);
        }
    }

    public void ResetPuzzle()
    {
        if (boxes == null) return;
        
        foreach (BoxController box in boxes)
        {
            if (box != null) box.ResetPosition();
        }
    }
}