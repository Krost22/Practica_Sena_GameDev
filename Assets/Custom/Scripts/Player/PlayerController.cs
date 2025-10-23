using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float acceleration = 15f;
    [SerializeField] private float deceleration = 20f;
    [SerializeField] private float rotationSpeed = 15f;
    
    [Header("Inertia Settings")]
    [SerializeField] private float friction = 8f;
    [SerializeField] private float airFrictionMultiplier = 0.5f;
    [SerializeField] private float groundDrag = 4f;
    [SerializeField] private float airDrag = 1.5f;
    
    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float gravityMultiplier = 2.5f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Box Pushing")]
[SerializeField] private float pushForce = 3f;


    [Header("Grab Settings")]
[SerializeField] private KeyCode grabKey = KeyCode.E;
[SerializeField] private float grabRange = 2f;
[SerializeField] private float grabSpeedMultiplier = 0.7f;
[SerializeField] private LayerMask boxLayer;
[SerializeField] private Transform grabPoint; // Punto donde se sostiene la caja

[Header("Camera Reference")]
[SerializeField] private Camera playerCamera; // Referencia directa a la cámara
[SerializeField] private bool autoAssignActiveCamera = true; // Asignar automáticamente la cámara activa

private BoxController carriedBox;
private float originalMaxSpeed;
private float originalAcceleration;
private float originalDeceleration;
    
    private Rigidbody _rb;
    private Vector3 _moveInput;
    private Vector3 _currentVelocity;
    private bool _isGrounded;
    private float _verticalVelocity;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;
        originalMaxSpeed = maxSpeed;
        originalAcceleration = acceleration;
        originalDeceleration = deceleration;
        
        // Asignar automáticamente la cámara activa si está habilitado
        if (autoAssignActiveCamera)
        {
            AssignActiveCamera();
        }
    }

    void Update()
    {
        // Verificar y actualizar la cámara activa si es necesario
        if (autoAssignActiveCamera && (playerCamera == null || !playerCamera.gameObject.activeInHierarchy))
        {
            AssignActiveCamera();
        }
        
        GatherInput();
        HandleBoxInteraction();
        HandleJump();
        GroundCheck();
        HandleGrabInput();
        UpdateCarriedBoxPosition();
        ShowGrabIndicator();
    }

    private void HandleGrabInput()
{
    if (Input.GetKeyDown(grabKey))
    {
        if (carriedBox == null)
        {
            TryGrabBox();
        }
        else
        {
            ReleaseBox(false);
        }
    }
}

private void TryGrabBox()
{
    // Verificar que grabPoint esté asignado
    if (grabPoint == null)
    {
        Debug.LogWarning("PlayerController: grabPoint no está asignado en el Inspector");
        return;
    }
    
    Vector3 rayStart = transform.position + Vector3.up * 0.5f;
    RaycastHit hit;
    
    if (Physics.Raycast(rayStart, transform.forward, out hit, grabRange, boxLayer))
    {
        if (hit.collider.TryGetComponent<BoxController>(out var box))
        {
            // No agarrar si ya está en su objetivo
            if (!box.isOnTarget)
            {
                carriedBox = box;
                carriedBox.Grab(grabPoint);
                
                // Reducir velocidad al cargar caja
                maxSpeed *= grabSpeedMultiplier;
                acceleration *= grabSpeedMultiplier;
                deceleration *= grabSpeedMultiplier;
            }
        }
    }
}

private void ReleaseBox(bool throwIt)
{
    if (carriedBox != null)
    {
        Vector3 releaseVelocity = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
        
        if (throwIt)
        {
            releaseVelocity = transform.forward * 10f + Vector3.up * 3f;
        }

        carriedBox.Release(releaseVelocity);
        
        // Restaurar velocidad original
        maxSpeed = originalMaxSpeed;
        acceleration = originalAcceleration;
        deceleration = originalDeceleration;
        
        carriedBox = null;
    }
}

