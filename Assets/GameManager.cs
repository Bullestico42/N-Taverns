using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;          // pour IEnumerator / Coroutines
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("HUD Panel")]
    [SerializeField] private UnityEngine.UI.Image pocketGoldPanel; 

    [Header("Caisse")]
    public int goldInRegister = 0;
    public TextMeshProUGUI goldText;

    [Header("Expérience & Niveau")]
    public int playerExp = 0;
    public int requiredExp = 150;
    public int playerLevel = 1;
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
    private float currentWaitTime;

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
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "SampleScene")
        {
            ReconnectSceneObjects();
            ResetGameState();
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
            ResetGameState();
            SceneManager.LoadScene("GameOver");
        }
    }

    private IEnumerator ReconnectAndResetWithRetry()
    {
        // Laisse une frame au moteur pour instancier les Canvas/TMP
        yield return null;

        // Réessaye pendant ~2 s (temps réel) jusqu’à ce que l’UI soit trouvée
        const float timeout = 2f;
        float t = 0f;
        bool ok = TryReconnectSceneObjects();   // premier essai

        while (!ok && t < timeout)
        {
            t += Time.unscaledDeltaTime;        // important: temps réel même si timeScale=0
            yield return null;
            ok = TryReconnectSceneObjects();
        }

        ResetGameState();   // remet les valeurs
        UpdateGoldUI();
        UpdateExpUI();    // force l’affichage
        GainExp(0);         // met à jour les textes d’XP/Niveau
    }


    private void ForceRefreshUI()
    {
        UpdateGoldUI();
        GainExp(0);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene" || scene.name == "SampleScene")
        {
            UpdateGoldUI();
            StartCoroutine(ReconnectAndResetWithRetry());
        }
    }

    public bool TryReconnectSceneObjects()
    {
        goldText         = FindTMPByName("GoldText");
        goldOnPlayerText = FindTMPByName("GoldOnPlayerText");
        playerExpText    = FindTMPByName("PlayerExpText");
        playerLevelText  = FindTMPByName("PlayerLevelText");

        // Si ton Unity le permet, includeInactive=true pour les objets désactivés
        bool allFound = goldText && goldOnPlayerText && playerExpText && playerLevelText;

        if (!allFound)
        {
            Debug.Log($"[GameManager] Reconnect… gold:{goldText} pocket:{goldOnPlayerText} exp:{playerExpText} lvl:{playerLevelText} beer:{beerDispenser}");
        }

        return allFound;
    }

    private TextMeshProUGUI FindTMPByName(string name)
    {
        // Trouve même si l’objet est désactivé
        var all = FindObjectsOfType<TextMeshProUGUI>(true);
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i].name == name)
                return all[i];
        }
        return null;
    }

    public void ReconnectSceneObjects()
    {
        Debug.Log("🔁 ReconnectSceneObjects exécuté :");
        Debug.Log($" - goldText trouvé : {goldText != null}");
        Debug.Log($" - goldOnPlayerText trouvé : {goldOnPlayerText != null}");
        Debug.Log($" - playerExpText trouvé : {playerExpText != null}");
        Debug.Log($" - playerLevelText trouvé : {playerLevelText != null}");
        Debug.Log($" - beerDispenser trouvé : {beerDispenser != null}");
        UpdateGoldUI();
        GainExp(0);
    }

    private TextMeshProUGUI FindTextObject(string tagOrName)
    {
        GameObject obj = GameObject.FindWithTag(tagOrName);
        if (obj == null)
        {
            obj = GameObject.Find(tagOrName); // fallback si pas de tag
        }

        if (obj != null)
            return obj.GetComponent<TextMeshProUGUI>();

        return null;
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

        GainExp(0);
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
        {
            pocketGoldPanel.color = Color.red; // rouge
        }
        else if (goldOnPlayer < 20)
        {
            pocketGoldPanel.color = new Color(1f, 0.5f, 0f); // orange
        }
        else if (goldOnPlayer < 50)
        {
            pocketGoldPanel.color = Color.yellow;
        }
        else if (goldOnPlayer == 100)
        {
            pocketGoldPanel.color = Color.blue;
        }
        else
        {
            pocketGoldPanel.color = Color.green;
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


    private void UpdateExpUI()
    {
        playerLevelText.text = "Lvl : 1";
    }
}
