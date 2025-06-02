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

    private bool isPaused = false;

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

    private void TrySpawnClient()
    {
        var freeSlots = beerSlots.Where(s => !s.IsOccupied).ToArray();
        if (freeSlots.Length == 0) return;

        var slot = freeSlots[Random.Range(0, freeSlots.Length)];
        var spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];

        var go = Instantiate(clientPrefab, spawn.position, Quaternion.identity);
        var client = go.GetComponent<Client>();
        client.targetSlot = slot;
        client.exitPoint = exitPoint;
    }
}