private void UpdateCarriedBoxPosition()
{
    if (carriedBox != null && grabPoint != null)
    {
        // Suavizar posición mientras se carga
        carriedBox.transform.position = Vector3.Lerp(
            carriedBox.transform.position,
            grabPoint.position,
            10f * Time.deltaTime
        );
        
        // Rotación alineada con jugador
        carriedBox.transform.rotation = Quaternion.Slerp(
            carriedBox.transform.rotation,
            transform.rotation,
            8f * Time.deltaTime
        );
    }
}

    void FixedUpdate()
    {
        ApplyMovement();
        ApplyFriction();
        ApplyGravity();
    }

    private void GatherInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        // Intentar usar la referencia directa a la cámara primero
        Camera cameraToUse = playerCamera;
        
        // Verificar si la cámara asignada está activa
        if (cameraToUse != null && (!cameraToUse.gameObject.activeInHierarchy || !cameraToUse.enabled))
        {
            cameraToUse = null;
        }
        
        // Si no hay referencia válida, buscar cámara activa automáticamente
        if (cameraToUse == null && autoAssignActiveCamera)
        {
            cameraToUse = FindActiveCamera();
            if (cameraToUse != null)
            {
                playerCamera = cameraToUse; // Actualizar la referencia
            }
        }
        
        // Si aún no hay cámara válida, usar Camera.main
        if (cameraToUse == null)
        {
            cameraToUse = Camera.main;
        }
        
        // Si tampoco hay Camera.main, usar dirección por defecto
        if (cameraToUse == null)
        {
            Debug.LogWarning("PlayerController: No se encontró cámara válida. Usando dirección por defecto.");
            // Usar dirección por defecto (hacia adelante) si no hay cámara
            _moveInput = new Vector3(horizontal, 0, vertical).normalized;
            return;
        }
        
        Vector3 cameraForward = cameraToUse.transform.forward;
        Vector3 cameraRight = cameraToUse.transform.right;
        
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();
        
        _moveInput = (cameraForward * vertical + cameraRight * horizontal).normalized;
    }

    private void ApplyMovement()
    {
        // Calcular velocidad deseada
        Vector3 targetVelocity = _moveInput * maxSpeed;
        targetVelocity.y = _rb.linearVelocity.y;

        // Interpolación suave de velocidad
        _currentVelocity = Vector3.Lerp(
            _currentVelocity, 
            targetVelocity, 
            (_moveInput.magnitude > 0.1f ? acceleration : deceleration) * Time.fixedDeltaTime
        );

        // Aplicar movimiento
        Vector3 moveDelta = _currentVelocity * Time.fixedDeltaTime;
        _rb.linearVelocity = new Vector3(_currentVelocity.x, _rb.linearVelocity.y, _currentVelocity.z);

        // Rotación hacia la dirección de movimiento
        if (_moveInput.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_moveInput);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, 
                targetRotation, 
                rotationSpeed * Time.fixedDeltaTime
            );
        }
    }

    private void ApplyFriction()
    {
        float currentDrag = _isGrounded ? groundDrag : airDrag;
        float currentFriction = _isGrounded ? friction : friction * airFrictionMultiplier;

        // Fricción cuando no hay input
        if (_moveInput.magnitude < 0.1f)
        {
            Vector3 horizontalVelocity = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
            Vector3 frictionForce = -horizontalVelocity.normalized * currentFriction;
            
            _rb.AddForce(frictionForce, ForceMode.Acceleration);
        }

        // Limitador de velocidad
        Vector2 horizontalVel = new Vector2(_rb.linearVelocity.x, _rb.linearVelocity.z);
        if (horizontalVel.magnitude > maxSpeed)
        {
            horizontalVel = horizontalVel.normalized * maxSpeed;
            _rb.linearVelocity = new Vector3(horizontalVel.x, _rb.linearVelocity.y, horizontalVel.y);
        }

        // Drag variable
        _rb.linearDamping = _isGrounded ? groundDrag : airDrag;
    }

    private void ApplyGravity()
    {
        if (!_isGrounded)
        {
            _rb.AddForce(Vector3.down * gravityMultiplier * Physics.gravity.magnitude, ForceMode.Acceleration);
        }
    }

    private void GroundCheck()
    {
        float rayLength = 0.2f;
        _isGrounded = Physics.Raycast(
            transform.position, 
            Vector3.down, 
            rayLength, 
            groundLayer
        );
    }

    private void HandleJump()
    {
        
        if (Input.GetButtonDown("Jump") && _isGrounded && carriedBox == null)
        {
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            
        }
    }

    // Visualizar ground check en el editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * 0.2f);
    }
    private void HandleBoxInteraction()
{
    Vector3 rayStart = transform.position + Vector3.up * 0.5f;
    if (Physics.Raycast(rayStart, transform.forward, out RaycastHit hit, 1.5f, boxLayer))
    {
        if (hit.collider.TryGetComponent(out BoxController box))
        {
            // Solo empujar si la caja no está siendo agarrada
            if (!box.isGrabbed)
            {
                // Calcular dirección de empuje basada en el input
                Vector3 pushDirection = new Vector3(_moveInput.x, 0, _moveInput.z).normalized;
                
                // Empujar solo si la dirección es válida
                if (pushDirection.magnitude > 0.1f)
                {
                    // Aplicar fuerza al Rigidbody de la caja
                    Rigidbody boxRb = box.GetComponent<Rigidbody>();
                    if (boxRb != null)
                    {
                        boxRb.AddForce(pushDirection * pushForce, ForceMode.Impulse);
                    }
                    
                    // Aplicar retroceso al jugador
                    _rb.AddForce(-pushDirection * pushForce * 0.3f, ForceMode.Impulse);
                }
            }
        }
    }
}

