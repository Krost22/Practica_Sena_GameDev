# AGENTS.md — Practica_Sena_GameDev

Guía para agentes IA (y humanos) que trabajen en este proyecto. Lee esto antes
de tocar nada.

---

## 1. Visión del juego

**Puzzle game con cámaras estáticas estilo Resident Evil clásico.**

- Vista: cámaras fijas que cambian al entrar en triggers (no cámara libre en
  tercera persona). El movimiento del jugador es relativo a la cámara activa.
- Género: puzzle / aventura con peligros ambientales (lava, plataformas que se
  caen, plataformas temporales). **Sin enemigos con IA** (decisión de diseño).
- Mecánica signature: **slow-mo local** (`LocalTimeManager` + `ITimeScalable`)
  que ralentiza objetos del escenario, no a todo el juego.
- Alcance objetivo: **3-5 niveles + final** con progresión e historia.

---

## 2. Stack técnico

| Item | Valor |
|------|-------|
| Motor | **Unity 6000.4.2f1** (Unity 6) |
| Render pipeline | **URP 17.4.0** |
| C# | C# 9 (Unity 6) |
| Input | **Input System 1.19.0** (migración en curso desde legacy `Input`) |
| Cámaras | Sistema custom (`CameraManager` + `CameraTrigger`), NO Cinemachine |
| Cinemachine | 3.1.4 instalado pero **no usado** (no tocar salvo decisión explícita) |
| Timeline | 1.8.12 instalado, sin usar todavía |
| UI | uGUI + TextMeshPro |
| Outline | QuickOutline (asset de terceros) |
| Escenarios | LowPolyDungeons (CatBorg Studio), Pure Poly, Stylized Lava |
| Personaje | RPG Tiny Hero Duo / StarterAssets ThirdPersonController |
| MCP | `com.unity.pipeline` 0.5.0-exp.1 + `com.coplaydev.unity-mcp` (MCP for Unity) |
| Pipeline CLI | `unity` CLI (beta) — ver sección 7 |

### APIs de Unity 6 a usar (no las legacy)

- `Rigidbody.linearVelocity` (NO `rb.velocity`).
- `FindAnyObjectByType<T>()` / `FindFirstObjectByType<T>()` (NO `FindObjectOfType`).
- `FindObjectsByType<T>()` **sin** `FindObjectsSortMode` (la sobrecarga con sort
  está deprecada — ver warnings actuales en `CameraManager` y `CameraController`).
- `Object.FindObjectsByType<T>(FindObjectsInactive)` si necesitas incluir
  inactivos.

---

## 3. Estructura del repositorio

```
Practica_Sena_GameDev/
├── AGENTS.md                 ← este archivo
├── Assets/
│   ├── Custom/               ← CÓDIGO Y ASSETS PROPIOS (todo lo nuevo va aquí)
│   │   ├── Animations/
│   │   ├── Materials/
│   │   ├── Prefabs/
│   │   ├── Scenes/
│   │   │   ├── 0_MAINMENU.unity
│   │   │   ├── 1_Level1.unity    ← nivel principal en desarrollo
│   │   │   ├── SampleScene.unity
│   │   │   └── Tarea.unity
│   │   ├── Scripts/
│   │   │   ├── Scripts.asmdef    ← Assembly Definition del código propio
│   │   │   ├── Box Puzzle/       ← puzzle de cajas → targets → puerta
│   │   │   ├── Cameras/          ← sistema de cámaras estáticas
│   │   │   ├── Player/           ← controlador, interacción, cámara del player
│   │   │   ├── Puzzles/          ← CrumblingPlatform
│   │   │   ├── RedBall Puzzle/   ← puzzle de botón que spawnea bola → meta
│   │   │   ├── Recovery/         ← RecoverySystem (teletransporte al caer)
│   │   │   ├── SceneManager/     ← SceneChanger
│   │   │   ├── Time/             ← slow-mo local + UI + post-FX
│   │   │   ├── UI/               ← MainMenuUI
│   │   │   └── TemporalPlataform.cs
│   │   ├── Shaders/
│   │   ├── Skybox/
│   │   ├── Sounds/
│   │   └── VFX/
│   ├── AN Interactive Physical Door Pack/   ← TERCEROS: no modificar
│   ├── CatBorg Studio/                      ← TERCEROS: no modificar
│   ├── Fantasy Skybox FREE/                 ← TERCEROS: no modificar
│   ├── GUIPackCartoon/                      ← TERCEROS: no modificar
│   ├── LowPolyDungeons(Lite)/               ← TERCEROS: no modificar
│   ├── QuickOutline/                        ← TERCEROS: no modificar
│   ├── RPG Monster DUO PBR Polyart/         ← TERCEROS: no modificar
│   ├── RPG Tiny Hero Duo/                   ← TERCEROS: no modificar
│   ├── StarterAssets/                       ← TERCEROS: no modificar
│   ├── Stylized Lava Materials/             ← TERCEROS: no modificar
│   ├── TextMesh Pro/                        ← TERCEROS: no modificar
│   ├── ToonyTinyPeople/                     ← TERCEROS: no modificar
│   └── VoxelAnimals/                        ← TERCEROS: no modificar
├── Packages/manifest.json
├── ProjectSettings/ProjectVersion.txt
└── Informe_Diagnostico_Juego.md   ← informe académico SENA (no borrar)
```

