using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Client : MonoBehaviour
{
    public float walkSpeed = 2f;
    public BeerSlot targetSlot;
    public Transform exitPoint;
    public float drinkDuration = 3f;
    public int price = 5;

    [Header("Impatience")]
    public float maxWaitTime = 10f;
    public GameObject impatienceIndicator;

    private enum State { Walking, WaitingBeer, Drinking, Leaving }
    private State state;

    private Coroutine impatienceRoutine;

    void Start()
    {
        state = State.Walking;
        if (impatienceIndicator != null)
            impatienceIndicator.SetActive(false);

        targetSlot.AssignClient();
        targetSlot.OnBeerPlaced += OnBeerArrived;
    }

    void Update()
    {
        switch (state)
        {
            case State.Walking:
                WalkToSeat();
                break;
            case State.Leaving:
                WalkToExit();
                break;
        }
    }

    private void WalkToSeat()
    {
        Vector3 goal = targetSlot.SeatPosition;
        transform.position = Vector3.MoveTowards(transform.position, goal, walkSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, goal) < 0.05f)
        {
            // Arrivé, on attend la bière
            targetSlot.EnableServe();
            state = State.WaitingBeer;
            Debug.Log("Client arrivé, attend la bière");

            // Lance timer d’impatience
            impatienceRoutine = StartCoroutine(ImpatienceTimer());
        }
    }

    private IEnumerator ImpatienceTimer()
    {
        if (impatienceIndicator != null)
            impatienceIndicator.SetActive(true);

        yield return new WaitForSeconds(maxWaitTime);

        if (state == State.WaitingBeer)
        {
            Debug.Log("Client s'impatiente et part !");
            // libère la place
            targetSlot.FreeSlot();
            // passe en Leaving
            state = State.Leaving;
        }
    }

    private void WalkToExit()
    {
        if (impatienceIndicator != null)
            impatienceIndicator.SetActive(false);

        Vector3 goal = exitPoint.position;
        transform.position = Vector3.MoveTowards(transform.position, goal, walkSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, goal) < 0.05f)
        {
            Destroy(gameObject);
        }
    }

    private void OnBeerArrived()
    {
        if (state != State.WaitingBeer) return;

        // Stoppe l’impatience
        if (impatienceRoutine != null)
            StopCoroutine(impatienceRoutine);
        if (impatienceIndicator != null)
            impatienceIndicator.SetActive(false);

        state = State.Drinking;
        Debug.Log("Client commence à boire");
        StartCoroutine(DrinkAndPay());
    }

    private IEnumerator DrinkAndPay()
    {
        yield return new WaitForSeconds(drinkDuration);

        GameManager.Instance?.AddGold(price);
        targetSlot.ConsumeBeer();
        targetSlot.FreeSlot();

        state = State.Leaving;
    }

    void OnDestroy()
    {
        if (targetSlot != null)
            targetSlot.OnBeerPlaced -= OnBeerArrived;
    }
}
