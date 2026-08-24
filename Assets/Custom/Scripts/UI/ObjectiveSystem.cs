using System;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

/// <summary>
/// Sistema de objetivos/pistas contextuales.
/// Muestra texto en pantalla indicando qué debe hacer el jugador.
/// Se integra con PuzzleManager y GameManager para actualizar objetivos automáticamente.
/// </summary>
public class ObjectiveSystem : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI objectiveText;
    [SerializeField] private GameObject objectivePanel;
    [SerializeField] private float displayDuration = 5f;
    [SerializeField] private float fadeDuration = 1f;

    [Header("Objetivos por defecto")]
    [SerializeField] private string defaultObjective = "Explora la mazmorra";

    private string currentObjective;
    private float displayTimer = 0f;
    private bool isFading = false;

    public static ObjectiveSystem Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        SetObjective(defaultObjective);
    }

    void Update()
    {
        if (objectivePanel == null) return;

        if (displayTimer > 0f)
        {
            displayTimer -= Time.deltaTime;
            if (displayTimer <= 0f && !isFading)
            {
                StartCoroutine(FadeOut());
            }
        }
    }

    public void SetObjective(string text)
    {
        currentObjective = text;
        if (objectiveText != null)
        {
            objectiveText.text = text;
        }
        if (objectivePanel != null)
        {
            objectivePanel.SetActive(true);
        }
        displayTimer = displayDuration;
        isFading = false;

        // Reset alpha
        if (objectiveText != null)
        {
            var color = objectiveText.color;
            color.a = 1f;
            objectiveText.color = color;
        }
    }

    public void ShowTemporaryHint(string hint, float duration = 3f)
    {
        SetObjective(hint);
        displayTimer = duration;
    }

    public void ClearObjective()
    {
        currentObjective = "";
        if (objectiveText != null)
        {
            objectiveText.text = "";
        }
        if (objectivePanel != null)
        {
            objectivePanel.SetActive(false);
        }
    }

    private System.Collections.IEnumerator FadeOut()
    {
        isFading = true;
        if (objectiveText == null) yield break;

        float t = 0f;
        Color startColor = objectiveText.color;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            objectiveText.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        if (objectivePanel != null)
        {
            objectivePanel.SetActive(false);
        }
        isFading = false;
    }
}
