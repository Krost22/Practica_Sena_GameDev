using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Lector central de input basado en InputSystem_Actions.
/// Expone eventos C# para que los sistemas (PlayerController, BoxInteraction,
/// LocalTimeManager, etc.) consuman input sin acoplarse al Input System directo.
///
/// Uso: asignar este ScriptableObject en el campo InputReader de cada consumidor.
/// El PlayerInput o el propio InputReader habilita las acciones al habilitarse.
/// </summary>
[CreateAssetMenu(fileName = "InputReader", menuName = "Custom/InputReader", order = 0)]
public class InputReader : ScriptableObject
{
    // Acción de input asset de referencia (arrastrar InputSystem_Actions aquí)
    [SerializeField] private InputActionAsset actionAsset;

    // Mapas de acciones cacheados
    private InputActionMap _playerMap;
    private InputActionMap _uiMap;

    // Acciones individuales cacheadas
    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _jumpAction;
    private InputAction _interactAction;
    private InputAction _sprintAction;
    private InputAction _slowMoAction;
    private InputAction _pauseAction;

    // ---- Eventos públicos ----
    // Movimiento (Value): se dispara cada frame con el vector 2D
    public event Action<Vector2> Move;
    public event Action<Vector2> Look;
    // Botones: started = press, canceled = release
    public event Action JumpStarted;
    public event Action InteractStarted;
    public event Action InteractCanceled;
    public event Action SprintStarted;
    public event Action SprintCanceled;
    public event Action SlowMoStarted;
    public event Action PauseStarted;

    // ---- Valores actuales (para polling) ----
    public Vector2 MoveValue => _moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
    public bool SprintHeld => _sprintAction?.IsPressed() ?? false;

    void OnEnable()
    {
        if (actionAsset == null)
        {
            Debug.LogError("[InputReader] No hay InputActionAsset asignado.");
            return;
        }

        CacheActions();
        EnablePlayer();
    }

    void OnDisable()
    {
        // No deshabilitar los mapas aquí — los ScriptableObjects pueden
        // tener OnDisable llamado durante recompilaciones, y eso rompería
        // el input en todas las escenas. Solo desuscribir callbacks.
        UnsubscribeCallbacks();
    }

    /// <summary>
    /// Llamado por consumidores (PlayerController, etc.) al iniciar.
    /// Garantiza que el mapa Player esté habilitado y los callbacks suscritos.
    /// </summary>
    public void EnsureEnabled()
    {
        if (actionAsset == null) return;
        if (_playerMap == null) CacheActions();
        EnablePlayer();
    }

    private void CacheActions()
    {
        _playerMap = actionAsset.FindActionMap("Player");
        _uiMap = actionAsset.FindActionMap("UI");

        if (_playerMap == null)
        {
            Debug.LogError("[InputReader] No se encontró el mapa 'Player'.");
            return;
        }

        _moveAction = _playerMap.FindAction("Move");
        _lookAction = _playerMap.FindAction("Look");
        _jumpAction = _playerMap.FindAction("Jump");
        _interactAction = _playerMap.FindAction("Interact");
        _sprintAction = _playerMap.FindAction("Sprint");
        _slowMoAction = _playerMap.FindAction("SlowMo");
        _pauseAction = _playerMap.FindAction("Pause");

        SubscribeCallbacks();
    }

    private void SubscribeCallbacks()
    {
        if (_moveAction != null)
        {
            _moveAction.performed += OnMove;
            _moveAction.canceled += OnMove;
        }
        if (_lookAction != null)
        {
            _lookAction.performed += OnLook;
            _lookAction.canceled += OnLook;
        }
        if (_jumpAction != null) { _jumpAction.started += OnJump; }
        if (_interactAction != null)
        {
            _interactAction.started += OnInteractStarted;
            _interactAction.canceled += OnInteractCanceled;
        }
        if (_sprintAction != null)
        {
            _sprintAction.started += OnSprintStarted;
            _sprintAction.canceled += OnSprintCanceled;
        }
        if (_slowMoAction != null) _slowMoAction.started += OnSlowMoStarted;
        if (_pauseAction != null) _pauseAction.started += OnPauseStarted;
    }

    private void UnsubscribeCallbacks()
    {
        if (_moveAction != null)
        {
            _moveAction.performed -= OnMove;
            _moveAction.canceled -= OnMove;
        }
        if (_lookAction != null)
        {
            _lookAction.performed -= OnLook;
            _lookAction.canceled -= OnLook;
        }
        if (_jumpAction != null) _jumpAction.started -= OnJump;
        if (_interactAction != null)
        {
            _interactAction.started -= OnInteractStarted;
            _interactAction.canceled -= OnInteractCanceled;
        }
        if (_sprintAction != null)
        {
            _sprintAction.started -= OnSprintStarted;
            _sprintAction.canceled -= OnSprintCanceled;
        }
        if (_slowMoAction != null) _slowMoAction.started -= OnSlowMoStarted;
        if (_pauseAction != null) _pauseAction.started -= OnPauseStarted;
    }

    // ---- Habilitar / Deshabilitar mapas ----
    public void EnablePlayer()
    {
        _playerMap?.Enable();
    }

    public void DisablePlayer()
    {
        _playerMap?.Disable();
    }

    public void EnableUI()
    {
        _uiMap?.Enable();
    }

    public void DisableUI()
    {
        _uiMap?.Disable();
    }

    private void DisableAll()
    {
        _playerMap?.Disable();
        _uiMap?.Disable();
    }

    // ---- Callbacks ----
    private void OnMove(InputAction.CallbackContext ctx) => Move?.Invoke(ctx.ReadValue<Vector2>());
    private void OnLook(InputAction.CallbackContext ctx) => Look?.Invoke(ctx.ReadValue<Vector2>());
    private void OnJump(InputAction.CallbackContext ctx) => JumpStarted?.Invoke();
    private void OnInteractStarted(InputAction.CallbackContext ctx) => InteractStarted?.Invoke();
    private void OnInteractCanceled(InputAction.CallbackContext ctx) => InteractCanceled?.Invoke();
    private void OnSprintStarted(InputAction.CallbackContext ctx) => SprintStarted?.Invoke();
    private void OnSprintCanceled(InputAction.CallbackContext ctx) => SprintCanceled?.Invoke();
    private void OnSlowMoStarted(InputAction.CallbackContext ctx) => SlowMoStarted?.Invoke();
    private void OnPauseStarted(InputAction.CallbackContext ctx) => PauseStarted?.Invoke();
}
