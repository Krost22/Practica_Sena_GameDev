using UnityEngine;

public class BoxInteractionController : MonoBehaviour
{
    [Header("Box Pushing")]
    [SerializeField] private float pushForce = 3f;

    [Header("Grab Settings")]
    [SerializeField] private KeyCode grabKey = KeyCode.E;
    [SerializeField] private float grabRange = 2f;
    [SerializeField] private float grabSpeedMultiplier = 0.7f;
    [SerializeField] private LayerMask boxLayer;
    [SerializeField] private Transform grabPoint; // Punto donde se sostiene la caja

    private BoxController carriedBox;
    private PlayerController playerController;

    // Propiedades públicas
    public BoxController CarriedBox => carriedBox;
    public bool IsCarryingBox => carriedBox != null;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("BoxInteractionController: No se encontró PlayerController en el GameObject");
        }
    }

    void Update()
    {
        HandleGrabInput();
        UpdateCarriedBoxPosition();
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
            Debug.LogWarning("BoxInteractionController: grabPoint no está asignado en el Inspector");
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
                    if (playerController != null)
                    {
                        playerController.SetMovementSpeed(grabSpeedMultiplier);
                    }
                }
            }
        }
    }

    public void ReleaseBox(bool throwIt)
    {
        if (carriedBox != null)
        {
            Vector3 releaseVelocity = Vector3.zero;
            
            if (throwIt)
            {
                releaseVelocity = transform.forward * 10f + Vector3.up * 3f;
            }
            else if (playerController != null)
            {
                // Mantener velocidad horizontal del jugador
                Vector3 currentVel = playerController.CurrentVelocity;
                releaseVelocity = new Vector3(currentVel.x, 0, currentVel.z);
            }

            carriedBox.Release(releaseVelocity);
            
            // Restaurar velocidad original
            if (playerController != null)
            {
                playerController.ResetMovementStats();
            }
            
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

    public bool IsNearBox()
    {
        if (carriedBox != null) return false;
        
        Vector3 rayStart = transform.position + Vector3.up * 0.5f;
        RaycastHit hit;
        if (Physics.Raycast(rayStart, transform.forward, out hit, grabRange, boxLayer))
        {
            if (hit.collider.TryGetComponent<BoxController>(out var box))
            {
                return !box.isOnTarget;
            }
        }
        return false;
    }

    // Empujar caja cuando el jugador la colisiona
    public void HandleBoxPushing(Vector3 moveInput)
    {
        if (carriedBox != null) return; // No empujar si estamos cargando la caja
        
        Vector3 rayStart = transform.position + Vector3.up * 0.5f;
        if (Physics.Raycast(rayStart, transform.forward, out RaycastHit hit, 1.5f, boxLayer))
        {
            if (hit.collider.TryGetComponent(out BoxController box))
            {
                // Solo empujar si la caja no está siendo agarrada
                if (!box.isGrabbed)
                {
                    // Calcular dirección de empuje basada en el input
                    Vector3 pushDirection = new Vector3(moveInput.x, 0, moveInput.z).normalized;
                    
                    // Empujar solo si la dirección es válida
                    if (pushDirection.magnitude > 0.1f)
                    {
                        // Aplicar fuerza al Rigidbody de la caja
                        Rigidbody boxRb = box.GetComponent<Rigidbody>();
                        if (boxRb != null)
                        {
                            boxRb.AddForce(pushDirection * pushForce, ForceMode.Impulse);
                        }
                    }
                }
            }
        }
    }
}

