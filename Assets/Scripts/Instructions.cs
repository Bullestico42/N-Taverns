using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Instructions : MonoBehaviour
{
    private void Update()
    {
        bool keyboardPress = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;

        bool gamepadPress = Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame;

        if (keyboardPress || gamepadPress)
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}
