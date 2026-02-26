using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
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

    private Rigidbody _rb;
    private Vector3 _moveInput;
    private Vector3 _currentVelocity;
    private bool _isGrounded;
    
    // Propiedades públicas para modificar velocidad desde otros sistemas
    public float MaxSpeed 
    { 
        get => maxSpeed; 
        set => maxSpeed = value; 
    }
    
    public float Acceleration 
    { 
        get => acceleration; 
        set => acceleration = value; 
    }
    
    public float Deceleration 
    { 
        get => deceleration; 
        set => deceleration = value; 
    }
    
    public bool IsGrounded => _isGrounded;
    public Vector3 CurrentVelocity => _currentVelocity;
    
    private float originalMaxSpeed;
    private float originalAcceleration;
    private float originalDeceleration;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;
        originalMaxSpeed = maxSpeed;
        originalAcceleration = acceleration;
        originalDeceleration = deceleration;
    }

    void FixedUpdate()
    {
        ApplyMovement();
        ApplyFriction();
        ApplyGravity();
        GroundCheck();
    }

    public void SetMovementInput(Vector3 input)
    {
        _moveInput = input;
    }

    public void ApplyJump()
    {
        if (_isGrounded)
        {
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    public void ApplyKnockback(Vector3 direction, float force)
    {
        _rb.AddForce(direction * force, ForceMode.Impulse);
    }

    public void ResetMovementStats()
    {
        maxSpeed = originalMaxSpeed;
        acceleration = originalAcceleration;
        deceleration = originalDeceleration;
    }

    public void SetMovementSpeed(float multiplier)
    {
        maxSpeed = originalMaxSpeed * multiplier;
        acceleration = originalAcceleration * multiplier;
        deceleration = originalDeceleration * multiplier;
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

    // Visualizar ground check en el editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * 0.2f);
    }
}
