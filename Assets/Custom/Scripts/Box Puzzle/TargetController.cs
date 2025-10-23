// TargetController.cs - Para los puntos de destino
using UnityEngine;

public class TargetController : MonoBehaviour
{
    public BoxController correctBox; // Asignar en inspector
    public bool isOccupied;
    public Material inactiveMaterial;
    public Material activeMaterial;

    private MeshRenderer meshRenderer;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        UpdateVisual();
    }

    public void UpdateVisual()
    {
        // Verificar que meshRenderer no sea null
        if (meshRenderer == null)
        {
            Debug.LogWarning("TargetController: meshRenderer es null en UpdateVisual");
            return;
        }

        // Verificar que los materiales no sean null
        if (isOccupied && activeMaterial == null)
        {
            Debug.LogWarning("TargetController: activeMaterial es null");
            return;
        }

        if (!isOccupied && inactiveMaterial == null)
        {
            Debug.LogWarning("TargetController: inactiveMaterial es null");
            return;
        }

        meshRenderer.material = isOccupied ? activeMaterial : inactiveMaterial;
    }
}