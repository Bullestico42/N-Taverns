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
        UpdateUI();
    }

    public bool AddBeer()
    {
        if (currentBeers >= maxBeers) return false;
        currentBeers++;
        UpdateUI();
        return true;
    }

    public bool RemoveBeer()
    {
        if (currentBeers <= 0) return false;
        currentBeers--;
        UpdateUI();
        return true;
    }

    void UpdateUI()
    {
        if (beerCountText != null)
            beerCountText.text = $"Beer : {currentBeers}/{maxBeers}";
    }
}
