using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class RigidbodyPusher : MonoBehaviour
{
    [Header("Configuración de Empuje")]
    [Tooltip("La fuerza base con la que el jugador empujará objetos físicos")]
    public float pushForce = 2.0f;
    
    [Tooltip("Multiplicador extra cuando interactuamos con puertas que tienen bisagras (Hinge Joint)")]
    public float hingeMultiplier = 2.5f;

    // Esta función es nativa de Unity. Se ejecuta cada vez que el CharacterController choca con algo en movimiento
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Obtenemos el Rigidbody del objeto con el que entramos en contacto
        Rigidbody body = hit.collider.attachedRigidbody;

        // Validamos que el objeto tenga físicas, y que su Kinematic esté apagado (no es inamovible)
        if (body == null || body.isKinematic)
        {
            return;
        }

        // Evitar intentar empujar objetos sobre los que estamos de pie, empujándolos hacia el piso
        if (hit.moveDirection.y < -0.3f)
        {
            return;
        }

        // Calculamos la dirección del empuje horizontal (plano XZ), basado en hacia donde avanza el personaje
        Vector3 pushDirection = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
        
        // Si el objeto tiene un Hinge Joint (como tu puerta), calculamos una fuerza extra 
        // ya que las bisagras son pesadas de mover.
        float finalForce = pushForce;
        if (body.GetComponent<HingeJoint>() != null)
        {
            finalForce *= hingeMultiplier;
        }

        // Aplicamos la fuerza de impulso al Rigidbody en el punto donde nuestro personaje hizo contacto
        body.AddForceAtPosition(pushDirection * finalForce, hit.point, ForceMode.Impulse);
    }
}