### Regla de oro de carpetas

- **Todo código/asset nuevo va en `Assets/Custom/`**, organizado por sistema.
- **No modificar assets de terceros** (carpetas listadas arriba). Si necesitas
  adaptar un prefab de tercero, crea una variante o un wrapper en `Custom/`.
- Los scripts se agrupan por sistema en subcarpetas de `Custom/Scripts/`.

---

## 4. Arquitectura de scripts (mapa actual)

### Cámaras (estilo RE estáticas)
- `CameraManager` — orquesta lista de triggers + cámaras, elige cámara inicial.
- `CameraTrigger` — al entrar el jugador (tag "Player") activa su `targetCamera`
  y desactiva las demás cámaras de triggers.
- `CameraController` (en el Player) — calcula dirección de movimiento relativa
  a la cámara activa; busca cámara activa automáticamente.
- `MainMenuCamera` — rotación 360° para el menú.

### Jugador
- `PlayerController` — `CharacterController` + gravedad + salto + animación.
  Usa `[RequireComponent]` para `BoxInteractionController` y `CameraController`.
  Destruye cualquier `Rigidbody` residual al arrancar (evita peleas físicas).
- `BoxInteractionController` — agarra (E) y empuja cajas. Usa `OverlapSphere` +
  `SphereCast` para encontrar caja objetivo. Dispara `UnityEvent` al
  targetear/destruir.
- `RigidbodyPusher` — empuja Rigidbodies al colisionar el CharacterController
  (multiplicador extra para puertas con `HingeJoint`).
- `CameraController` — ver arriba.

### Puzzles
- **Box Puzzle**: `BoxController` (caja física, grab/release/snap a target) +
  `TargetController` (snap cuando la caja correcta entra, avisa al manager) +
  `PuzzleManager` (verifica todos los targets ocupados → abre puerta).
- **Red Ball Puzzle**: `SpawnButton` (botón físico con `ConfigurableJoint` que
  spawnea una bola al ser presionado) + `RedBallGoal` (meta que al tocar la bola
  desactiva el botón).
- `CrumblingPlatform` — se cae al pisarla (implementa `ITimeScalable`).
- `TemporalPlataform` — fade in/out cíclico con shader `_Transparency`.

### Slow-mo local
- `ITimeScalable` — interfaz: `SetTimeScale(float)`.
- `LocalTimeManager` — activa slow-mo con tecla, aplica el scale a todos los
  `ITimeScalable` registrados, gestiona duración + cooldown, dispara eventos
  (`SlowStarted`, `SlowEnded`, `CooldownStarted`, `CooldownEnded`).
- `CooldownUI` — barra radial + texto del cooldown.
- `SlowMoPostFX` — modifica Bloom/Vignette/ChromaticAberration durante slow-mo.
- `CameraKick` — (en `TimeManager`) efecto de cámara al activar.

