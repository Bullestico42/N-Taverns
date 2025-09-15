using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("HUD Panel")]
    [SerializeField] private Image pocketGoldPanel;

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

    // ======== Flèche (SpriteRenderer en scène) ========
    [Header("Arrow (SpriteRenderer)")]
    [SerializeField] private SpriteRenderer arrowSprite; // assigne ton objet SpriteRenderer ici
    [SerializeField] private float moveDistance = 0.5f;  // déplacement local Y entre 0 et -0.5
    [SerializeField] private float moveDuration = 0.5f;  // durée d’un aller (plus petit = plus rapide)
    private Coroutine arrowCoroutine;
    private bool arrowRotatedOnce = false;
    // ==================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            currentWalkSpeed = defaultWalkSpeed;
            currentWaitTime  = defaultWaitTime;

            if (arrowSprite != null)
                arrowSprite.gameObject.SetActive(false);

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
        if (this == null || gameObject == null) yield break;

        const float timeout = 2f;
        float t = 0f;
        bool ok = TryReconnectSceneObjects();

        while (!ok && t < timeout)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
            if (this == null || gameObject == null) yield break;
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
                pocketGoldPanel = panelObj.GetComponent<Image>();
        }

        beerDispenser = FindAnyObjectByType<BeerDispenser>();

        if (arrowSprite != null)
            arrowSprite.gameObject.SetActive(false);

        bool allFound = goldText && goldOnPlayerText && playerExpText && playerLevelText && pocketGoldPanel;
        return allFound;
    }

    private TextMeshProUGUI FindTMPByName(string name)
    {
        var all = FindObjectsOfType<TextMeshProUGUI>(true);
        foreach (var tmp in all)
            if (tmp.name == name) return tmp;
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
        goldOnPlayer   = 10;
        playerExp      = 0;
        playerLevel    = 1;
        requiredExp    = 100;
        currentWalkSpeed = defaultWalkSpeed;
        currentWaitTime  = defaultWaitTime;

        if (beerDispenser != null)
            beerDispenser.refillInterval = 4f;

        Time.timeScale = 1f;

        StopArrowAnim(); // reset flèche
    }

    public bool CanReceiveGold(int amount) => goldOnPlayer + amount <= maxGoldOnPlayer;

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
        if (goldInRegister > 0) goldInRegister -= 10;
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
        if (playerExpText   != null) playerExpText.text   = $"Exp : {playerExp}/{requiredExp}";
        if (playerLevelText != null) playerLevelText.text = $"Lvl : {playerLevel}";
    }

    public void IncreaseDifficulty()
    {
        currentWaitTime *= 0.85f;

        var cm = FindAnyObjectByType<ClientManager>();
        if (cm != null)
        {
            cm.SetDifficulty(currentWaitTime);
            cm.spawnInterval *= 0.5f;
        }
    }

    // =================== ARROW ANIM ===================
    public void setArrowAnim()
    {
        if (arrowSprite == null) return;

        if (!arrowSprite.gameObject.activeSelf)
            arrowSprite.gameObject.SetActive(true);

        // rotation 90° sur Z une seule fois
        if (!arrowRotatedOnce)
        {
            arrowSprite.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            arrowRotatedOnce = true;
        }

        if (arrowCoroutine == null)
            arrowCoroutine = StartCoroutine(ArrowLoop());
    }

    private void StopArrowAnim()
    {
        if (arrowCoroutine != null)
        {
            StopCoroutine(arrowCoroutine);
            arrowCoroutine = null;
        }

        if (arrowSprite != null)
            arrowSprite.gameObject.SetActive(false);
    }

    private IEnumerator ArrowLoop()
    {
        // Position de base au moment du start
        Vector3 basePos = arrowSprite.transform.localPosition;
        Vector3 downPos = basePos + Vector3.down * moveDistance;

        float t = 0f;

        // Tant que l’or est au max, on anime ; sinon on sort proprement
        while (goldOnPlayer == maxGoldOnPlayer)
        {
            t += Time.deltaTime;

            // PingPong entre 0 et 1 sur une durée "moveDuration"
            float k = Mathf.PingPong(t / moveDuration, 1f);
            arrowSprite.transform.localPosition = Vector3.Lerp(basePos, downPos, k);

            yield return null;
        }

        // on n’est plus au max → reset et couper
        arrowSprite.transform.localPosition = basePos;
        arrowSprite.gameObject.SetActive(false);
        arrowCoroutine = null;
    }
    // ==================================================

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
        {
            pocketGoldPanel.color = Color.blue;
            setArrowAnim();           // démarre/continue l’anim
        }
        else
        {
            pocketGoldPanel.color = Color.green;
            StopArrowAnim();          // coupe instant si plus max
        }
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
