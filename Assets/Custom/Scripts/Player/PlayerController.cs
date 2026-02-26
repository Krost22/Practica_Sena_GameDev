using UnityEngine;

/// <summary>
/// Controlador principal del jugador que orquesta todos los sistemas
/// </summary>
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(BoxInteractionController))]
[RequireComponent(typeof(CameraController))]
public class PlayerController : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private BoxInteractionController boxInteraction;
    private CameraController cameraController;
    private Rigidbody _rb;

    void Start()
    {
        // Obtener referencias a los componentes
        playerMovement = GetComponent<PlayerMovement>();
        boxInteraction = GetComponent<BoxInteractionController>();
        cameraController = GetComponent<CameraController>();
        _rb = GetComponent<Rigidbody>();
        
        // Validar que todos los componentes existen
        if (playerMovement == null)
        {
            Debug.LogError("PlayerController: No se encontró PlayerMovement");
        }
        if (boxInteraction == null)
        {
            Debug.LogError("PlayerController: No se encontró BoxInteractionController");
        }
        if (cameraController == null)
        {
            Debug.LogError("PlayerController: No se encontró CameraController");
        }
    }

    void Update()
    {
        // Recolectar input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        // Obtener dirección de movimiento basada en la cámara
        Vector3 moveDirection = cameraController.GetMovementDirection(horizontal, vertical);
        playerMovement.SetMovementInput(moveDirection);
        
        // Manejar salto
        if (Input.GetButtonDown("Jump") && playerMovement.IsGrounded && !boxInteraction.IsCarryingBox)
        {
            playerMovement.ApplyJump();
        }
        
        // Manejar empuje de cajas
        boxInteraction.HandleBoxPushing(moveDirection);
    }
}