using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathMenu : MonoBehaviour
{

    public TMPro.TextMeshProUGUI[] textDialog;
    public float timeBetweenLetters = 0.05f;
    public float timeBetweenDialogs = 1f;
    private string currentDialog = "";
    public TMPro.TextMeshProUGUI mainText;



    void Start()
    {
        StartCoroutine(ShowDeathMenu());
    }

    private System.Collections.IEnumerator ShowDeathMenu()
    {
        yield return new WaitForSeconds(1f);

        foreach (var dialog in textDialog)
        {
            currentDialog = dialog.text;
            dialog.text = "";
            foreach (char letter in currentDialog.ToCharArray())
            {
                AudioManager.Instance.PlaySound("LetterType");
                dialog.text += letter;
                mainText.text = dialog.text;
                yield return new WaitForSeconds(timeBetweenLetters);
            }
            yield return new WaitForSeconds(timeBetweenDialogs);
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 2);
    }

}
