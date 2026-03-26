using UnityEngine;

/// <summary>
/// Controlador principal del jugador que orquesta todos los sistemas y maneja el movimiento usando CharacterController de forma simplificada.
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(BoxInteractionController))]
[RequireComponent(typeof(CameraController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float rotationSpeed = 15f;
    
    [Header("Jump & Gravity Settings")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float gravity = -15f;
    [SerializeField] private float gravityMultiplier = 2.0f;

    // Componentes
    private CharacterController _characterController;
    private BoxInteractionController _boxInteraction;
    private CameraController _cameraController;

    // Variables de estado
    private Vector3 _verticalVelocity;
    
    // Propiedades públicas
    public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }
    public bool IsGrounded => _characterController != null && _characterController.isGrounded;
    public Vector3 CurrentVelocity => _characterController != null ? _characterController.velocity : Vector3.zero;
    
    private float _originalMoveSpeed;

    void Start()
    {
        // Limpiar Rigidbody si se quedó adherido (causa lentitud y peleas con el CharacterController)
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) Destroy(rb);

        // Inicializar componentes
        _characterController = GetComponent<CharacterController>();
        _boxInteraction = GetComponent<BoxInteractionController>();
        _cameraController = GetComponent<CameraController>();
        
        // Guardar valores originales
        _originalMoveSpeed = moveSpeed;

        // Validaciones
        if (_boxInteraction == null) Debug.LogError("PlayerController: No se encontró BoxInteractionController");
        if (_cameraController == null) Debug.LogError("PlayerController: No se encontró CameraController");
    }

    void Update()
    {
        // 1. Obtener la dirección basada en la cámara activa y el input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = Vector3.zero;

        // Validar input con una pequeña zona muerta para evitar drift del mando
        if (Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f)
        {
            moveDirection = _cameraController.GetMovementDirection(horizontal, vertical);
        }

        // 2. Manejar la gravedad
        HandleGravity();

        // 3. Manejar interacciones y acciones
        _boxInteraction.HandleBoxPushing(moveDirection);
        HandleJump();
        
        // 4. Aplicar movimiento final
        HandleMovement(moveDirection);
    }

    private void HandleGravity()
    {
        if (_characterController.isGrounded && _verticalVelocity.y < 0)
        {
            // Pequeño valor negativo consistente par asegurar que el CharacterController permanezca atado al suelo
            _verticalVelocity.y = -2f; 
        }

        // Aplicar fuerza de gravedad
        _verticalVelocity.y += gravity * gravityMultiplier * Time.deltaTime;
    }

    private void HandleMovement(Vector3 moveDirection)
    {
        // Vector horizontal directo, sin aceleración (hace el control más responsivo y simple)
        Vector3 horizontalMove = moveDirection * moveSpeed;

        // Rotar el modelo hacia donde nos movemos instantáneamente o suavizado
        if (moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Mover combinando ejes: horizontal + vertical
        Vector3 finalMovement = horizontalMove + _verticalVelocity;
        _characterController.Move(finalMovement * Time.deltaTime);
    }

    private void HandleJump()
    {
        // Saltar solo si está en el suelo y no está cargando una caja
        if (Input.GetButtonDown("Jump") && _characterController.isGrounded && !_boxInteraction.IsCarryingBox)
        {
            _verticalVelocity.y = jumpForce;
        }
    }

    // Métodos públicos para modificadores externos
    public void SetMovementSpeed(float multiplier)
    {
        moveSpeed = _originalMoveSpeed * multiplier;
    }

    public void ResetMovementStats()
    {
        moveSpeed = _originalMoveSpeed;
    }
    
    public void ApplyKnockback(Vector3 direction, float force)
    {
        // Empuje vertical y horizontal ignorando interpolaciones
        _verticalVelocity.y = direction.y * force;
        _characterController.Move(new Vector3(direction.x, 0, direction.z) * force * Time.deltaTime);
    }
}