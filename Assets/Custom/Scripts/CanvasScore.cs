using UnityEngine; 
using TMPro;

public class CanvasScore : MonoBehaviour
{
    public Coin coin;
    public TMP_Text textScore;

    void Update()
    {
        textScore.text = "Score: " + coin.score;
    }
}