// PuzzleManager.cs - Controla la lógica global
using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public List<BoxController> boxes;
    public List<TargetController> targets;
    public GameObject exitDoor;
    [SerializeField] private float placementThreshold = 0.5f;
    void Update()
    {
        CheckPuzzleState();
        CheckBoxPlacement();
    }

     private void CheckPuzzleState()
    {
        bool puzzleComplete = true;

        foreach (TargetController target in targets)
        {
            target.isOccupied = false;
            
            foreach (BoxController box in boxes)
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
            
            target.UpdateVisual();
            
            if (!target.isOccupied || !target.correctBox.isOnTarget)
                puzzleComplete = false;
        }

        exitDoor.SetActive(!puzzleComplete);
    }

     private void CheckBoxPlacement()
    {
        foreach (TargetController target in targets)
        {
            foreach (BoxController box in boxes)
            {
                if (!box.isGrabbed)
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
            }
        }
    }

    public void ResetPuzzle()
    {
        foreach (BoxController box in boxes)
        {
            box.ResetPosition();
        }
    }
}