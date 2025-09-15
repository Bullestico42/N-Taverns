using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem; // Nouveau Input System
using TMPro;

public class GameOverController : MonoBehaviour
{
    [SerializeField] private GameObject defaultSelectedButton; // bouton par défaut (Retry)
    public TextMeshProUGUI scoreText;

    void Start()
    {
        if (GameManager.Instance != null)
        {
            int score = GameManager.Instance.score;
            scoreText.text = $"Score : {score}";
        }
        else
        {
            scoreText.text = "Full on kaks";
        }
    }

    private void OnEnable()
    {
        // Assure qu’un bouton est sélectionné quand on arrive sur l’écran
        if (defaultSelectedButton != null)
            EventSystem.current.SetSelectedGameObject(defaultSelectedButton);
    }

    // Bouton Retry (appelé par UI ou manette bouton A)
    public void OnRetryButton()
    {
        SceneManager.LoadScene("SampleScene");
    }

    // Bouton Menu
    public void OnMenuButton()
    {
        SceneManager.LoadScene("MainMenu");
    }

    // Exemple : bouton B pour retour menu
    private void Update()
    {
        if (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame)
        {
            OnMenuButton();
        }
    }
}
