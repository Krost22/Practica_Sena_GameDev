using UnityEngine;

public class BoxController : MonoBehaviour
{
    public bool isOnTarget;
    public bool isGrabbed;
    public Transform grabAnchor; // Punto de agarre (asignar en prefab)
    [SerializeField] private LayerMask targetLayer; // Layer del target
    
    private Vector3 lastValidPosition;
    private Rigidbody rb;
    private Transform grabber;
    private Transform currentTarget;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        lastValidPosition = transform.position;
        rb.maxAngularVelocity = 0.1f;
    }

    public void Grab(Transform grabber)
    {
        isGrabbed = true;
        this.grabber = grabber;
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.None;
        transform.SetParent(grabber);
        transform.position = grabber.position;
        transform.rotation = grabber.rotation;
    }

    public void Release(Vector3 releaseVelocity)
    {
        isGrabbed = false;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        transform.SetParent(null);
        
        // Aplicar inercia al soltar
        rb.linearVelocity = releaseVelocity * 0.8f;
        
        // Guardar posición para posible reset
        lastValidPosition = transform.position;
    }

    public void PlaceOnTarget()
    {
        if (isOnTarget && currentTarget != null)
        {
            // Detener toda la velocidad antes de hacer cinemático
            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            
            // Hacer cinemático
            rb.isKinematic = true;
            
            // Colocar la caja exactamente en el target
            transform.position = currentTarget.position;
            transform.rotation = currentTarget.rotation;
            
            // Aquí podrías añadir efectos visuales
        }
    }

    public void ResetPosition()
    {
        Release(Vector3.zero);
        transform.position = lastValidPosition;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    // Detectar cuando entra en contacto con el target
    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & targetLayer) != 0)
        {
            isOnTarget = true;
            currentTarget = other.transform;
            PlaceOnTarget();
        }
    }

    // Detectar cuando sale del target
    private void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & targetLayer) != 0)
        {
            isOnTarget = false;
            currentTarget = null;
            
            // Reactivar física si no está siendo agarrada
            if (!isGrabbed)
            {
                rb.isKinematic = false;
            }
        }
    }

    // Alternativa usando OnCollisionEnter si prefieres colisiones físicas
    private void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & targetLayer) != 0)
        {
            isOnTarget = true;
            currentTarget = collision.transform;
            PlaceOnTarget();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & targetLayer) != 0)
        {
            isOnTarget = false;
            currentTarget = null;
            
            // Reactivar física si no está siendo agarrada
            if (!isGrabbed)
            {
                rb.isKinematic = false;
            }
        }
    }
}