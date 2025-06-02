using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Caisse")]
    public int goldInRegister = 0;
    public TextMeshProUGUI goldText;

    [Header("Or sur le joueur")]
    public int goldOnPlayer = 0;
    public int maxGoldOnPlayer = 100;
    public TextMeshProUGUI goldOnPlayerText;

    void Awake()
    {
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

    public bool CanReceiveGold(int amount)
    {
        return goldOnPlayer + amount <= maxGoldOnPlayer;
    }

    public bool AddGoldToPlayer(int amount)
    {
        if (!CanReceiveGold(amount))
            goldOnPlayer = maxGoldOnPlayer;
        else
            goldOnPlayer += amount;
        UpdateGoldUI();
        return true;
    }

    public void DepositGoldToRegister()
    {
        goldInRegister += goldOnPlayer;
        goldOnPlayer = 10;
        UpdateGoldUI();
    }

    public void UpdateGoldUI()
    {
        if (goldText != null)
            goldText.text = $"Cash Register : {goldInRegister}";

        if (goldOnPlayerText != null)
            goldOnPlayerText.text = $"Pocket Gold : {goldOnPlayer}/{maxGoldOnPlayer}";
    }
}
