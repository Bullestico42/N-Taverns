using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverController : MonoBehaviour
{
    public void OnRetryButton()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void OnMenuButton()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
