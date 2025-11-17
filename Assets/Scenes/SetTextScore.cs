using UnityEngine;

public class SetTextScore : MonoBehaviour
{
    private TMPro.TMP_Text scoreText;


    private void Start()
    {
        if (scoreText == null)
        {
            scoreText = GetComponent<TMPro.TMP_Text>();
            SaveManager.PlayerData lastData = SaveManager.Load<SaveManager.PlayerData>("LastScore");
            SaveManager.PlayerData bestData = SaveManager.Load<SaveManager.PlayerData>("BestScore");
            int lastScore = lastData != null ? lastData.score : 0;
            int bestScore = bestData != null ? bestData.score : 0;

            if (lastScore > bestScore)
            {
                scoreText.text = "You did better this time with " + lastScore + " points. Try again.";
                SaveManager.Save(lastData, "BestScore");
            }
            else
            {
                scoreText.text = "You did " + lastScore + " points! Be better next time.";
            }
        }
    }
}
