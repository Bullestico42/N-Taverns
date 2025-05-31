using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class BeerSlot : MonoBehaviour
{
    public event Action OnBeerPlaced;

    [Header("Visuel de la bière")]
    public Sprite beerSprite;
    public float beerScale = 0.1f;

    private bool clientAssigned = false;
    private bool serveAllowed = false;
    private GameObject beerInstance;
    private PlayerInventory playerInvInRange;

    public bool IsOccupied => clientAssigned;
    public Vector3 SeatPosition => transform.position + Vector3.up * 0.6f;

    public void AssignClient()
    {
        if (clientAssigned)
            throw new InvalidOperationException("Slot déjà pris par un client");
        clientAssigned = true;
        serveAllowed = false;
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

        // 3) appui sur E → pose animée
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (playerInvInRange.RemoveBeer())
            {
                PlaceBeerWithSlide();
                Debug.Log("Bière servie au client !");
            }
            else
            {
                Debug.Log("Inventaire vide !");
            }
        }
    }

    private void PlaceBeerWithSlide()
    {
        // 1) Déterminer la position de départ (position du joueur)
        Vector3 startPos = playerInvInRange.transform.position;

        // 2) Créer l'objet bière à cette position
        beerInstance = new GameObject("Beer");
        beerInstance.transform.position = startPos;
        beerInstance.transform.localScale = Vector3.one * beerScale;

        // 3) Ajouter le SpriteRenderer
        var sr = beerInstance.AddComponent<SpriteRenderer>();
        sr.sprite = beerSprite;
        sr.sortingOrder = 0;

        // 4) Ajouter le CircleCollider2D EN TRIGGER pour ne pas pousser le joueur
        //var col = beerInstance.AddComponent<CircleCollider2D>();
        //col.isTrigger = true;       // <-- ici on met en trigger pendant la glissade

        // 5) Lancer la coroutine pour glisser jusqu'au slot
        StartCoroutine(SlideBeer(beerInstance, transform.position));

        // 6) Émettre l'événement de placement
        OnBeerPlaced?.Invoke();
    }

    private IEnumerator SlideBeer(GameObject beer, Vector3 targetPosition)
    {
        float duration = 0.2f; // Durée de l'animation (ajustable)
        Vector3 startPosition = beer.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            // interpolation linéaire
            beer.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // À la fin, on place exactement à la position du slot
        beer.transform.position = targetPosition;

        // 1) On remet le collider en non‐trigger si on veut qu'il devienne solide ensuite
        //col.isTrigger = false;

        // 2) On parent l'objet bière au slot et on ajuste la position locale
        beer.transform.SetParent(transform);
        beer.transform.localPosition = Vector3.zero;
    }

    public void ConsumeBeer()
    {
        if (beerInstance == null) return;
        Destroy(beerInstance);
        beerInstance = null;
    }
}
