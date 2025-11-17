using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public TMPro.TMP_Text bestScoreText;

    private void Start()
    {
        SaveManager.PlayerData bestData = SaveManager.Load<SaveManager.PlayerData>("BestScore");

        if(bestData == null)
        {
            bestScoreText.text = "Best Score: 0";
            return;
        }
        bestScoreText.text = "Best Score: " + bestData.score;
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {
        Debug.Log("Quit!");
        Application.Quit();
    }
}
