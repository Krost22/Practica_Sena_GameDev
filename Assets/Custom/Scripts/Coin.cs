using UnityEngine;
using TMPro;

public class Coin : MonoBehaviour
{
    public int score;
    public bool isOriginalCoin = true; // Flag to check if the coin is original

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (isOriginalCoin)
            {
                score += 1; // Increase score only if the coin is original
            }
            Destroy(gameObject); // Destroy the coin
        }
    }
}