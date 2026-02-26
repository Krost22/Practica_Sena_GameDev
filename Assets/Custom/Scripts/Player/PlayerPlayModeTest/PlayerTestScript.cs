using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayerTestScript
{
    private GameObject playerGameObject;
    private PlayerController playerController;
    private PlayerMovement playerMovement;

    [SetUp]
    public void Setup()
    {
        // Se ejecuta antes de cada test
        playerGameObject = new GameObject("Player");
        // Agregar PlayerController automáticamente agrega las dependencias por RequireComponent
        playerController = playerGameObject.AddComponent<PlayerController>();
        playerMovement = playerGameObject.GetComponent<PlayerMovement>();

        // Aseguramos que haya una cámara para el CameraController
        if (Camera.main == null)
        {
            var cameraGO = new GameObject("Main Camera");
            cameraGO.AddComponent<Camera>();
            cameraGO.tag = "MainCamera";
        }
    }

    [TearDown]
    public void Teardown()
    {
        // Se ejecuta después de cada test
        if (playerGameObject != null)
        {
            Object.Destroy(playerGameObject);
        }
    }

    // Un Test simple para verificar que los componentes se inicializan correctamente
    [Test]
    public void PlayerController_Initialization_ComponentsExist()
    {
        // Assert
        Assert.IsNotNull(playerController, "El PlayerController debería existir");
        Assert.IsNotNull(playerGameObject.GetComponent<PlayerMovement>(), "El PlayerMovement debería haber sido agregado automáticamente");
        Assert.IsNotNull(playerGameObject.GetComponent<BoxInteractionController>(), "El BoxInteractionController debería haber sido agregado automáticamente");
        Assert.IsNotNull(playerGameObject.GetComponent<CameraController>(), "El CameraController debería haber sido agregado automáticamente");
        Assert.IsNotNull(playerGameObject.GetComponent<Rigidbody>(), "El Rigidbody debería haber sido agregado automáticamente");
    }

    // Un UnityTest permite saltar frames (es una corutina)
    // Probamos si el PlayerMovement realmente mueve el objeto cuando se le da input
    [UnityTest]
    public IEnumerator PlayerMovement_Moves_WhenInputGiven()
    {
        // Arrange
        Vector3 inputDirection = Vector3.forward;
        Vector3 initialPosition = playerGameObject.transform.position;
        
        // Act
        // Simulamos input directamente en el componente de movimiento
        playerMovement.SetMovementInput(inputDirection);
        
        // Esperamos un tiempo físico para que el Rigidbody se mueva
        yield return new WaitForSeconds(0.5f);

        // Assert
        Assert.AreNotEqual(initialPosition, playerGameObject.transform.position, "El jugador debería haberse movido después de recibir input");
    }

    // Prueba para verificar que la gravedad funciona
    [UnityTest]
    public IEnumerator PlayerMovement_Gravity_Applied()
    {
        // Arrange
        // Movemos al jugador arriba
        playerGameObject.transform.position = new Vector3(0, 10, 0);
        float initialY = playerGameObject.transform.position.y;

        // Act
        // Esperamos para que la física actúe
        yield return new WaitForSeconds(0.5f);

        // Assert
        Assert.Less(playerGameObject.transform.position.y, initialY, "El jugador debería caer debido a la gravedad");
    }
}
