// PuzzleManager.cs - Controla la lógica global
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class PuzzleManager : MonoBehaviour
{
    public List<BoxController> boxes;
    public List<TargetController> targets;
    public GameObject exitDoor;
    [SerializeField] private float placementThreshold = 0.5f;
    [SerializeField] private float checkInterval = 0.1f; // Verificar cada 0.1 segundos en lugar de cada frame
    
    private bool isInitialized = false;
    private bool puzzleComplete = false;
    
    void Start()
    {
        StartCoroutine(InitializePuzzle());
    }
    
    private Coroutine puzzleStateCoroutine;
    private Coroutine boxPlacementCoroutine;
    
    void Update()
    {
        // Solo verificar si está inicializado
        if (isInitialized)
        {
            // Iniciar corrutinas solo si no están ejecutándose
            if (puzzleStateCoroutine == null)
            {
                puzzleStateCoroutine = StartCoroutine(CheckPuzzleStateCoroutine());
            }
            
            if (boxPlacementCoroutine == null)
            {
                boxPlacementCoroutine = StartCoroutine(CheckBoxPlacementCoroutine());
            }
        }
    }
    
    private IEnumerator InitializePuzzle()
    {
        // Esperar un frame para asegurar que todo esté inicializado
        yield return null;
        
        // Verificar que las listas estén asignadas
        if (targets == null || boxes == null)
        {
            Debug.LogError("PuzzleManager: Las listas targets o boxes no están asignadas en el Inspector");
            yield break;
        }
        
        // Verificar elementos individuales
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] == null)
            {
                Debug.LogError($"PuzzleManager: Target en índice {i} es null");
            }
        }
        
        for (int i = 0; i < boxes.Count; i++)
        {
            if (boxes[i] == null)
            {
                Debug.LogError($"PuzzleManager: Box en índice {i} es null");
            }
        }
        
        isInitialized = true;
        Debug.Log("PuzzleManager inicializado correctamente");
    }

    private IEnumerator CheckPuzzleStateCoroutine()
    {
        // Evitar múltiples ejecuciones simultáneas
        if (!isInitialized) yield break;
        
        // Esperar el intervalo antes de verificar
        yield return new WaitForSeconds(checkInterval);
        
        bool newPuzzleComplete = true;
        
        // Usar índices en lugar de foreach para evitar problemas de concurrencia
        for (int i = 0; i < targets.Count; i++)
        {
            TargetController target = targets[i];
            if (target == null) continue;
            
            target.isOccupied = false;
            
            for (int j = 0; j < boxes.Count; j++)
            {
                BoxController box = boxes[j];
                if (box == null) continue;
                
                try
                {
                    float distance = Vector3.Distance(target.transform.position, box.transform.position);
                    
                    if (distance < placementThreshold)
                    {
                        target.isOccupied = true;
                        box.isOnTarget = (box == target.correctBox);
                        
                        if (box == target.correctBox)
                        {
                            box.PlaceOnTarget();
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error verificando distancia entre target {i} y box {j}: {e.Message}");
                }
            }
            
            try
            {
                target.UpdateVisual();
                
                // Verificar que correctBox no sea null antes de acceder a sus propiedades
                if (!target.isOccupied || (target.correctBox != null && !target.correctBox.isOnTarget))
                    newPuzzleComplete = false;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error actualizando visual del target {i}: {e.Message}");
            }
        }
        
        // Solo actualizar el estado si ha cambiado
        if (newPuzzleComplete != puzzleComplete)
        {
            puzzleComplete = newPuzzleComplete;
            
            try
            {
                if (exitDoor != null)
                {
                    exitDoor.SetActive(!puzzleComplete);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error actualizando exitDoor: {e.Message}");
            }
        }
        
        // Reiniciar la variable para permitir nueva ejecución
        puzzleStateCoroutine = null;
    }

    private IEnumerator CheckBoxPlacementCoroutine()
    {
        if (!isInitialized) yield break;
        
        yield return new WaitForSeconds(checkInterval * 2); // Verificar menos frecuentemente
        
        // Usar índices para evitar problemas de concurrencia
        for (int i = 0; i < targets.Count; i++)
        {
            TargetController target = targets[i];
            if (target == null) continue;

            for (int j = 0; j < boxes.Count; j++)
            {
                BoxController box = boxes[j];
                if (box == null) continue;

                if (!box.isGrabbed)
                {
                    try
                    {
                        float distance = Vector3.Distance(target.transform.position, box.transform.position);
                        
                        if (distance < placementThreshold)
                        {
                            // Colocar automáticamente en el objetivo
                            box.transform.position = target.transform.position;
                            box.transform.rotation = Quaternion.identity;
                            box.PlaceOnTarget();
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Error en CheckBoxPlacement entre target {i} y box {j}: {e.Message}");
                    }
                }
            }
        }
        
        // Reiniciar la variable para permitir nueva ejecución
        boxPlacementCoroutine = null;
    }

    public void ResetPuzzle()
    {
        // Verificar que la lista no sea null
        if (boxes == null)
        {
            Debug.LogWarning("PuzzleManager: boxes lista es null en ResetPuzzle");
            return;
        }

        foreach (BoxController box in boxes)
        {
            // Verificar que la box no sea null
            if (box != null)
            {
                box.ResetPosition();
            }
        }
    }
}