using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;


public class ToggleShop : MonoBehaviour
{
    public ShopManager sm;

    private PlayerInventory playerInvInRange;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
 
    }
    // Update is called once per frame

    private void OnTriggerEnter2D(Collider2D other)
    {
        var inv = other.GetComponent<PlayerInventory>();
        if (inv != null)
            playerInvInRange = inv;
    }

    // On détecte la sortie du joueur de la zone

    private void OnTriggerExit2D(Collider2D other)
    {
        var inv = other.GetComponent<PlayerInventory>();
        if (inv != null && inv == playerInvInRange)
            playerInvInRange = null;
    }
    void Update()
    {
        if (!playerInvInRange)
            return ;
        if (!Keyboard.current.eKey.wasPressedThisFrame)
            return;
        sm.ToggleShop();
    }
}
