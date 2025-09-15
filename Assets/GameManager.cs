using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("HUD Panel")]
    [SerializeField] private UnityEngine.UI.Image pocketGoldPanel;

    [Header("Caisse")]
    public int goldInRegister = 10;
    public TextMeshProUGUI goldText;

    [Header("Expérience & Niveau")]
    public int playerExp = 0;
    public int requiredExp = 150;
    public int playerLevel = 1;
    public int score;
    public TextMeshProUGUI playerLevelText;
    public TextMeshProUGUI playerExpText;
    public BeerDispenser beerDispenser;

    [Header("Or sur le joueur")]
    public int goldOnPlayer = 10;
    public int maxGoldOnPlayer = 100;
    public TextMeshProUGUI goldOnPlayerText;

    private readonly float defaultWalkSpeed = 5f;
    private readonly float defaultWaitTime = 15f;
    private float currentWalkSpeed;
    public float currentWaitTime;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            currentWalkSpeed = defaultWalkSpeed;
            currentWaitTime = defaultWaitTime;

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    private void Update()
    {
        CheckGameOver();
    }

    private void CheckGameOver()
    {
        if (goldOnPlayer <= -20)
        {
            score = playerLevel * 100 + playerExp;
            ResetGameState();
            SceneManager.LoadScene("GameOver");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[GameManager] Scene loaded: {scene.name}");

        if (scene.name == "GameScene" || scene.name == "SampleScene")
        {
            StartCoroutine(ReconnectAndResetWithRetry());
        }
        else if (scene.name == "GameOver")
        {
            StartCoroutine(ReconnectUIOnly());
        }
    }

    private IEnumerator ReconnectAndResetWithRetry()
    {
        yield return null;

        if (this == null || gameObject == null)
            yield break;

        const float timeout = 2f;
        float t = 0f;
        bool ok = TryReconnectSceneObjects();

        while (!ok && t < timeout)
        {
            t += Time.unscaledDeltaTime;
            yield return null;

            if (this == null || gameObject == null)
                yield break;

            ok = TryReconnectSceneObjects();
        }

        ResetGameState();
        ForceUIRefresh();
    }

    private IEnumerator ReconnectUIOnly()
    {
        yield return null;
        TryReconnectSceneObjects();
        ForceUIRefresh();
    }

    public bool TryReconnectSceneObjects()
    {
        goldText         = FindTMPByName("GoldText");
        goldOnPlayerText = FindTMPByName("GoldOnPlayerText");
        playerExpText    = FindTMPByName("PlayerExpText");
        playerLevelText  = FindTMPByName("PlayerLevelText");

        if (pocketGoldPanel == null)
        {
            GameObject panelObj = GameObject.Find("HUDPanel");
            if (panelObj != null)
                pocketGoldPanel = panelObj.GetComponent<UnityEngine.UI.Image>();
        }

        beerDispenser = FindAnyObjectByType<BeerDispenser>();

        bool allFound = goldText && goldOnPlayerText && playerExpText && playerLevelText && pocketGoldPanel;

        Debug.Log($"[GameManager] UI reconnect: gold:{goldText != null}, pocket:{goldOnPlayerText != null}, exp:{playerExpText != null}, level:{playerLevelText != null}, panel:{pocketGoldPanel != null}");

        return allFound;
    }

    private TextMeshProUGUI FindTMPByName(string name)
    {
        var all = FindObjectsOfType<TextMeshProUGUI>(true);
        foreach (var tmp in all)
        {
            if (tmp.name == name)
                return tmp;
        }
        return null;
    }

    private void ForceUIRefresh()
    {
        UpdateGoldUI();
        UpdateExpUI();
    }

    public void ResetGameState()
    {
        goldInRegister = 0;
        goldOnPlayer = 10;
        playerExp = 0;
        playerLevel = 1;
        requiredExp = 100;
        currentWalkSpeed = defaultWalkSpeed;
        currentWaitTime = defaultWaitTime;

        if (beerDispenser != null)
            beerDispenser.refillInterval = 4f;

        Time.timeScale = 1f;
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

    public void StealFromRegister()
    {
        Debug.Log("MES SOUUUUUS");
        if (goldInRegister > 0)
            goldInRegister -= 10;
        UpdateGoldUI();
    }

    public void DepositGoldToRegister()
    {
        goldInRegister += goldOnPlayer - 20;
        goldOnPlayer = 20;
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
            playerExp -= requiredExp;
            requiredExp = Mathf.RoundToInt(requiredExp * 1.2f);
        }

        UpdateExpUI();
    }

    private void UpdateExpUI()
    {
        if (playerExpText != null)
            playerExpText.text = $"Exp : {playerExp}/{requiredExp}";
        if (playerLevelText != null)
            playerLevelText.text = $"Lvl : {playerLevel}";
    }

    public void IncreaseDifficulty()
    {
        currentWaitTime *= 0.85f;

        var cm = FindAnyObjectByType<ClientManager>();
        if (cm != null)
        {
            cm.SetDifficulty(currentWaitTime);
            cm.spawnInterval *= 0.7f;
        }

        if (beerDispenser != null && beerDispenser.refillInterval > 0.5f)
            beerDispenser.refillInterval -= 0.1f;
    }

    private void UpdatePocketGoldPanelColor()
    {
        if (pocketGoldPanel == null) return;

        if (goldOnPlayer < 0)
            pocketGoldPanel.color = Color.red;
        else if (goldOnPlayer < 20)
            pocketGoldPanel.color = new Color(1f, 0.5f, 0f);
        else if (goldOnPlayer < 50)
            pocketGoldPanel.color = Color.yellow;
        else if (goldOnPlayer == maxGoldOnPlayer)
            pocketGoldPanel.color = Color.blue;
        else
            pocketGoldPanel.color = Color.green;
    }

    public void UpdateGoldUI()
    {
        if (goldText != null)
            goldText.text = $"Cash Register : {goldInRegister}";

        if (goldOnPlayerText != null)
            goldOnPlayerText.text = $"Pocket Gold : {goldOnPlayer}/{maxGoldOnPlayer}";

        UpdatePocketGoldPanelColor();
    }
}
