using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Sistema de progresión de nivel que unifica múltiples puzzles.
/// Registra puzzles individuales como completados/incompletos y
/// abre la salida solo cuando TODOS los puzzles del nivel están completos.
///
/// Uso: asignar en el Inspector los IDs de los puzzles del nivel y
/// conectar los eventos OnPuzzleCompleted de cada PuzzleManager/RedBallGoal
/// a los métodos RegisterPuzzleComplete/RegisterPuzzleIncomplete.
/// </summary>
public class LevelProgression : MonoBehaviour
{
    [System.Serializable]
    public class PuzzleEntry
    {
        public string puzzleId;
        public string displayName;
        public bool isCompleted;
    }

    [Header("Puzzles del nivel")]
    [SerializeField] private List<PuzzleEntry> puzzles = new List<PuzzleEntry>();

    [Header("Salida")]
    [Tooltip("Puerta/trigger de salida que se activa al completar todos los puzzles")]
    [SerializeField] private GameObject exitObject;
    [SerializeField] private bool openExitOnComplete = true;

    [Header("Eventos")]
    public UnityEvent OnAllPuzzlesCompleted;
    public UnityEvent OnAnyPuzzleIncomplete;
    public UnityEvent<string> OnPuzzleCompleted;
    public UnityEvent<string> OnPuzzleIncomplete;

    [Header("Debug")]
    [SerializeField] private bool debugMode = false;

    private bool allCompleted = false;

    void Start()
    {
        // Verificar estado inicial (si no hay puzzles, la salida se abre inmediatamente)
        CheckAllPuzzles();
    }

    /// <summary>
    /// Marca un puzzle como completado por ID.
    /// Llamar desde el OnPuzzleComplete del PuzzleManager o RedBallGoal.
    /// </summary>
    public void RegisterPuzzleComplete(string puzzleId)
    {
        bool found = false;
        foreach (var p in puzzles)
        {
            if (p.puzzleId == puzzleId)
            {
                p.isCompleted = true;
                found = true;
                if (debugMode) Debug.Log($"[LevelProgression] Puzzle completado: {puzzleId}");
                break;
            }
        }

        if (!found)
        {
            // Auto-registrar si no existe
            puzzles.Add(new PuzzleEntry { puzzleId = puzzleId, displayName = puzzleId, isCompleted = true });
            if (debugMode) Debug.Log($"[LevelProgression] Puzzle auto-registrado y completado: {puzzleId}");
        }

        OnPuzzleCompleted?.Invoke(puzzleId);
        CheckAllPuzzles();
    }

    /// <summary>
    /// Marca un puzzle como incompleto por ID.
    /// </summary>
    public void RegisterPuzzleIncomplete(string puzzleId)
    {
        foreach (var p in puzzles)
        {
            if (p.puzzleId == puzzleId)
            {
                p.isCompleted = false;
                break;
            }
        }

        OnPuzzleIncomplete?.Invoke(puzzleId);
        CheckAllPuzzles();
    }

    /// <summary>
    /// Verifica si todos los puzzles están completos.
    /// </summary>
    public void CheckAllPuzzles()
    {
        bool allDone = true;
        foreach (var p in puzzles)
        {
            if (!p.isCompleted)
            {
                allDone = false;
                break;
            }
        }

        if (allDone && !allCompleted)
        {
            allCompleted = true;
            if (debugMode) Debug.Log("[LevelProgression] ¡TODOS los puzzles completos! Abriendo salida.");
            OnAllPuzzlesCompleted?.Invoke();

            if (openExitOnComplete && exitObject != null)
            {
                OpenExit();
            }

            // Notificar al GameManager
            if (GameManager.Instance != null)
            {
                // Marcar el nivel como completado (para guardado)
                GameManager.Instance.MarkPuzzleCompleted($"Level_{GameManager.Instance.Level}");
            }
        }
        else if (!allDone && allCompleted)
        {
            allCompleted = false;
            OnAnyPuzzleIncomplete?.Invoke();
            if (exitObject != null) CloseExit();
        }
    }

    private void OpenExit()
    {
        // Intentar abrir puerta física (HingeJoint) o animación o activar GameObject
        Rigidbody[] doorRbs = exitObject.GetComponentsInChildren<Rigidbody>();
        foreach (var rb in doorRbs)
        {
            if (rb.GetComponent<HingeJoint>() != null)
            {
                rb.isKinematic = false;
                rb.WakeUp();
                return;
            }
        }

        Animator doorAnim = exitObject.GetComponentInChildren<Animator>();
        if (doorAnim != null)
        {
            doorAnim.SetBool("IsOpen", true);
            return;
        }

        // Fallback: desactivar el objeto de salida (típico para puertas de niebla)
        exitObject.SetActive(false);
    }

    private void CloseExit()
    {
        if (exitObject == null) return;
        exitObject.SetActive(true);

        Animator doorAnim = exitObject.GetComponentInChildren<Animator>();
        if (doorAnim != null)
        {
            doorAnim.SetBool("IsOpen", false);
        }
    }

    public bool IsLevelComplete()
    {
        return allCompleted;
    }

    public List<PuzzleEntry> GetPuzzles()
    {
        return puzzles;
    }
}
