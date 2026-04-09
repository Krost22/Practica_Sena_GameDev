using UnityEngine;
using UnityEngine.Events;

public class BoxInteractionController : MonoBehaviour
{
    [Header("Box Pushing")]
    [SerializeField] private float pushForce = 3f;

    [Header("Grab Settings")]
    [SerializeField] private KeyCode grabKey = KeyCode.E;
    [SerializeField] private float grabRange = 3.5f; // Mayor rango de distancia
    [SerializeField] private float grabRadius = 0.75f; // Radio para facilitar apuntar a la caja
    [SerializeField] private float grabSpeedMultiplier = 0.7f;
    [SerializeField] private LayerMask boxLayer;
    [SerializeField] private Transform grabPoint; // Punto donde se sostiene la caja

    [Header("Events")]
    public UnityEvent<GameObject> OnBoxTargeted;
    public UnityEvent OnBoxUntargeted;

    private GameObject currentTargetBox;

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
        CheckTargetBox();
        HandleGrabInput();
        UpdateCarriedBoxPosition();
    }

    private BoxController FindTargetBox()
    {
        Vector3 rayStart = transform.position + Vector3.up * 0.5f;
        
        // --- 1. Chequeo de proximidad (OverlapSphere) ---
        // Esto soluciona el problema de Unity donde el SphereCast ignora objetos
        // si ya estamos "dentro" o tocando sus colliders al momento de lanzarlo.
        Collider[] overlaps = Physics.OverlapSphere(rayStart, grabRadius, boxLayer);
        BoxController closestBox = null;
        float closestDist = float.MaxValue;

        foreach (var col in overlaps)
        {
            if (col.TryGetComponent<BoxController>(out var box) && !box.isOnTarget)
            {
                // Validar que la caja esté "frente" al jugador y no en su espalda
                Vector3 dirToBox = (box.transform.position - transform.position).normalized;
                if (Vector3.Dot(transform.forward, dirToBox) > 0.1f) // 0.1f significa "ligeramente al frente"
                {
                    float dist = Vector3.Distance(transform.position, box.transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestBox = box;
                    }
                }
            }
        }

        // Si encontramos una caja tocándonos, la devolvemos inmediatamente
        if (closestBox != null) return closestBox;

        // --- 2. Chequeo a distancia (SphereCast) ---
        // Si no estamos tocando nada, lanzamos el "tubo" hacia adelante
        if (Physics.SphereCast(rayStart, grabRadius, transform.forward, out RaycastHit hit, grabRange, boxLayer))
        {
            if (hit.collider.TryGetComponent<BoxController>(out var box) && !box.isOnTarget)
            {
                return box;
            }
        }

        return null;
    }

    private void CheckTargetBox()
    {
        if (carriedBox != null)
        {
            if (currentTargetBox != null)
            {
                OnBoxUntargeted?.Invoke();
                currentTargetBox = null;
            }
            return;
        }

        BoxController box = FindTargetBox();
        
        if (box != null)
        {
            if (currentTargetBox != box.gameObject)
            {
                if (currentTargetBox != null) OnBoxUntargeted?.Invoke();
                currentTargetBox = box.gameObject;
                OnBoxTargeted?.Invoke(currentTargetBox);
            }
            return;
        }
        
        // Si no hay caja enfrente, disparar evento de deselección
        if (currentTargetBox != null)
        {
            OnBoxUntargeted?.Invoke();
            currentTargetBox = null;
        }
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
        
        BoxController box = FindTargetBox();
        
        if (box != null)
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
        return FindTargetBox() != null;
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

    private void OnDrawGizmosSelected()
    {
        // Usamos un color SÓLIDO y llamativo (Cyan) para que no se pierda contra la lava o luces amarillas.
        Gizmos.color = Color.cyan;
        
        Vector3 rayStart = transform.position + Vector3.up * 0.5f;
        Vector3 rayEnd = rayStart + transform.forward * grabRange;

        // Esfera en la base (jugador)
        Gizmos.DrawWireSphere(rayStart, grabRadius);
        
        // Esfera en la punta (límite de alcance)
        Gizmos.DrawWireSphere(rayEnd, grabRadius);
        
        // Líneas laterales para simular el tubo o cilindro del SphereCast
        Gizmos.DrawLine(rayStart + transform.up * grabRadius, rayEnd + transform.up * grabRadius);
        Gizmos.DrawLine(rayStart - transform.up * grabRadius, rayEnd - transform.up * grabRadius);
        Gizmos.DrawLine(rayStart + transform.right * grabRadius, rayEnd + transform.right * grabRadius);
        Gizmos.DrawLine(rayStart - transform.right * grabRadius, rayEnd - transform.right * grabRadius);
    }
}

