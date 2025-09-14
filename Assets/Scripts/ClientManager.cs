using System.Collections;
using System.Linq;
using UnityEngine;

public class ClientManager : MonoBehaviour
{
    [Header("Réglages des clients")]
    public Client clientPrefabSettings;

    public GameObject clientPrefab;
    public Transform[] spawnPoints;
    public BeerSlot[] beerSlots;
    public Transform exitPoint;
    public float spawnInterval = 5f;
    public BeerDispenser beerDispenser;


    private bool isPaused = false;
    private float walkSpeed = 5f;
    private float waitTime = 15f;

    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (!isPaused)
                TrySpawnClient();
        }
    }

    public void SetPaused(bool value)
    {
        isPaused = value;
    }

    public void SetDifficulty(float newWaitTime)
    {
        waitTime = newWaitTime;
    }

    private void TrySpawnClient()
    {
        var freeSlots = beerSlots.Where(s => !s.IsOccupied).ToArray();
        if (freeSlots.Length == 0) return;

        var slot = freeSlots[Random.Range(0, freeSlots.Length)];
        var spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];

        var go = Instantiate(clientPrefab, spawn.position, Quaternion.identity);
        var client = go.GetComponent<Client>();

        client.walkSpeed = walkSpeed;
        client.maxWaitTime = waitTime;
        client.targetSlot = slot;
        client.exitPoint = exitPoint;
    }
}
