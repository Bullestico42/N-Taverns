using UnityEngine;

public class ForceResolution : MonoBehaviour
{
    void Start()
    {
        // Force le jeu en 1280x720 en plein écran (format 16:9)
        Screen.SetResolution(1280, 720, true);
    }
}
