using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject defaultSelectedButton; // bouton par défaut (Jouer)

    private void OnEnable()
    {
        if (defaultSelectedButton != null)
            EventSystem.current.SetSelectedGameObject(defaultSelectedButton);
    }

    // Appelé par bouton Jouer (UI ou manette bouton A)
    public void PlayGame()
    {
        SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
    }

    public void OpenInstructions()
    {
        SceneManager.LoadScene("Instructions", LoadSceneMode.Single);
    }

    // Appelé par bouton Quitter
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void Update()
    {
        if (Gamepad.current == null) return;

        // Bouton A = jouer
        if (Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            PlayGame();
        }

        // Bouton B = quitter
        if (Gamepad.current.buttonEast.wasPressedThisFrame)
        {
            QuitGame();
        }
    }
}
