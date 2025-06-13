using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ToggleShop : MonoBehaviour
{
    public ShopManager sm;
    private PlayerInventory playerInvInRange;

    private void OnTriggerEnter2D(Collider2D other)
    {
        var inv = other.GetComponent<PlayerInventory>();
        if (inv != null)
            playerInvInRange = inv;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var inv = other.GetComponent<PlayerInventory>();
        if (inv != null && inv == playerInvInRange)
            playerInvInRange = null;
    }

    void Update()
    {
        if (!playerInvInRange)
            return;

        // ✅ Clavier : touche E
        bool keyboardPress = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;

        // ✅ Manette (arcade / XInput) : bouton A (buttonSouth)
        bool gamepadPress = Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame;

        if (keyboardPress || gamepadPress)
        {
            sm.ToggleShop();
        }
    }
}
