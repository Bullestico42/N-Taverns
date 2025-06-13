using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

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
    public int goldOnPlayer = 0;
    public int maxGoldOnPlayer = 100;
    public TextMeshProUGUI goldOnPlayerText;

    // Valeurs par défaut pour Reset
    private readonly float defaultWalkSpeed = 5f;
    private readonly float defaultWaitTime = 15f;
    private float currentWalkSpeed;
    private float currentWaitTime;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialisation des valeurs courantes
            currentWalkSpeed = defaultWalkSpeed;
            currentWaitTime  = defaultWaitTime;
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

    void Update()
    {
        CheckGameOver();
    }

    // --- Gestion du Game Over ---

    private void CheckGameOver()
    {
        // Si l'or du joueur atteint -20 ou moins
        if (goldOnPlayer <= -20)
        {
            ResetGameState();
            // Chargement de la scène Game Over (par nom ou index)
            SceneManager.LoadScene("GameOver");
        }
    }

    private void ResetGameState()
    {
        // Remise à zéro de toutes les variables de jeu
        goldInRegister    = 0;
        goldOnPlayer      = 0;
        playerExp         = 0;
        playerLevel       = 1;
        requiredExp       = 150;
        currentWalkSpeed  = defaultWalkSpeed;
        currentWaitTime   = defaultWaitTime;

        // Si vous modifiez ces valeurs ailleurs (spawnInterval, refillInterval…), remettez-les ici aussi :
        if (beerDispenser != null)
            beerDispenser.refillInterval = 4;
        // (Assurez-vous d’exposer defaultRefillInterval dans votre BeerDispenser)

        Time.timeScale = 1f;  // en cas de pause
    }

    // --- Vos méthodes existantes ---

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
            requiredExp = (requiredExp * 120) / 100;
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

        if (beerDispenser.refillInterval > 0.5f)
            beerDispenser.refillInterval -= 0.1f;
    }

    public void UpdateGoldUI()
    {
        if (goldText != null)
            goldText.text = $"Cash Register : {goldInRegister}";

        if (goldOnPlayerText != null)
            goldOnPlayerText.text = $"Pocket Gold : {goldOnPlayer}/{maxGoldOnPlayer}";
    }
}