// En el Update del jugador
private void ShowGrabIndicator()
{
    if (carriedBox == null)
    {
        Vector3 rayStart = transform.position + Vector3.up * 0.5f;
        RaycastHit hit;
        if (Physics.Raycast(rayStart, transform.forward, out hit, grabRange, boxLayer))
        {
            if (hit.collider.TryGetComponent<BoxController>(out var box))
            {
                // Mostrar UI o cambiar material
            }
        }
    }
}
public Transform GetNearestBox()
{
    Collider[] boxes = Physics.OverlapSphere(transform.position, grabRange, boxLayer);
    Transform nearest = null;
    float minDistance = Mathf.Infinity;
    
    foreach (var box in boxes)
    {
        float distance = Vector3.Distance(transform.position, box.transform.position);
        if (distance < minDistance)
        {
            minDistance = distance;
            nearest = box.transform;
        }
    }
    return nearest;
}

    /// <summary>
    /// Asigna automáticamente la cámara activa a la referencia del jugador
    /// </summary>
    private void AssignActiveCamera()
    {
        // Buscar la cámara activa en la escena
        Camera activeCamera = FindActiveCamera();
        
        if (activeCamera != null)
        {
            playerCamera = activeCamera;
            Debug.Log($"PlayerController: Cámara automáticamente asignada - {activeCamera.name}");
        }
        else
        {
            Debug.LogWarning("PlayerController: No se encontró ninguna cámara activa para asignar automáticamente.");
        }
    }
    
    /// <summary>
    /// Encuentra la cámara actualmente activa en la escena
    /// </summary>
    private Camera FindActiveCamera()
    {
        // Buscar todas las cámaras en la escena
        Camera[] allCameras = FindObjectsOfType<Camera>();
        
        // Prioridad 1: Buscar cámara con tag "MainCamera" que esté activa
        foreach (Camera cam in allCameras)
        {
            if (cam.CompareTag("MainCamera") && cam.gameObject.activeInHierarchy && cam.enabled)
            {
                return cam;
            }
        }
        
        // Prioridad 2: Buscar la primera cámara activa y habilitada
        foreach (Camera cam in allCameras)
        {
            if (cam.gameObject.activeInHierarchy && cam.enabled)
            {
                return cam;
            }
        }
        
        // Prioridad 3: Usar Camera.main como último recurso
        return Camera.main;
    }
    
    /// <summary>
    /// Método público para forzar la asignación de cámara activa
    /// </summary>
    [ContextMenu("Asignar Cámara Activa")]
    public void ForceAssignActiveCamera()
    {
        AssignActiveCamera();
    }

}