### Recuperación
- `RecoverySystem` — trigger que teletransporta al jugador (y caja) a puntos de
  recuperación al caer a lava. Desactiva el `CharacterController` durante el
  teleport (requisito de Unity para CC).

### Escenas / UI
- `SceneChanger` — carga escena por índice (StartGame → escena 1).
- `MainMenuUI` — toggle entre menú principal y opciones.

---

## 5. Convenciones de código

- **Naming**: mezcla español/inglés histórica. Mantener el idioma del archivo que
  edites. Para archivos nuevos, preferir inglés para clases/métodos, español
  para `Header`/`Tooltip` (costumbre del proyecto).
- **Desacoplamiento**: usar `UnityEvent` para comunicación entre sistemas (ver
  `BoxController.OnKeyItemGrab`, `PuzzleManager.OnPuzzleComplete`).
- **Slow-mo**: cualquier script que tenga movimiento/tiempo propio y deba ser
  afectado por el slow-mo debe implementar `ITimeScalable` y registrarse en el
  `LocalTimeManager` (arrastrar su GameObject raíz a `groupsToSlow`).
- **RequireComponent**: usar `[RequireComponent(typeof(X))]` para dependencias
  fuertes (ver `PlayerController`, `BoxController`, `TargetController`).
- **Tags importantes**: `Player`, `Box`, `RedBall`, `ButtonTrigger`.
- **Layers**: Layer 7 = `Box`, Layer 8 = `Target` (verificar en TagManager).
- **No `Debug.Log` en hot paths** (`Update`, `OnTriggerStay`, `CheckPuzzleState`).
  Usar `debugMode` bool o `#if UNITY_EDITOR`.
- **No `FindObjectsByType` en `Update`** — cachear en `Start`/`Awake` o usar
  referencias serializadas.
- **No editar `.unity`/`.prefab`/`.asset` a mano si el Editor está conectado**
  (ver sección 7). Usar el MCP/CLI.

---

## 6. Problemas conocidos (TODO del plan)

1. **Conflicto de tecla E**: `BoxInteractionController.grabKey` y
   `LocalTimeManager.activationKey` ambos = `KeyCode.E`. Se resuelve con la
   migración a Input System (acciones distintas).
2. **Input híbrido**: Input System instalado pero el código usa legacy
   `Input.GetAxisRaw`/`GetKeyDown`. Migración en curso.
3. **Cámaras ineficientes**:
   - `CameraController.FindActiveCamera` busca todas las cámaras cada frame.
   - `CameraManager.DeactivateAllCameras` desactiva TODAS las cámaras (rompe UI).
   - `CameraManager.GetTriggerCamera` usa **reflexión** para leer un campo
     privado — exponer getter público en `CameraTrigger`.
   - `SmoothCameraTransition` está vacío (solo `WaitForSeconds(0.1f)`).
4. **Warnings de APIs obsoletas**: `FindObjectsByType(...FindObjectsSortMode)`,
   `FindFirstObjectByType`, `FindObjectOfType` en `CameraManager`,
   `CameraController`, `TargetController`, y assets de terceros.
5. **`GameManager` sin lógica**: el GameObject solo tiene `PuzzleManager`.
   Falta un `GameManager` real con estado global y progresión.
6. **Sin sistemas core**: no hay guardado, inventario, audio manager, pausa,
   objetivos, ni sistema de vidas/daño.
7. **Puzzles desconectados**: `PuzzleManager` (Box) y `RedBallGoal` (Red Ball)
   no están unificados bajo un sistema de progresión de nivel.
8. **`TemporalPlataform` no implementa `ITimeScalable`** (inconsistencia con
   `CrumblingPlatform`).
9. **`Debug.Log` masivo** en `PuzzleManager`, `TargetController`, `CameraTrigger`,
   `CameraManager`, `SpawnButton`.
10. **`TargetController.OnTriggerStay`** se ejecuta cada frame aunque ya tenga
    guard `isOccupied` — mover snap a `OnTriggerEnter`.

---

## 7. Flujo de trabajo con Unity (CLI + MCP)

