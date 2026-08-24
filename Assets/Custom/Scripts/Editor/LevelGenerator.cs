#if UNITY_EDITOR
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// Generador de niveles: crea las 5 escenas nuevas (Level 2-5 + Final)
/// con geometría básica, luces, cámaras estáticas, puzzles y sistemas core.
///
/// Uso: Tools > Generate Levels
/// </summary>
public class LevelGenerator : EditorWindow
{
    [MenuItem("Tools/Generate Levels")]
    static void GenerateAll()
    {
        string[] scenePaths = new string[]
        {
            "Assets/Custom/Scenes/2_Level2.unity",
            "Assets/Custom/Scenes/3_Level3.unity",
            "Assets/Custom/Scenes/4_Level4.unity",
            "Assets/Custom/Scenes/5_Level5.unity",
            "Assets/Custom/Scenes/6_Final.unity"
        };

        GameManager.LevelId[] levelIds = new GameManager.LevelId[]
        {
            GameManager.LevelId.Level2,
            GameManager.LevelId.Level3,
            GameManager.LevelId.Level4,
            GameManager.LevelId.Level5,
            GameManager.LevelId.Final
        };

        string[] objectiveTexts = new string[]
        {
            "Empuja las cajas a los objetivos",
            "Guía la bola roja hasta la meta",
            "Cruza el puente antes de que colapse",
            "Usa el slow-mo para cruzar las plataformas temporales",
            "Resuelve todos los puzzles para escapar"
        };

        for (int i = 0; i < scenePaths.Length; i++)
        {
            GenerateScene(scenePaths[i], levelIds[i], objectiveTexts[i], i + 2);
        }

        // Actualizar Build Settings
        UpdateBuildSettings();

        AssetDatabase.SaveAssets();
        Debug.Log("=== Generación de niveles completa ===");
    }

    static void GenerateScene(string path, GameManager.LevelId levelId, string objective, int levelNumber)
    {
        // Crear escena vacía
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // === 1. Geometría básica ===
        CreateBasicGeometry(levelNumber);

        // === 2. Luces ===
        CreateLights();

        // === 3. Cámaras estáticas + CameraManager ===
        CreateCameraSystem(levelNumber);

        // === 4. Player spawn + LevelBootstrap ===
        CreateBootstrap(levelId, objective);

        // === 5. Puzzle específico del nivel ===
        CreatePuzzle(levelNumber);

        // === 6. LevelProgression ===
        CreateLevelProgression(levelNumber);

        // === 7. Recovery point (hazard) ===
        CreateRecoveryPoint();

        // === 8. Exit door ===
        CreateExitDoor(levelNumber);

        // Guardar escena
        EditorSceneManager.SaveScene(scene, path);
        Debug.Log($"Escena creada: {path}");
    }

