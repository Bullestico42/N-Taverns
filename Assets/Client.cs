using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Client : MonoBehaviour
{
    public float walkSpeed = 5f;
    public BeerSlot targetSlot;
    public Transform exitPoint;
    public float drinkDuration = 3f;
    public int price = 5;

    [Header("Sprites du client")]
    public Sprite walkingSprite;
    public Sprite sittingSprite;

    [Header("Bulle d'impatience (sprite seul)")]
    public Sprite angerBubbleSprite;       // ← l’image de bulle
    public Vector2 angerBubbleOffset = new Vector2(0f, 1.2f); // position au-dessus de la tête
    public float angerBubbleScale = 1f;

    [Header("Impatience")]
    public float maxWaitTime = 15f;

    private enum State { Walking, WaitingBeer, Drinking, Leaving }
    private State state;

    private SpriteRenderer sr;          // sprite du client
    private Animator anim;
    private SpriteRenderer angerBubble; // sprite de la bulle (créé au Start)
    private Coroutine impatienceRoutine;

    void Start()
    {
        state = State.Walking;

        // Récupère les composants du client (même si sur un enfant)
        sr = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponentInChildren<Animator>();

        if (walkingSprite != null && sr != null) sr.sprite = walkingSprite;

        // --- Crée la bulle si un sprite est fourni ---
        if (angerBubbleSprite != null)
        {
            var go = new GameObject("AngerBubble");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = angerBubbleOffset;
            go.transform.localScale = Vector3.one * Mathf.Max(0.01f, angerBubbleScale);

            angerBubble = go.AddComponent<SpriteRenderer>();
            angerBubble.sprite = angerBubbleSprite;
            angerBubble.sortingLayerID = sr != null ? sr.sortingLayerID : angerBubble.sortingLayerID;
            angerBubble.sortingOrder = (sr != null ? sr.sortingOrder : 0) + 1; // au-dessus du perso
            angerBubble.enabled = false; // caché par défaut
        }

        // Liaison slot
        targetSlot.AssignClient();
        targetSlot.OnBeerPlaced += OnBeerArrived;
    }

    void Update()
    {
        switch (state)
        {
            case State.Walking: WalkToSeat(); break;
            case State.Leaving: WalkToExit(); break;
        }
    }

    private void WalkToSeat()
    {
        Vector3 goal = targetSlot.SeatPosition;
        transform.position = Vector3.MoveTowards(transform.position, goal, walkSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, goal) < 0.05f)
        {
            targetSlot.EnableServe();
            state = State.WaitingBeer;

            // Assis/immobile
            SwitchToSitting();

            // Timer d'impatience
            if (impatienceRoutine != null) StopCoroutine(impatienceRoutine);
            impatienceRoutine = StartCoroutine(ImpatienceTimer());
        }
    }

    private IEnumerator ImpatienceTimer()
    {
        float threshold = maxWaitTime * 0.7f;
        yield return new WaitForSeconds(threshold);

        // Affiche la bulle
        ShowAngerBubble(true);

        yield return new WaitForSeconds(Mathf.Max(0f, maxWaitTime - threshold));

        if (state == State.WaitingBeer)
        {
            targetSlot.FreeSlot();
            GameManager.Instance.AddGoldToPlayer(-8);

            ShowAngerBubble(false);
            SwitchToWalking();
            state = State.Leaving;
        }
    }

    private void WalkToExit()
    {
        ShowAngerBubble(false);

        if (sr != null) sr.flipX = true;
        transform.position = Vector3.MoveTowards(transform.position, exitPoint.position, walkSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, exitPoint.position) < 0.05f)
            Destroy(gameObject);
    }

    private void OnBeerArrived()
    {
        if (state != State.WaitingBeer) return;

        if (impatienceRoutine != null)
        {
            StopCoroutine(impatienceRoutine);
            impatienceRoutine = null;
        }

        ShowAngerBubble(false);

        state = State.Drinking;
        StartCoroutine(DrinkAndPay());
    }

    private IEnumerator DrinkAndPay()
    {
        yield return new WaitForSeconds(drinkDuration);

        targetSlot.ConsumeBeer();
        targetSlot.FreeSlot();

        if (GameManager.Instance.AddGoldToPlayer(price))
        {
            GameManager.Instance.GainExp(5);
            Debug.Log($"Client a payé {price} or.");
        }
        else
        {
            Debug.Log("Joueur ne peut pas encaisser plus d’or.");
        }

        SwitchToWalking();
        state = State.Leaving;
    }

    void OnDestroy()
    {
        if (targetSlot != null)
            targetSlot.OnBeerPlaced -= OnBeerArrived;
    }

    // ---- Helpers ----
    private void SwitchToSitting()
    {
        if (anim != null) anim.enabled = false; // évite qu'il écrase le sprite
        if (sr != null && sittingSprite != null) sr.sprite = sittingSprite;
    }

    private void SwitchToWalking()
    {
        if (anim != null) anim.enabled = true; // mets true si tu veux rejouer une anim de marche
        if (sr != null && walkingSprite != null) sr.sprite = walkingSprite;
    }

    private void ShowAngerBubble(bool show)
    {
        if (angerBubble != null)
            angerBubble.enabled = show;
    }
}
