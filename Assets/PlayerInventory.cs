using UnityEngine;
using TMPro;             // 1) On importe TMPro

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerInventory : MonoBehaviour
{
    [Header("Inventaire")]
    public int maxBeers = 5;
    public int currentBeers = 0;

    [Header("UI")]
    public TextMeshProUGUI beerCountText;  // 2) On utilise TMP ici

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
            beerCountText.text = $"Bière : {currentBeers}/{maxBeers}";
    }
}
