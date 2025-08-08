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
        meshRenderer.material = isOccupied ? activeMaterial : inactiveMaterial;
    }
}