    static void CreateBasicGeometry(int levelNum)
    {
        GameObject root = new GameObject("Escenario");

        // Suelo
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Floor";
        floor.transform.SetParent(root.transform);
        floor.transform.localScale = new Vector3(3f, 1f, 3f); // 30x30 unidades
        floor.transform.position = Vector3.zero;

        // Material oscuro para el suelo
        var floorMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        floorMat.color = new Color(0.15f, 0.12f, 0.1f);
        floor.GetComponent<MeshRenderer>().material = floorMat;

        // Paredes perimetrales
        float wallHeight = 6f;
        float floorSize = 30f;
        float wallThickness = 0.5f;

        // Pared norte
        CreateWall(root.transform, new Vector3(0, wallHeight/2, floorSize/2), new Vector3(floorSize, wallHeight, wallThickness), "Wall_North");
        // Pared sur
        CreateWall(root.transform, new Vector3(0, wallHeight/2, -floorSize/2), new Vector3(floorSize, wallHeight, wallThickness), "Wall_South");
        // Pared este
        CreateWall(root.transform, new Vector3(floorSize/2, wallHeight/2, 0), new Vector3(wallThickness, wallHeight, floorSize), "Wall_East");
        // Pared oeste
        CreateWall(root.transform, new Vector3(-floorSize/2, wallHeight/2, 0), new Vector3(wallThickness, wallHeight, floorSize), "Wall_West");

        // Pit (hazard) en el centro - un agujero con lava visual
        GameObject pit = GameObject.CreatePrimitive(PrimitiveType.Plane);
        pit.name = "Hazard_Pit";
        pit.transform.SetParent(root.transform);
        pit.transform.position = new Vector3(0, -0.5f, 0);
        pit.transform.localScale = new Vector3(0.3f, 1f, 0.3f); // 3x3
        var pitMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        pitMat.color = new Color(0.8f, 0.2f, 0f, 1f);
        pitMat.SetFloat("_EmissionColor", 1.5f);
        pit.GetComponent<MeshRenderer>().material = pitMat;

        // Plataforma central sobre el pit
        GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        platform.name = "Central_Platform";
        platform.transform.SetParent(root.transform);
        platform.transform.position = new Vector3(0, 0.5f, 0);
        platform.transform.localScale = new Vector3(4f, 1f, 4f);
        var platMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        platMat.color = new Color(0.3f, 0.25f, 0.2f);
        platform.GetComponent<MeshRenderer>().material = platMat;
    }

