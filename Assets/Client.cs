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

    private enum State { Walking, WaitingBeer, Drinking, Leaving }
    private State state;

    void Start()
    {
        state = State.Walking;
        targetSlot.AssignClient();
        targetSlot.OnBeerPlaced += OnBeerArrived;
    }

    void Update()
    {
        switch (state)
        {
            case State.Walking:
                // marche vers le siège
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    targetSlot.SeatPosition,
                    walkSpeed * Time.deltaTime
                );
                if (Vector3.Distance(transform.position, targetSlot.SeatPosition) < 0.05f)
                {
                    // le client est arrivé : autorise la pose de bière
                    targetSlot.EnableServe();
                    state = State.WaitingBeer;
                    Debug.Log("Client arrivé, attend la bière");
                }
                break;

            case State.Leaving:
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    exitPoint.position,
                    walkSpeed * Time.deltaTime
                );
                if (Vector3.Distance(transform.position, exitPoint.position) < 0.05f)
                {
                    targetSlot.FreeSlot();
                    Destroy(gameObject);
                }
                break;
        }
    }

    private void OnBeerArrived()
    {
        if (state != State.WaitingBeer) return;
        state = State.Drinking;
        Debug.Log("Client commence à boire");
        StartCoroutine(DrinkAndPay());
    }

    private IEnumerator DrinkAndPay()
    {
        yield return new WaitForSeconds(drinkDuration);

        if (GameManager.Instance != null)
            GameManager.Instance.AddGold(price);

        targetSlot.ConsumeBeer();

        state = State.Leaving;
    }

    void OnDestroy()
    {
        if (targetSlot != null)
            targetSlot.OnBeerPlaced -= OnBeerArrived;
    }
}