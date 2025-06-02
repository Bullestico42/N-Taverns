using UnityEngine;
using TMPro;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerInventory : MonoBehaviour
{
    [Header("Inventaire")]
    public int maxBeers = 5;
    public int currentBeers = 0;

    [Header("UI")]
    public TextMeshProUGUI beerCountText;

    void Start()
    {
        UpdateBeerUI();
    }

    public bool AddBeer()
    {
        if (currentBeers >= maxBeers) return false;
        currentBeers++;
        UpdateBeerUI();
        return true;
    }

    public bool RemoveBeer()
    {
        if (currentBeers <= 0) return false;
        currentBeers--;
        UpdateBeerUI();
        return true;
    }

    public void UpdateBeerUI()
    {
        if (beerCountText != null)
            beerCountText.text = $"Beer : {currentBeers}/{maxBeers}";
    }
}
