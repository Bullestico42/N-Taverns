using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Argent")]
    public int gold = 0;
    public TextMeshProUGUI goldText;  // 2) On utilise TMP ici

    void Awake()
    {
        // Singleton basique
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateGoldUI();
    }

    public void AddGold(int amount)
    {
        gold += amount;
        Debug.Log($"Or ajouté : {amount}. Total = {gold}");
        UpdateGoldUI();
    }

    private void UpdateGoldUI()
    {
        if (goldText != null)
            goldText.text = $"Or : {gold}";
    }
}
