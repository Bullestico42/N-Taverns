using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuSelection : MonoBehaviour
{
    public Button firstButton;

    void OnEnable()
    {
        // Mettre à jour le Selected GameObject pour le clavier/manette
        EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
    }
}
