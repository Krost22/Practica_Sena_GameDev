using UnityEngine;

public class MainMenuCamera : MonoBehaviour
{
    [Header("Configuración de Rotación")]
    [SerializeField] private float velocidadRotacion = 10f;
    [SerializeField] private bool rotarEnX = false;
    [SerializeField] private bool rotarEnY = true;
    [SerializeField] private bool rotarEnZ = false;
    
    // Variables públicas para modificar la velocidad desde el Inspector
    public float VelocidadRotacion 
    { 
        get { return velocidadRotacion; } 
        set { velocidadRotacion = value; } 
    }
    
    public bool RotarEnX 
    { 
        get { return rotarEnX; } 
        set { rotarEnX = value; } 
    }
    
    public bool RotarEnY 
    { 
        get { return rotarEnY; } 
        set { rotarEnY = value; } 
    }
    
    public bool RotarEnZ 
    { 
        get { return rotarEnZ; } 
        set { rotarEnZ = value; } 
    }

    void Start()
    {
        // Inicialización si es necesaria
    }

    void Update()
    {
        // Rotar la cámara en 360 grados según los ejes configurados
        Vector3 rotacion = Vector3.zero;
        
        if (rotarEnX)
            rotacion.x = velocidadRotacion * Time.deltaTime;
        if (rotarEnY)
            rotacion.y = velocidadRotacion * Time.deltaTime;
        if (rotarEnZ)
            rotacion.z = velocidadRotacion * Time.deltaTime;
            
        transform.Rotate(rotacion);
    }
}
