using UnityEngine;

public class RedBallGoal : MonoBehaviour
{
    [SerializeField]
    private SpawnButton spawnButton;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("RedBall"))
        {
            spawnButton.OnPuzzleCompleted();
        }
    }
}