### Estado del Editor

El proyecto tiene `com.unity.pipeline` instalado. Para verificar conexión:

```powershell
unity status --format json
```

Si `state: "ready"` hay un Editor conectado y **se deben usar las herramientas
MCP / CLI para editar escenas**, no editar YAML a mano.

### Regla crítica

> **Nunca editar `.unity`, `.prefab` ni `.asset` a mano si `unity status`
> muestra un Editor conectado.** Los fileIDs/GUIDs se asignan mal a mano y el
> Editor no ve los cambios hasta reimport. Usar `manage_scene`,
> `manage_gameobject`, `manage_asset`, o `execute_code` del MCP.

Excepción: si el Editor está en **Safe Mode** por errores de compilación,
editar el C# para arreglarlos SÍ es correcto (ver `unity pipeline list`).

### Herramientas MCP disponibles (servidor `unityMCP`)

- `manage_scene` — `get_hierarchy`, cargar/guardar escenas.
- `manage_gameobject` — CRUD de GameObjects y componentes.
- `manage_asset` — buscar/crear/Modificar assets.
- `find_gameobjects` — buscar por nombre/tag/layer/component/path/id.
- `execute_code` — ejecutar C# arbitrario en el Editor (compile in-memory).
- `execute_menu_item` — lanzar items de menú de Unity.
- `read_console` — leer logs de la consola (sin filtro por tipo: usar
  `filter_type` NO válido; leer todo y filtrar cliente-side).
- `batch_execute` — agrupar múltiples comandos (10-100x más rápido).
- Recursos: `mcpforunity://editor/state`, `mcpforunity://project/info`,
  `mcpforunity://scene/gameobject/{id}`, etc.

### Ciclo al editar scripts C#

1. Editar `.cs` con herramientas de archivo.
2. El Editor recompila automáticamente. **Esperar a que termine**:
   - Poll `mcpforunity://editor/state` → `data.compilation.is_compiling`.
   - O `read_console` para ver errores.
3. Solo después de compilar sin errores se pueden usar nuevos componentes/tipos
   en `manage_gameobject`.

### Comandos CLI útiles

```powershell
unity status --format json              # estado del Editor
unity pipeline list                     # instancias + Safe Mode check
unity open .                            # abrir el proyecto
unity build . --target StandaloneWindows64 --execute-method Builder.PerformBuild
unity test . --mode PlayMode --report-format junit --output test-results.xml
```

---

## 8. Comandos de verificación

No hay un test runner configurado formalmente más allá del `Informe_Diagnostico`
que menciona `PlayerPlayModeTest.asmdef`. Tras cambios:

1. `read_console` (MCP) → confirmar 0 errores de compilación.
2. Si hay tests PlayMode: `unity test . --mode PlayMode`.
3. Para validar runtime: entrar en Play mode con `manage_editor` y observar
   consola, o probar la mecánica manualmente.

---

## 9. Plan de desarrollo (resumen)

Fases aprobadas (detalle en el plan de la sesión):

- **FASE 0** ✅ Crear este `AGENTS.md`.
- **FASE 1** Arreglos críticos: conflicto tecla E, migrar a Input System,
  optimizar cámaras, limpiar Debug.Log, optimizar TargetController.
- **FASE 2** Sistemas core: GameManager, SaveSystem, inventario, AudioManager,
  pausa, objetivos, vidas.
- **FASE 3** Mejoras de Level 1: jerarquía, progresión de puzzles unificada,
  recovery con penalización, TemporalPlataform + slow-mo, puerta de niebla,
  iluminación.
- **FASE 4** Niveles 2-5 + final (progresión por mecánicas).
- **FASE 5** Pulido: victory/defeat, créditos, settings, optimización, build PC.

---

## 10. Notas

- `Informe_Diagnostico_Juego.md` es entregable académico SENA — **no borrar**.
- `repomix-output.xml` es un export de repomix, puede ignorarse/borrarse.
- El proyecto está en OneDrive (`C:\Users\eduar\OneDrive\...`) — cuidado con
  locks de archivo si Unity y el agente editan a la vez.
