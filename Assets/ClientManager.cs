using System.Collections;
using System.Linq;
using UnityEngine;

public class ClientManager : MonoBehaviour
{
    public GameObject clientPrefab;
    public Transform[] spawnPoints;
    public BeerSlot[] beerSlots;
    public Transform exitPoint;
    public float spawnInterval = 5f;

    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while(true)
        {
            yield return new WaitForSeconds(spawnInterval);
            TrySpawnClient();
        }
    }

    private void TrySpawnClient()
    {
        // récupère les slots libres
        var freeSlots = beerSlots.Where(s => !s.IsOccupied).ToArray();
        if (freeSlots.Length == 0) return;

        // choix aléatoire
        var slot = freeSlots[Random.Range(0, freeSlots.Length)];
        // choix d’un point de spawn au hasard
        var spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // instanciation
        var go = Instantiate(clientPrefab, spawn.position, Quaternion.identity);
        var client = go.GetComponent<Client>();
        client.targetSlot = slot;
        client.exitPoint = exitPoint;
    }
}