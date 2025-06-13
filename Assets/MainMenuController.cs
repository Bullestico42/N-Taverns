using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    // Appel� par le bouton Jouer
    public void PlayGame()
    {
        // Charge la sc�ne d�index 1 dans Build Settings (votre jeu)
        SceneManager.LoadScene("SampleScene");
    }

    // Appel� par le bouton Quitter
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
