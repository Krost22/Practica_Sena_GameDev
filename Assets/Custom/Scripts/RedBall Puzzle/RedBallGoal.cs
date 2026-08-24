using UnityEngine;

public class RedBallGoal : MonoBehaviour
{
    [SerializeField]
    private SpawnButton spawnButton;

    [Header("Progresión")]
    [Tooltip("ID del puzzle para LevelProgression. Dejar vacío si no se usa LevelProgression.")]
    [SerializeField]
    private string puzzleId = "RedBallPuzzle";

    [SerializeField]
    private LevelProgression levelProgression;

    void Start()
    {
        if (levelProgression == null)
        {
            levelProgression = FindAnyObjectByType<LevelProgression>();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("RedBall"))
        {
            spawnButton.OnPuzzleCompleted();

            // Notificar al LevelProgression
            if (levelProgression != null && !string.IsNullOrEmpty(puzzleId))
            {
                levelProgression.RegisterPuzzleComplete(puzzleId);
            }
        }
    }
}
