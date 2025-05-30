using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class BeerDispenser : MonoBehaviour
{
    [Header("Paramètres du distributeur")]
    [Tooltip("Nombre max de bières que le distributeur peut contenir")]
    public int maxBeers = 5;
    [Tooltip("Intervalle (en sec) entre chaque recharge d'une bière")]
    public float refillInterval = 10f;

    [Header("Visuel")]
    [Tooltip("Prefab de la bière à instancier pour l'affichage")]
    public GameObject beerPrefab;
    [Tooltip("Transforms des emplacements (slots) où les bières apparaissent")]
    public Transform[] slotTransforms;

    // État interne
    private int currentBeers = 0;
    private List<GameObject> beerInstances = new List<GameObject>();

    // Inventaire du joueur actuellement dans la zone
    private PlayerInventory playerInvInRange;

    void Start()
    {
        // Démarre la coroutine de recharge
        StartCoroutine(RefillRoutine());
    }

    // Recharge une bière tant qu'on n'est pas à la capacité max
    private IEnumerator RefillRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(refillInterval);
            if (currentBeers < maxBeers)
                AddDispenserBeer();
        }
    }

    // Instancie une bière à l'emplacement libre suivant
    private void AddDispenserBeer()
    {
        if (currentBeers >= slotTransforms.Length) return;

        Transform slot = slotTransforms[currentBeers];
        GameObject inst = Instantiate(beerPrefab, slot.position, Quaternion.identity, transform);
        beerInstances.Add(inst);
        currentBeers++;
    }

    // On détecte l'entrée du joueur dans la zone
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

    // Gère l'appui sur E une seule fois par frame, si le joueur est dans la zone
    private void Update()
    {
        if (playerInvInRange == null)
            return;

        if (!Keyboard.current.eKey.wasPressedThisFrame)
            return;

        if (currentBeers > 0)
        {
            if (playerInvInRange.AddBeer())
            {
                TakeBeerFromDispenser();
                Debug.Log("Bière récoltée !");
            }
            else
            {
                Debug.Log("Inventaire plein.");
            }
        }
        else
        {
            Debug.Log("Distributeur vide.");
        }
    }

    // Retire une bière du visuel et décrémente le stock
    private void TakeBeerFromDispenser()
    {
        int last = beerInstances.Count - 1;
        Destroy(beerInstances[last]);
        beerInstances.RemoveAt(last);
        currentBeers--;
    }
}
