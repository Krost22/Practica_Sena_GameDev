using System.Diagnostics.Contracts;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class BoxController : MonoBehaviour
{
    [Header("Estado (Solo lectura)")]
    public bool isOnTarget;
    public bool isGrabbed;
    
    [Header("Configuración")]
    public Transform grabAnchor; 
    
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Rigidbody rb;
    private Transform grabber;
    private Transform currentTarget;

    [Header("Public Events")]
    public UnityEvent OnKeyItemGrab;
    public UnityEvent OnKeyItemRelease;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        rb.maxAngularVelocity = 0.1f;
    }

    public void Grab(Transform newGrabber)
    {
        isGrabbed = true;
        this.grabber = newGrabber;
        
        rb.isKinematic = true; // Se mantiene en mano
        rb.interpolation = RigidbodyInterpolation.None;
        
        transform.SetParent(grabber);
        transform.position = grabber.position;
        transform.rotation = grabber.rotation;

        OnKeyItemGrab?.Invoke();

        if (isOnTarget)
        {
            isOnTarget = false;
            currentTarget = null;
        }
    }

    public void Release(Vector3 releaseVelocity)
    {
        isGrabbed = false;
        transform.SetParent(null);
        
        // Retornamos las fisicas nativas para que se mueva a su entorno 
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        
        // Unity 6 Native: linearVelocity en lugar de velocity
        rb.linearVelocity = releaseVelocity * 0.8f;

        OnKeyItemRelease?.Invoke();
    }

    public void PlaceOnTarget(Transform targetTransform)
    {
        if (isGrabbed) return; 

        isOnTarget = true;
        currentTarget = targetTransform;
        
        // Evitamos posibles tirones físicos del Hinge o Colisiones al hacer snap
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        // Asignamos la posicion y emparentamos inteligentemente a la base objetivo
        transform.position = targetTransform.position;
        transform.rotation = targetTransform.rotation;
        transform.SetParent(targetTransform); 
    }

    public void ResetPosition()
    {
        if (isGrabbed) return;
        
        isOnTarget = false;
        currentTarget = null;
        transform.SetParent(null);
        
        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        transform.position = initialPosition;
        transform.rotation = initialRotation;
    }
}