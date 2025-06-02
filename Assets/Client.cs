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

    [Header("Impatience")]
    public float maxWaitTime = 15f;
    public GameObject impatienceIndicator;

    private enum State { Walking, WaitingBeer, Drinking, Leaving }
    private State state;
    private SpriteRenderer sr;
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
            impatienceRoutine = StartCoroutine(ImpatienceTimer());
        }
    }

    private IEnumerator ImpatienceTimer()
    {
        float threshold = maxWaitTime * 0.7f;
        yield return new WaitForSeconds(threshold);

        if (impatienceIndicator != null)
            impatienceIndicator.SetActive(true);

        yield return new WaitForSeconds(maxWaitTime - threshold);

        if (state == State.WaitingBeer)
        {
            targetSlot.FreeSlot();
            GameManager.Instance.AddGoldToPlayer(-8);
            state = State.Leaving;
        }
    }

    private void WalkToExit()
    {
        if (impatienceIndicator != null)
            impatienceIndicator.SetActive(false);

        sr = GetComponent<SpriteRenderer>();
        sr.flipX = false;
        transform.position = Vector3.MoveTowards(transform.position, exitPoint.position, walkSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, exitPoint.position) < 0.05f)
        {
            Destroy(gameObject);
        }
    }

    private void OnBeerArrived()
    {
        if (state != State.WaitingBeer) return;

        if (impatienceRoutine != null)
            StopCoroutine(impatienceRoutine);

        if (impatienceIndicator != null)
            impatienceIndicator.SetActive(false);
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

        state = State.Leaving;
    }

    void OnDestroy()
    {
        if (targetSlot != null)
            targetSlot.OnBeerPlaced -= OnBeerArrived;
    }
}
