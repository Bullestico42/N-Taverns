using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class BeerSlot : MonoBehaviour
{
    public event Action OnBeerPlaced;

    [Header("Visuel de la bière")]
    public Sprite beerSprite;
    public float beerScale = 1f;

    private bool clientAssigned = false;
    private bool serveAllowed = false;           // ← nouveau
    private GameObject beerInstance;
    private PlayerInventory playerInvInRange;

    public bool IsOccupied => clientAssigned;
    public Vector3 SeatPosition => transform.position + Vector3.up * 1f;

    public void AssignClient()
    {
        if (clientAssigned)
            throw new InvalidOperationException("Slot déjà pris par un client");
        clientAssigned = true;
        serveAllowed = false;                     // on interdit jusqu’à l’arrivée
    }

    public void FreeSlot()
    {
        clientAssigned = false;
        serveAllowed = false;
        if (beerInstance != null)
        {
            Destroy(beerInstance);
            beerInstance = null;
        }
    }

    /// <summary>
    /// Appelée par le Client quand il atteint le SeatPosition
    /// </summary>
    public void EnableServe()
    {
        serveAllowed = true;
    }

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

    private void Update()
    {
        // 1) le client doit être assigné, arrivé, et pas déjà servi
        if (!clientAssigned || !serveAllowed || beerInstance != null)
            return;

        // 2) joueur doit être dans la zone
        if (playerInvInRange == null)
            return;

        // 3) appui sur E → pose
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (playerInvInRange.RemoveBeer())
            {
                PlaceBeer();
                Debug.Log("Bière servie au client !");
            }
            else
            {
                Debug.Log("Inventaire vide !");
            }
        }
    }

    private void PlaceBeer()
    {
        beerInstance = new GameObject("Beer");
        beerInstance.transform.SetParent(transform);
        beerInstance.transform.localPosition = Vector3.zero;
        beerInstance.transform.localScale = Vector3.one * beerScale;

        var sr = beerInstance.AddComponent<SpriteRenderer>();
        sr.sprite = beerSprite;
        sr.sortingOrder = 1;

        var col = beerInstance.AddComponent<CircleCollider2D>();
        col.isTrigger = false;

        OnBeerPlaced?.Invoke();
    }

    public void ConsumeBeer()
    {
        if (beerInstance == null) return;
        Destroy(beerInstance);
        beerInstance = null;
    }
}
