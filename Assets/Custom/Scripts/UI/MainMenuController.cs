using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

/// <summary>
/// Controller del MainMenu usando UI Toolkit.
/// Gestiona tres paneles: menu principal, libro de capitulos (selector de niveles),
/// y opciones. Usa UIDocument + UXML + USS.
/// Incluye navegacion por teclado, manejo de foco y dialogo de confirmacion.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class MainMenuController : MonoBehaviour
{
    // === Datos de niveles ===
    [System.Serializable]
    public struct LevelInfo
    {
        public int buildIndex;
        public string romanNumeral;
        public string displayName;
        public string description;
    }

    [Header("Datos de niveles")]
    [SerializeField] private LevelInfo[] levels = new LevelInfo[]
    {
        new LevelInfo { buildIndex = 1, romanNumeral = "I", displayName = "La Mazmorra", description = "Despiertas en una celda oscura. Encuentra la forma de salir resolviendo el puzzle de cajas." },
        new LevelInfo { buildIndex = 2, romanNumeral = "II", displayName = "Las Cajas", description = "Empuja las cajas a los objetivos para abrir la puerta de salida." },
        new LevelInfo { buildIndex = 3, romanNumeral = "III", displayName = "La Bola Roja", description = "Pulsa el boton para spawnear la bola roja y guiala hasta la meta." },
        new LevelInfo { buildIndex = 4, romanNumeral = "IV", displayName = "El Puente", description = "Cruza el puente antes de que las plataformas colapsen bajo tus pies." },
        new LevelInfo { buildIndex = 5, romanNumeral = "V", displayName = "Tiempo Roto", description = "Usa el slow-mo para cruzar las plataformas temporales que aparecen y desaparecen." },
        new LevelInfo { buildIndex = 6, romanNumeral = "VI", displayName = "La Salida", description = "El ultimo desafio. Resuelve todos los puzzles para escapar de la mazmorra." },
    };

    // === Referencias UI ===
    private VisualElement _root;
    private VisualElement _layoutRoot;
    private VisualElement _mainMenuPanel;
    private VisualElement _bookPanel;
    private VisualElement _optionsPanel;
    private VisualElement _confirmDialog;

    // Panel principal
    private Button _btnPlay;
    private Button _btnOptions;
    private Button _btnQuit;

    // Libro
    private Label _levelNumber;
    private Label _levelName;
    private Label _levelDescription;
    private Label _levelStatusIcon;
    private Label _levelStatusText;
    private VisualElement _waxSeal;
    private Button _btnEnterLevel;
    private Button _btnPrevPage;
    private Button _btnNextPage;
    private Label _pageIndicator;
    private Button _btnBackFromBook;

    // Opciones
    private Slider _sliderMaster;
    private Slider _sliderMusic;
    private Slider _sliderSfx;
    private Button _btnResetProgress;
    private Button _btnBackFromOptions;

    // Dialogo de confirmacion
    private Button _btnConfirmYes;
    private Button _btnConfirmNo;

    // Estado del libro
    private int _currentPage = 0;

    // Panel activo actual (para navegacion por teclado)
    private enum ActivePanel { MainMenu, Book, Options }
    private ActivePanel _currentPanel = ActivePanel.MainMenu;

    void OnEnable()
    {
        // En UI Toolkit, el rootVisualElement puede no tener el arbol clonado
        // inmediatamente en OnEnable. Usamos schedule para esperar un frame.
        var doc = GetComponent<UIDocument>();
        if (doc == null)
        {
            Debug.LogError("[MainMenuController] No se encontro UIDocument.");
            return;
        }

        _root = doc.rootVisualElement;
        if (_root == null)
        {
            Debug.LogError("[MainMenuController] rootVisualElement es null.");
            return;
        }

        // Si el arbol aun no se ha clonado (0 hijos), esperar al proximo frame
        if (_root.childCount == 0)
        {
            _root.schedule.Execute(() =>
            {
                if (_root.childCount == 0)
                {
                    Debug.LogError("[MainMenuController] El UXML no se cargo. Verifica el sourceAsset del UIDocument.");
                    return;
                }
                InitUI();
            }).ExecuteLater(0);
        }
        else
        {
            InitUI();
        }
    }

    private void InitUI()
    {
        CacheElements();
        SubscribeCallbacks();
        ShowMainMenu();
        LoadVolumeSettings();
    }

    void OnDisable()
    {
        UnsubscribeCallbacks();
    }

    private void CacheElements()
    {
        _layoutRoot = _root.Q<VisualElement>("app-root");
        _mainMenuPanel = _root.Q<VisualElement>("main-menu-panel");
        _bookPanel = _root.Q<VisualElement>("book-panel");
        _optionsPanel = _root.Q<VisualElement>("options-panel");
        _confirmDialog = _root.Q<VisualElement>("confirm-dialog");

        // Panel principal
        _btnPlay = _root.Q<Button>("btn-play");
        _btnOptions = _root.Q<Button>("btn-options");
        _btnQuit = _root.Q<Button>("btn-quit");

        // Libro
        _levelNumber = _root.Q<Label>("level-number");
        _levelName = _root.Q<Label>("level-name");
        _levelDescription = _root.Q<Label>("level-description");
        _levelStatusIcon = _root.Q<Label>("level-status-icon");
        _levelStatusText = _root.Q<Label>("level-status-text");
        _waxSeal = _root.Q<VisualElement>("wax-seal");
        _btnEnterLevel = _root.Q<Button>("btn-enter-level");
        _btnPrevPage = _root.Q<Button>("btn-prev-page");
        _btnNextPage = _root.Q<Button>("btn-next-page");
        _pageIndicator = _root.Q<Label>("page-indicator");
        _btnBackFromBook = _root.Q<Button>("btn-back-from-book");

        // Opciones
        _sliderMaster = _root.Q<Slider>("slider-master");
        _sliderMusic = _root.Q<Slider>("slider-music");
        _sliderSfx = _root.Q<Slider>("slider-sfx");
        _btnResetProgress = _root.Q<Button>("btn-reset-progress");
        _btnBackFromOptions = _root.Q<Button>("btn-back-from-options");

        // Dialogo de confirmacion
        _btnConfirmYes = _root.Q<Button>("btn-confirm-yes");
        _btnConfirmNo = _root.Q<Button>("btn-confirm-no");
    }

    private void SubscribeCallbacks()
    {
        _btnPlay.clicked += ShowBook;
        _btnOptions.clicked += ShowOptions;
        _btnQuit.clicked += OnQuit;

        _btnEnterLevel.clicked += OnEnterLevel;
        _btnPrevPage.clicked += OnPrevPage;
        _btnNextPage.clicked += OnNextPage;
        _btnBackFromBook.clicked += ShowMainMenu;

        _btnBackFromOptions.clicked += ShowMainMenu;
        _btnResetProgress.clicked += OnResetProgressClicked;

        _btnConfirmYes.clicked += OnConfirmReset;
        _btnConfirmNo.clicked += OnCancelReset;

        _sliderMaster.RegisterValueChangedCallback(OnMasterVolumeChanged);
        _sliderMusic.RegisterValueChangedCallback(OnMusicVolumeChanged);
        _sliderSfx.RegisterValueChangedCallback(OnSfxVolumeChanged);

        // Navegacion por teclado
        _root.RegisterCallback<KeyDownEvent>(OnKeyDown);
        _layoutRoot.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        UpdateResponsiveClasses(_layoutRoot.resolvedStyle.width, _layoutRoot.resolvedStyle.height);
    }

    private void UnsubscribeCallbacks()
    {
        if (_btnPlay != null) _btnPlay.clicked -= ShowBook;
        if (_btnOptions != null) _btnOptions.clicked -= ShowOptions;
        if (_btnQuit != null) _btnQuit.clicked -= OnQuit;

        if (_btnEnterLevel != null) _btnEnterLevel.clicked -= OnEnterLevel;
        if (_btnPrevPage != null) _btnPrevPage.clicked -= OnPrevPage;
        if (_btnNextPage != null) _btnNextPage.clicked -= OnNextPage;
        if (_btnBackFromBook != null) _btnBackFromBook.clicked -= ShowMainMenu;

        if (_btnBackFromOptions != null) _btnBackFromOptions.clicked -= ShowMainMenu;
        if (_btnResetProgress != null) _btnResetProgress.clicked -= OnResetProgressClicked;

        if (_btnConfirmYes != null) _btnConfirmYes.clicked -= OnConfirmReset;
        if (_btnConfirmNo != null) _btnConfirmNo.clicked -= OnCancelReset;

        if (_sliderMaster != null) _sliderMaster.UnregisterValueChangedCallback(OnMasterVolumeChanged);
        if (_sliderMusic != null) _sliderMusic.UnregisterValueChangedCallback(OnMusicVolumeChanged);
        if (_sliderSfx != null) _sliderSfx.UnregisterValueChangedCallback(OnSfxVolumeChanged);

        if (_root != null) _root.UnregisterCallback<KeyDownEvent>(OnKeyDown);
        if (_layoutRoot != null) _layoutRoot.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
    }

    private void OnGeometryChanged(GeometryChangedEvent evt)
    {
        UpdateResponsiveClasses(evt.newRect.width, evt.newRect.height);
    }

    private void UpdateResponsiveClasses(float width, float height)
    {
        if (_layoutRoot == null || width <= 0f || height <= 0f) return;

        _layoutRoot.EnableInClassList("compact", width < 1280f || width / height < 1.45f);
        _layoutRoot.EnableInClassList("short", height < 760f);
    }

    // === Navegacion por teclado ===
    private void OnKeyDown(KeyDownEvent evt)
    {
        // Escape: volver atras o cerrar dialogo
        if (evt.keyCode == KeyCode.Escape)
        {
            if (_confirmDialog != null && !_confirmDialog.ClassListContains("hidden"))
            {
                OnCancelReset();
            }
            else
            {
                switch (_currentPanel)
                {
                    case ActivePanel.Book:
                    case ActivePanel.Options:
                        ShowMainMenu();
                        break;
                }
            }
            evt.StopPropagation();
            return;
        }

        // Flechas izquierda/derecha: navegar paginas del libro
        if (_currentPanel == ActivePanel.Book &&
            _confirmDialog != null && _confirmDialog.ClassListContains("hidden"))
        {
            if (evt.keyCode == KeyCode.LeftArrow)
            {
                OnPrevPage();
                evt.StopPropagation();
            }
            else if (evt.keyCode == KeyCode.RightArrow)
            {
                OnNextPage();
                evt.StopPropagation();
            }
        }
    }

    // === Mostrar/Ocultar paneles ===
    private void ShowMainMenu()
    {
        if (_mainMenuPanel == null || _bookPanel == null || _optionsPanel == null)
        {
            Debug.LogError("[MainMenuController] ShowMainMenu: panels are null. UI not initialized.");
            return;
        }
        _mainMenuPanel.RemoveFromClassList("hidden");
        _bookPanel.AddToClassList("hidden");
        _optionsPanel.AddToClassList("hidden");
        _currentPanel = ActivePanel.MainMenu;

        // Enfocar el primer boton para accesibilidad
        _btnPlay?.Focus();
    }

    private void ShowBook()
    {
        _mainMenuPanel.AddToClassList("hidden");
        _bookPanel.RemoveFromClassList("hidden");
        _optionsPanel.AddToClassList("hidden");
        _currentPanel = ActivePanel.Book;

        _currentPage = 0;
        UpdateBookPage();

        // Enfocar el boton de entrar
        _btnEnterLevel?.Focus();
    }

    private void ShowOptions()
    {
        _mainMenuPanel.AddToClassList("hidden");
        _bookPanel.AddToClassList("hidden");
        _optionsPanel.RemoveFromClassList("hidden");
        _currentPanel = ActivePanel.Options;

        // Enfocar el primer slider
        _sliderMaster?.Focus();
    }

    // === Logica del libro ===
    private void UpdateBookPage()
    {
        if (levels.Length == 0) return;

        LevelInfo info = levels[_currentPage];
        int buildIndex = info.buildIndex;

        // Texto basico
        _levelNumber.text = $"CAPITULO {info.romanNumeral}";
        _levelName.text = info.displayName;
        _levelDescription.text = info.description;

        // Indicador de pagina
        _pageIndicator.text = $"Capitulo {_currentPage + 1} / {levels.Length}";

        // Estado: bloqueado / desbloqueado / completado
        bool unlocked = SaveSystem.IsLevelUnlocked(buildIndex);
        bool completed = SaveSystem.IsLevelCompleted(buildIndex);

        // Limpiar clases de estado
        _levelName.RemoveFromClassList("locked");
        _levelName.RemoveFromClassList("completed");
        _levelDescription.RemoveFromClassList("locked");
        _levelStatusIcon.RemoveFromClassList("completed");

        if (!unlocked)
        {
            // Bloqueado
            _waxSeal.style.display = DisplayStyle.Flex;
            _btnEnterLevel.SetEnabled(false);
            _levelStatusIcon.text = "";
            _levelStatusText.text = "BLOQUEADO";
            _levelName.AddToClassList("locked");
            _levelDescription.AddToClassList("locked");
        }
        else
        {
            // Desbloqueado
            _waxSeal.style.display = DisplayStyle.None;
            _btnEnterLevel.SetEnabled(true);

            if (completed)
            {
                _levelStatusIcon.text = "\u2713"; // Check mark
                _levelStatusIcon.AddToClassList("completed");
                _levelName.AddToClassList("completed");
                _levelStatusText.text = "COMPLETADO";
            }
            else
            {
                _levelStatusIcon.text = "!";
                _levelStatusText.text = "PENDIENTE";
            }
        }

        // Navegacion
        _btnPrevPage.SetEnabled(_currentPage > 0);
        _btnNextPage.SetEnabled(_currentPage < levels.Length - 1);
    }

    private void OnPrevPage()
    {
        if (_currentPage > 0)
        {
            _currentPage--;
            UpdateBookPage();
        }
    }

    private void OnNextPage()
    {
        if (_currentPage < levels.Length - 1)
        {
            _currentPage++;
            UpdateBookPage();
        }
    }

    private void OnEnterLevel()
    {
        int buildIndex = levels[_currentPage].buildIndex;

        if (GameManager.Instance != null)
        {
            if (System.Enum.IsDefined(typeof(GameManager.LevelId), buildIndex))
            {
                GameManager.Instance.LoadLevel((GameManager.LevelId)buildIndex);
            }
        }
        else
        {
            SceneManager.LoadScene(buildIndex);
        }
    }

    // === Opciones ===
    private void LoadVolumeSettings()
    {
        if (AudioManager.Instance != null)
        {
            _sliderMaster.value = AudioManager.Instance.GetMasterVolume();
            _sliderMusic.value = AudioManager.Instance.GetMusicVolume();
            _sliderSfx.value = AudioManager.Instance.GetSFXVolume();
        }
    }

    private void OnMasterVolumeChanged(ChangeEvent<float> evt)
    {
        AudioManager.Instance?.SetMasterVolume(evt.newValue);
    }

    private void OnMusicVolumeChanged(ChangeEvent<float> evt)
    {
        AudioManager.Instance?.SetMusicVolume(evt.newValue);
    }

    private void OnSfxVolumeChanged(ChangeEvent<float> evt)
    {
        AudioManager.Instance?.SetSFXVolume(evt.newValue);
    }

    // === Dialogo de confirmacion para borrar progreso ===
    private void OnResetProgressClicked()
    {
        _confirmDialog?.RemoveFromClassList("hidden");
        _btnConfirmNo?.Focus();
    }

    private void OnConfirmReset()
    {
        SaveSystem.ResetProgress();
        Debug.Log("[MainMenu] Progreso borrado.");
        _confirmDialog?.AddToClassList("hidden");

        // Si estamos en el libro, actualizar la pagina
        if (_currentPanel == ActivePanel.Book)
        {
            UpdateBookPage();
        }
        _btnResetProgress?.Focus();
    }

    private void OnCancelReset()
    {
        _confirmDialog?.AddToClassList("hidden");
        _btnResetProgress?.Focus();
    }

    // === Salir ===
    private void OnQuit()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
        else
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