    static void CreateWall(Transform parent, Vector3 pos, Vector3 scale, string name)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(parent);
        wall.transform.position = pos;
        wall.transform.localScale = scale;
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.2f, 0.18f, 0.15f);
        wall.GetComponent<MeshRenderer>().material = mat;
    }

    static void CreateLights()
    {
        GameObject lightsRoot = new GameObject("Lights");

        // Luz direccional principal
        GameObject dirLight = new GameObject("Directional_Light");
        dirLight.transform.SetParent(lightsRoot.transform);
        Light dl = dirLight.AddComponent<Light>();
        dl.type = LightType.Directional;
        dl.intensity = 0.8f;
        dl.color = new Color(0.9f, 0.85f, 0.7f);
        dl.shadows = LightShadows.Soft;
        dirLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // Luz puntual central (atmósfera)
        GameObject pointLight = new GameObject("Point_Light_Center");
        pointLight.transform.SetParent(lightsRoot.transform);
        pointLight.transform.position = new Vector3(0, 4f, 0);
        Light pl = pointLight.AddComponent<Light>();
        pl.type = LightType.Point;
        pl.intensity = 2f;
        pl.range = 15f;
        pl.color = new Color(1f, 0.7f, 0.3f);
        pl.shadows = LightShadows.Soft;
    }

    static void CreateCameraSystem(int levelNum)
    {
        // Camera Manager
        GameObject camMgrObj = new GameObject("Camera Manager");
        var camMgrType = FindType("CameraManager");
        if (camMgrType != null) camMgrObj.AddComponent(camMgrType);

        // Cámaras estáticas (2 por nivel: overview + puzzle)
        GameObject cam1 = new GameObject("Camera_Overview");
        cam1.tag = "MainCamera";
        Camera c1 = cam1.AddComponent<Camera>();
        c1.clearFlags = CameraClearFlags.SolidColor;
        c1.backgroundColor = Color.black;
        cam1.transform.position = new Vector3(0, 12f, -15f);
        cam1.transform.LookAt(new Vector3(0, 0, 0));

        // Audio Listener en la primera cámara
        cam1.AddComponent<AudioListener>();

        GameObject cam2 = new GameObject("Camera_Puzzle");
        Camera c2 = cam2.AddComponent<Camera>();
        c2.clearFlags = CameraClearFlags.SolidColor;
        c2.backgroundColor = Color.black;
        cam2.transform.position = new Vector3(10f, 8f, 10f);
        cam2.transform.LookAt(new Vector3(0, 0, 0));
        cam2.SetActive(false); // Inactiva hasta que el trigger la active

        // Camera Triggers
        GameObject triggersRoot = new GameObject("Camera Triggers");

        // Trigger 1: entrada (ya tiene la cam1 activa por defecto)
        GameObject trigger1 = new GameObject("Trigger_Overview");
        trigger1.transform.SetParent(triggersRoot.transform);
        trigger1.transform.position = new Vector3(0, 1f, -10f);
        var col1 = trigger1.AddComponent<BoxCollider>();
        col1.isTrigger = true;
        col1.size = new Vector3(8f, 4f, 2f);
        var ct1 = trigger1.AddComponent<CameraTrigger>();
        SetPrivateField(ct1, "targetCamera", c1);

        // Trigger 2: zona del puzzle
        GameObject trigger2 = new GameObject("Trigger_Puzzle");
        trigger2.transform.SetParent(triggersRoot.transform);
        trigger2.transform.position = new Vector3(5f, 1f, 5f);
        var col2 = trigger2.AddComponent<BoxCollider>();
        col2.isTrigger = true;
        col2.size = new Vector3(8f, 4f, 2f);
        var ct2 = trigger2.AddComponent<CameraTrigger>();
        SetPrivateField(ct2, "targetCamera", c2);
    }

    static void CreateBootstrap(GameManager.LevelId levelId, string objective)
    {
        GameObject bootstrap = new GameObject("LevelBootstrap");
        var bs = bootstrap.AddComponent<LevelBootstrap>();

        // Asignar prefab del Player
        var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Custom/Prefabs/Player.prefab");
        SetPrivateField(bs, "playerPrefab", playerPrefab);

        // Crear spawn point
        GameObject spawn = new GameObject("SpawnPoint");
        spawn.transform.position = new Vector3(0, 1f, -12f);
        SetPrivateField(bs, "spawnPoint", spawn.transform);

        // Asignar levelId y objective
        SetPrivateField(bs, "levelId", levelId);
        SetPrivateField(bs, "objectiveText", objective);
    }

    static void CreatePuzzle(int levelNum)
    {
        GameObject puzzleRoot = new GameObject("Puzzle");

        switch (levelNum)
        {
            case 2: // Box puzzle (2 targets)
                CreateBoxPuzzle(puzzleRoot, 2);
                break;
            case 3: // Red ball puzzle
                CreateRedBallPuzzle(puzzleRoot);
                break;
            case 4: // Crumbling platforms
                CreateCrumblingPlatformsPuzzle(puzzleRoot);
                break;
            case 5: // Temporal platforms
                CreateTemporalPlatformsPuzzle(puzzleRoot);
                break;
            case 6: // Final: combined
                CreateBoxPuzzle(puzzleRoot, 2);
                CreateRedBallPuzzle(puzzleRoot);
                break;
        }
    }

    static void CreateBoxPuzzle(GameObject parent, int targetCount)
    {
        // Crear PuzzleManager
        GameObject pmObj = new GameObject("BoxPuzzleManager");
        pmObj.transform.SetParent(parent.transform);
        var pm = pmObj.AddComponent<PuzzleManager>();

        // Crear cajas y targets
        for (int i = 0; i < targetCount; i++)
        {
            float offsetX = (i - (targetCount - 1) / 2f) * 4f;

            // Caja
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = $"Box_{i}";
            box.transform.SetParent(parent.transform);
            box.transform.position = new Vector3(offsetX, 0.5f, -5f);
            box.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            box.tag = "Box";
            var rb = box.AddComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            var bc = box.GetComponent<BoxCollider>();
            var boxCtrl = box.AddComponent<BoxController>();
            SetPrivateField(boxCtrl, "boxCollider", bc);
            SetPrivateField(boxCtrl, "rb", rb);

            var boxMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            boxMat.color = i == 0 ? Color.red : Color.blue;
            box.GetComponent<MeshRenderer>().material = boxMat;

            // Target
            GameObject target = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            target.name = $"Target_{i}";
            target.transform.SetParent(parent.transform);
            target.transform.position = new Vector3(offsetX, 0.05f, 5f);
            target.transform.localScale = new Vector3(1.5f, 0.1f, 1.5f);
            var tc = target.AddComponent<TargetController>();
            tc.correctBox = boxCtrl;

            var targetMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            targetMat.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            tc.inactiveMaterial = targetMat;
            var activeMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            activeMat.color = Color.green;
            tc.activeMaterial = activeMat;
            target.GetComponent<MeshRenderer>().material = targetMat;
        }
    }

    static void CreateRedBallPuzzle(GameObject parent)
    {
        GameObject rbpRoot = new GameObject("RedBallPuzzle");
        rbpRoot.transform.SetParent(parent.transform);

        // SpawnButton
        GameObject buttonObj = new GameObject("SpawnButton");
        buttonObj.transform.SetParent(rbpRoot.transform);
        buttonObj.transform.position = new Vector3(-8f, 0.5f, 0f);
        var btnCol = buttonObj.AddComponent<BoxCollider>();
        btnCol.isTrigger = true;
        btnCol.size = new Vector3(2f, 1f, 2f);
        var sb = buttonObj.AddComponent<SpawnButton>();

        // Goal
        GameObject goalObj = new GameObject("RedBallGoal");
        goalObj.transform.SetParent(rbpRoot.transform);
        goalObj.transform.position = new Vector3(8f, 0.5f, 0f);
        var goalCol = goalObj.AddComponent<BoxCollider>();
        goalCol.isTrigger = true;
        goalCol.size = new Vector3(2f, 2f, 2f);
        var rbg = goalObj.AddComponent<RedBallGoal>();
        SetPrivateField(rbg, "spawnButton", sb);

        // Visual del goal
        GameObject goalVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        goalVisual.name = "GoalVisual";
        goalVisual.transform.SetParent(goalObj.transform);
        goalVisual.transform.localPosition = Vector3.zero;
        goalVisual.transform.localScale = new Vector3(1.5f, 0.1f, 1.5f);
        var goalMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        goalMat.color = Color.yellow;
        goalVisual.GetComponent<MeshRenderer>().material = goalMat;
    }

    static void CreateCrumblingPlatformsPuzzle(GameObject parent)
    {
        GameObject cpRoot = new GameObject("CrumblingPlatforms");
        cpRoot.transform.SetParent(parent.transform);

        // Crear 4 plataformas que se caen
        for (int i = 0; i < 4; i++)
        {
            GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platform.name = $"CrumblingPlatform_{i}";
            platform.transform.SetParent(cpRoot.transform);
            platform.transform.position = new Vector3(-6f + i * 4f, 0f, 8f);
            platform.transform.localScale = new Vector3(3f, 0.5f, 3f);
            platform.AddComponent<BoxCollider>();
            var cp = platform.AddComponent<CrumblingPlatform>();

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.4f, 0.3f, 0.2f);
            platform.GetComponent<MeshRenderer>().material = mat;
        }
    }

    static void CreateTemporalPlatformsPuzzle(GameObject parent)
    {
        GameObject tpRoot = new GameObject("TemporalPlatforms");
        tpRoot.transform.SetParent(parent.transform);

        // Crear 4 plataformas temporales alternadas
        for (int i = 0; i < 4; i++)
        {
            GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platform.name = $"TemporalPlatform_{i}";
            platform.transform.SetParent(tpRoot.transform);
            platform.transform.position = new Vector3(-6f + i * 4f, 0f, 8f);
            platform.transform.localScale = new Vector3(3f, 0.5f, 3f);
            platform.AddComponent<BoxCollider>();
            var tp = platform.AddComponent<TemporalPlataform>();

            // Desfasar los tiempos para que aparezcan alternadas
            tp.tiempoActivo = 2f;
            tp.tiempoInactivo = 2f;
            tp.tiempoDesvanecimiento = 0.5f;
            tp.tiempoAparicion = 0.5f;

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.3f, 0.4f, 0.6f);
            platform.GetComponent<MeshRenderer>().material = mat;
        }
    }

    static void CreateLevelProgression(int levelNum)
    {
        GameObject lpObj = new GameObject("LevelProgression");
        var lp = lpObj.AddComponent<LevelProgression>();

        // Configurar puzzles según el nivel
        // (los puzzle IDs se configuran automáticamente via PuzzleManager.puzzleId)
    }

    static void CreateRecoveryPoint()
    {
        GameObject recoveryRoot = new GameObject("RecoveryPoints");

        GameObject recovery = new GameObject("RecoveryPoint");
        recovery.transform.SetParent(recoveryRoot.transform);
        recovery.transform.position = new Vector3(0, -1f, 0f);
        var col = recovery.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(4f, 1f, 4f);
        recovery.AddComponent<RecoverySystem>();

        // Recovery target (donde aparece el player al recuperar)
        GameObject recoveryTarget = new GameObject("RecoveryTarget");
        recoveryTarget.transform.SetParent(recoveryRoot.transform);
        recoveryTarget.transform.position = new Vector3(0, 1f, -12f);

        // Asignar referencias al RecoverySystem
        var rs = recovery.GetComponent<RecoverySystem>();
        // El player se asigna automáticamente cuando el LevelBootstrap lo crea
        // Por ahora dejamos que funcione con tag-based detection
    }

    static void CreateExitDoor(int levelNum)
    {
        GameObject exit = new GameObject("ExitDoor");
        exit.transform.position = new Vector3(0, 0f, 14f);

        // Visual de la puerta
        GameObject doorVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        doorVisual.name = "DoorVisual";
        doorVisual.transform.SetParent(exit.transform);
        doorVisual.transform.localPosition = Vector3.zero;
        doorVisual.transform.localScale = new Vector3(4f, 5f, 0.5f);
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.1f, 0.05f, 0.02f);
        doorVisual.GetComponent<MeshRenderer>().material = mat;

        // Trigger de salida (carga siguiente nivel)
        GameObject exitTrigger = new GameObject("ExitTrigger");
        exitTrigger.transform.SetParent(exit.transform);
        exitTrigger.transform.localPosition = Vector3.zero;
        var col = exitTrigger.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(4f, 5f, 1f);
        var sc = exitTrigger.AddComponent<SceneChanger>();

        // Asignar el índice del siguiente nivel
        // Level 2 -> load level 3, etc.
        // El SceneChanger.LoadLevel(int) usa GameManager si existe
    }

    static void UpdateBuildSettings()
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>
        {
            new EditorBuildSettingsScene("Assets/Custom/Scenes/0_MAINMENU.unity", true),
            new EditorBuildSettingsScene("Assets/Custom/Scenes/1_Level1.unity", true),
            new EditorBuildSettingsScene("Assets/Custom/Scenes/2_Level2.unity", true),
            new EditorBuildSettingsScene("Assets/Custom/Scenes/3_Level3.unity", true),
            new EditorBuildSettingsScene("Assets/Custom/Scenes/4_Level4.unity", true),
            new EditorBuildSettingsScene("Assets/Custom/Scenes/5_Level5.unity", true),
            new EditorBuildSettingsScene("Assets/Custom/Scenes/6_Final.unity", true)
        };

        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log("Build Settings actualizado con 7 escenas.");
    }

    // === Utilidades ===
    static System.Type FindType(string name)
    {
        return System.AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Array.Empty<System.Type>(); } })
            .FirstOrDefault(t => t.Name == name);
    }

    static void SetPrivateField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(obj, value);
        }
    }
}
#endif
