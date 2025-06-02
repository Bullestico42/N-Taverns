using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Caisse")]
    public int goldInRegister = 0;
    public TextMeshProUGUI goldText;

    public int playerExp = 0;
    public int requiredExp = 150;
    public int playerLevel = 1;
    public TextMeshProUGUI playerLevelText;
    public TextMeshProUGUI playerExpText;

    [Header("Or sur le joueur")]
    public int goldOnPlayer = 0;
    public int maxGoldOnPlayer = 100;
    public TextMeshProUGUI goldOnPlayerText;

    private float currentWalkSpeed = 5f;
    private float currentWaitTime = 15f;

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
        GainExp(0);
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

    public void PayAmountWithRegister(int amount)
    {
        goldInRegister -= amount;
        UpdateGoldUI();
    }

    public void GainExp(int amount)
    {
        playerExp += amount;

        if (playerExp >= requiredExp)
        {
            playerLevel++;
            IncreaseDifficulty();
            playerExp = playerExp - requiredExp;
            requiredExp = (requiredExp * 120) / 100;
        }
        if (playerExpText != null)
            playerExpText.text = $"Exp : {playerExp}/{requiredExp}";
        if (playerLevelText != null)
            playerLevelText.text = $"Lvl : {playerLevel}";
    }

    public void IncreaseDifficulty()
    {
        currentWalkSpeed *= 1.3f;
        currentWaitTime *= 0.85f;

        var cm = FindAnyObjectByType<ClientManager>();
        if (cm != null)
        {
            cm.SetDifficulty(currentWalkSpeed, currentWaitTime);
            cm.spawnInterval *= 0.7f;
        }
    }

    public void UpdateGoldUI()
    {
        if (goldText != null)
            goldText.text = $"Cash Register : {goldInRegister}";

        if (goldOnPlayerText != null)
            goldOnPlayerText.text = $"Pocket Gold : {goldOnPlayer}/{maxGoldOnPlayer}";
    }
}
