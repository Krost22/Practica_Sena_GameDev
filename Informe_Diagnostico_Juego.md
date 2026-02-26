# Informe de Diagnóstico de Funcionalidad y Lógica del Videojuego

**Evidencia:** GA8-220501088-AA2-EV01  
**Fecha:** 11 de Febrero de 2026  
**Proyecto:** Practica_Sena_GameDev  

---

## Tabla de Contenido
1. [Introducción](#introducción)
2. [Comportamientos y Niveles Verificados](#comportamientos-y-niveles-verificados)
3. [Proceso de Verificación y Tipo de Pruebas](#proceso-de-verificación-y-tipo-de-pruebas)
4. [Lista Priorizada de Hallazgos y Errores](#lista-priorizada-de-hallazgos-y-errores)
5. [Referencias Bibliográficas](#referencias-bibliográficas)

---

## Introducción
El presente informe detalla el proceso de diagnóstico y verificación funcional realizado sobre el componente principal del videojuego: el controlador del jugador (`PlayerController`). El objetivo principal ha sido asegurar la estabilidad de las mecánicas base (movimiento, física e inicialización) mediante la implementación de pruebas automatizadas en el entorno de Unity. Se identifican, clasifican y priorizan los errores encontrados durante el ciclo de desarrollo y pruebas, evaluando su impacto en la lógica y jugabilidad del proyecto.

---

## Comportamientos y Niveles Verificados

En esta etapa de diagnóstico, se han centrado las pruebas en el **Nivel de Gameplay Base**, específicamente en las mecánicas del **Avatar del Jugador**. Los comportamientos verificados incluyen:

1.  **Inicialización del Sistema del Jugador (`PlayerController` Initialization):**
    *   Verificación de la correcta asignación de dependencias críticas al inicio del juego.
    *   Garantía de que componentes esenciales como `Rigidbody` (física), `PlayerMovement` (lógica de movimiento), `BoxInteractionController` (interacción) y `CameraController` (vista) estén presentes y operativos.

2.  **Lógica de Movimiento (`PlayerMovement` Input):**
    *   Verificación de la respuesta del personaje ante las entradas de control (inputs) del usuario.
    *   Validación de que los vectores de movimiento se calculan y aplican correctamente al componente físico.

3.  **Física y Gravedad (`Physics & Gravity`):**
    *   Comprobación de la afectación de la gravedad sobre el jugador.
    *   Verificación de que el personaje no queda flotando o en estados físicos inválidos al estar en el aire.

---

## Proceso de Verificación y Tipo de Pruebas

Para el diagnóstico se implementó una metodología de **Pruebas Automatizadas en Modo de Juego (PlayMode Tests)** utilizando el **Unity Test Framework**.

### Metodología:
*   **Pruebas de Integración (PlayMode):** A diferencia de las pruebas unitarias simples, se optó por ejecutar pruebas que instancian el juego real (GameObjects, Componentes y Física) para simular condiciones idénticas a las que experimentará el usuario final.
*   **Ciclo Arreglar-Probar (Fix-Verify):** Se identificaron errores de configuración (como faltantes de Assembly Definitions) que impedían la compilación, se corrigieron, y se ejecutaron las pruebas para validar la solución.

### Herramientas Utilizadas:
*   **Unity Test Runner:** Para la ejecución y orquestación de los casos de prueba.
*   **NUnit Framework:** Librería base para las aserciones lógicas (Asserts).
*   **Assembly Definition Files (.asmdef):** Para aislar y organizar el código del jugador y permitir su testeo modular.

---

## Lista Priorizada de Hallazgos y Errores

A continuación, se presentan los errores y riesgos identificados, ordenados por su nivel de prioridad y afectación a la funcionalidad del juego.

### 🔴 Prioridad Alta (Críticos - Bloqueantes)

**1. Ausencia de Dependencias Críticas (Componentes Nulos)**
*   **Descripción:** Durante el desarrollo, existía el riesgo latente de que el `PlayerController` intentara acceder a subsistemas (como el `PlayerMovement` o `CameraController`) que no habían sido agregados al objeto del juego, lo que resultaría en errores de tipo `NullReferenceException` en tiempo de ejecución.
*   **Afectación (Lógica):** Si este error ocurre, el juego se detiene o el personaje queda completamente inoperable al inicio de la partida. Rompe la lógica de inicialización.
*   **Estado:** **Mitigado** mediante el test `PlayerController_Initialization_ComponentsExist` y el uso de atributos `[RequireComponent]` en el código.

**2. Fallo en la Respuesta de Movimiento (Input Lag / No Movement)**
*   **Descripción:** Se identificaron casos potenciales donde las entradas del usuario (teclas de movimiento) no se traducían en desplazamiento físico debido a conflictos entre el `Rigidbody` y el cálculo de vectores.
*   **Afectación (Funcional):** Impide la jugabilidad básica. El jugador presiona teclas pero el avatar no responde, rompiendo la interacción principal del juego.
*   **Estado:** **Verificado** con el test `PlayerMovement_Moves_WhenInputGiven`.

### 🟡 Prioridad Media (Importantes - No Bloqueantes)

**3. Errores de Referencias de Ensamblado (Assembly Definitions)**
*   **Descripción:** Al intentar ejecutar las pruebas, se generaron múltiples errores de compilación (`CS0246`) debido a que los scripts de prueba no tenían visibilidad sobre los scripts del juego ni sobre librerías externas (como `TextMeshPro` o `Unity.RenderPipelines`).
*   **Afectación (Desarrollo):** No afecta al jugador final directamente, pero bloquea el flujo de trabajo de calidad y validación. Impide saber si el juego funciona correctamente antes de generar una versión final.
*   **Solución Aplicada:** Se configuraron correctamente los archivos `Scripts.asmdef` y `PlayerPlayModeTest.asmdef` para incluir las referencias necesarias.

### 🟢 Prioridad Baja (Menores - Estéticos/Ajustes)

**4. Comportamiento Físico (Gravedad)**
*   **Descripción:** Necesidad de verificar que la gravedad se aplique de manera consistente.
*   **Afectación (Experiencia):** Si la gravedad falla o es insuficiente, el juego puede sentirse "flotante" (moon jump), afectando la inmersión, aunque no necesariamente rompe la progresión del juego.
*   **Estado:** **Verificado** con el test `PlayerMovement_Gravity_Applied`.

---

## Referencias Bibliográficas
*   Unity Technologies. (2023). *Unity Test Framework Documentation*. Recuperado de: https://docs.unity3d.com/Packages/com.unity.test-framework@1.1/manual/index.html
*   Docs.microsoft.com. (2023). *NUnit Documentation*.
*   SENA. (2024). *Material de Formación: Pruebas y Calidad de Software en Videojuegos*.
