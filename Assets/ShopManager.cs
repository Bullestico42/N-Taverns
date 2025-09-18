using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

public class ShopManager : MonoBehaviour
{
    [Header("UI Root")]
    [SerializeField] private GameObject shopUI;                 // Doit contenir un RectTransform
    [SerializeField] private GameObject firstSelectedButton;    // Bouton à focus à l’ouverture

    [Header("Gameplay Refs")]
    [SerializeField] private TopDownMovement playerMovement;
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private ClientManager clientManager;
    [SerializeField] private BeerDispenser beerDispenser;

    [Header("Animation")]
    [SerializeField] private float animationDuration = 0.5f;

    [Header("Audio")]
    [SerializeField] private AudioClip GlissementDeTerrainSound;
    [SerializeField] private AudioClip Error;

    [Header("Boutons d'upgrade")]
    [SerializeField] private Button btnSpeed;
    [SerializeField] private Button btnBeerCap;
    [SerializeField] private Button btnSpawnRate;
    [SerializeField] private Button btnRefillSpeed;
    [SerializeField] private Button btnImpatience;

    // ---- caps & coût (ajuste selon le game design) ----
    private const int   COST       = 50;
    private const float SPEED_MAX  = 8f;
    private const int   BEERS_MAX  = 10;
    private const float REFILL_MIN = 0.5f; // s
    private const float SPAWN_MAX  = 6f;   // s (on autorise d'augmenter l'intervalle jusqu'à 6s)

    // ---- internes ----
    private AudioSource audioSource;
    private bool isShopOpen = false;
    private RectTransform shopRect;
    private Vector2 hiddenPosition;
    private Vector2 visiblePosition;
    private Coroutine currentAnimation;

    // Couleur pour bouton désactivé (foncé, OPAQUE)
    private static readonly Color DISABLED_DARK = new Color(0.25f, 0.25f, 0.25f, 1f);

    // ========================
    //      Lifecycle
    // ========================
    private void Awake()
    {
        EnsureAudioSource();
    }

    private void Start()
    {
        if (shopUI == null)
        {
            Debug.LogError("[ShopManager] shopUI n'est pas assigné.");
            enabled = false;
            return;
        }

        shopRect = shopUI.GetComponent<RectTransform>();
        if (shopRect == null)
        {
            Debug.LogError("[ShopManager] shopUI n'a pas de RectTransform.");
            enabled = false;
            return;
        }

        visiblePosition = shopRect.anchoredPosition;
        hiddenPosition  = new Vector2(visiblePosition.x, -Screen.height);

        shopRect.anchoredPosition = hiddenPosition;
        shopUI.SetActive(false);

        RefreshButtons();
    }

    private void Update()
    {
        if (!isShopOpen && clientManager != null)
            clientManager.SetPaused(false);
    }

    // ========================
    //   Open / Close Shop
    // ========================
    public void ToggleShop()
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        EnsureAudioSource();

        if (isShopOpen)
        {
            currentAnimation = StartCoroutine(CloseShopAnimated());
        }
        else
        {
            if (GlissementDeTerrainSound != null)
                audioSource.PlayOneShot(GlissementDeTerrainSound);

            currentAnimation = StartCoroutine(OpenShopAnimated());
        }
    }

    private IEnumerator OpenShopAnimated()
    {
        isShopOpen = true;
        shopUI.SetActive(true);

        if (clientManager != null) clientManager.SetPaused(true);
        if (playerMovement != null) playerMovement.enabled = false;

        RefreshButtons();

        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            float t = Mathf.SmoothStep(0f, 1f, elapsed / animationDuration);
            shopRect.anchoredPosition = Vector2.Lerp(hiddenPosition, visiblePosition, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        shopRect.anchoredPosition = visiblePosition;

        SelectFirstAvailableButton();
    }

    private IEnumerator CloseShopAnimated()
    {
        isShopOpen = false;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            float t = Mathf.SmoothStep(0f, 1f, elapsed / animationDuration);
            shopRect.anchoredPosition = Vector2.Lerp(visiblePosition, hiddenPosition, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        shopRect.anchoredPosition = hiddenPosition;
        shopUI.SetActive(false);

        if (clientManager != null) clientManager.SetPaused(false);
        if (playerMovement != null) playerMovement.enabled = true;
    }

    // ========================
    //        Upgrades
    // ========================
    public void UpgradeSpeed()
    {
        if (GameManager.Instance.goldInRegister >= COST && playerMovement != null && playerMovement.speed < SPEED_MAX)
        {
            GameManager.Instance.PayAmountWithRegister(COST);
            playerMovement.speed = Mathf.Min(SPEED_MAX, playerMovement.speed + 1f);
        }
        else PlayError();

        RefreshButtons();
    }

    public void UpgradeBeerCapacity()
    {
        if (GameManager.Instance.goldInRegister >= COST && playerInventory != null && playerInventory.maxBeers < BEERS_MAX)
        {
            GameManager.Instance.PayAmountWithRegister(COST);
            playerInventory.maxBeers = Mathf.Min(BEERS_MAX, playerInventory.maxBeers + 1);
            GameManager.Instance.UpdateGoldUI();
            playerInventory.UpdateBeerUI();
        }
        else PlayError();

        RefreshButtons();
    }

    public void UpgradeClientSpawnRate()
    {
        // Ici, on "augmente le délai" jusqu'à SPAWN_MAX (moins de clients). Inverse la logique si besoin.
        if (GameManager.Instance.goldInRegister >= COST && clientManager != null && clientManager.spawnInterval < SPAWN_MAX)
        {
            GameManager.Instance.PayAmountWithRegister(COST);
            clientManager.spawnInterval = Mathf.Min(SPAWN_MAX, clientManager.spawnInterval + 0.2f);
            Debug.Log($"Nouveau délai entre spawns : {clientManager.spawnInterval:F1}s");
        }
        else PlayError();

        RefreshButtons();
    }

    public void UpgradeDispenserRefillSpeed()
    {
        if (GameManager.Instance.goldInRegister >= COST && beerDispenser != null && beerDispenser.refillInterval > REFILL_MIN)
        {
            GameManager.Instance.PayAmountWithRegister(COST);
            beerDispenser.refillInterval = Mathf.Max(REFILL_MIN, beerDispenser.refillInterval - 0.5f);
            Debug.Log($"Nouvel intervalle de recharge : {beerDispenser.refillInterval:F1}s");
        }
        else PlayError();

        RefreshButtons();
    }

    public void UpgradeClientImpatience()
    {
        if (clientManager == null) { PlayError(); return; }

        float cur = GameManager.Instance.currentWaitTime;
        float cap = clientManager.clientPrefabSettings.maxWaitTime;

        if (GameManager.Instance.goldInRegister >= COST && cur < cap)
        {
            GameManager.Instance.PayAmountWithRegister(COST);
            GameManager.Instance.currentWaitTime = Mathf.Min(cap, cur * 1.1f);
        }
        else PlayError();

        RefreshButtons();
    }

    public void CloseShop()
    {
        isShopOpen = false;
        shopUI.SetActive(false);
        if (clientManager != null) clientManager.SetPaused(false);
        if (playerMovement != null) playerMovement.enabled = true;
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void SuperEquilibrage()
    {
        if (playerMovement != null) playerMovement.speed /= 2f;
        if (playerInventory != null) playerInventory.maxBeers -= 4;
        if (clientManager != null)
        {
            clientManager.spawnInterval /= 2f;
            clientManager.clientPrefabSettings.maxWaitTime /= 1.5f;
        }
        RefreshButtons();
    }

    // ========================
    //        UI Helpers
    // ========================
    public void RefreshButtons()
    {
        SetButtonState(btnSpeed,       CanUpgradeSpeed());
        SetButtonState(btnBeerCap,     CanUpgradeBeerCap());
        SetButtonState(btnSpawnRate,   CanUpgradeSpawnRate());
        SetButtonState(btnRefillSpeed, CanUpgradeRefill());
        SetButtonState(btnImpatience,  CanUpgradeImpatience());

        // Si le bouton sélectionné est désactivé, re-sélectionner un valide
        var es = EventSystem.current;
        if (es != null)
        {
            var cur = es.currentSelectedGameObject;
            if (cur != null)
            {
                var b = cur.GetComponent<Button>();
                if (b != null && !b.interactable)
                    StartCoroutine(ReselectAfterFrame(b));
            }
        }
    }

    private void SetButtonState(Button btn, bool enabled)
    {
        if (!btn) return;

        var es = EventSystem.current;
        bool wasSelected = es != null && es.currentSelectedGameObject == btn.gameObject;

        // Interactabilité
        btn.interactable = enabled;

        // Teinte visuelle (foncé, opaque)
        var cb = btn.colors;
        cb.disabledColor = DISABLED_DARK;
        cb.fadeDuration = 0.05f;
        btn.colors = cb;

        // Si on vient de désactiver le bouton sélectionné, re-sélectionner au prochain frame
        if (wasSelected && !enabled)
            StartCoroutine(ReselectAfterFrame(btn));
    }

    private IEnumerator ReselectAfterFrame(Button from)
    {
        yield return null; // attendre la fin du frame du click

        var es = EventSystem.current;
        if (es == null) yield break;

        GameObject cur = es.currentSelectedGameObject;
        if (cur != null && cur.TryGetComponent<Button>(out var b) && b.interactable) yield break;

        SelectNextButton(from);
    }

    private void SelectNextButton(Button from)
    {
        var es = EventSystem.current;
        if (es == null || shopUI == null) return;

        var all = shopUI.GetComponentsInChildren<Button>(true);
        int idx = System.Array.IndexOf(all, from);

        // Vers l’avant
        for (int i = idx + 1; i < all.Length; i++)
            if (all[i].interactable) { es.SetSelectedGameObject(all[i].gameObject); return; }

        // Vers l’arrière
        for (int i = idx - 1; i >= 0; i--)
            if (all[i].interactable) { es.SetSelectedGameObject(all[i].gameObject); return; }

        // Premier interactable
        foreach (var btn in all)
            if (btn.interactable) { es.SetSelectedGameObject(btn.gameObject); return; }

        es.SetSelectedGameObject(null);
    }

    private void SelectFirstAvailableButton()
    {
        var es = EventSystem.current;
        if (es == null) return;

        if (firstSelectedButton != null)
        {
            var fb = firstSelectedButton.GetComponent<Button>();
            if (fb == null || fb.interactable)
            {
                es.SetSelectedGameObject(firstSelectedButton);
                return;
            }
        }

        var buttons = shopUI.GetComponentsInChildren<Button>(true);
        foreach (var b in buttons)
        {
            if (b.interactable)
            {
                es.SetSelectedGameObject(b.gameObject);
                return;
            }
        }
        es.SetSelectedGameObject(null);
    }

    // Conditions
    private bool HasGold(int amount) => GameManager.Instance.goldInRegister >= amount;
    private bool CanUpgradeSpeed()      => playerMovement != null && HasGold(COST) && playerMovement.speed < SPEED_MAX;
    private bool CanUpgradeBeerCap()    => playerInventory != null && HasGold(COST) && playerInventory.maxBeers < BEERS_MAX;
    private bool CanUpgradeSpawnRate()  => clientManager   != null && HasGold(COST) && clientManager.spawnInterval < SPAWN_MAX;
    private bool CanUpgradeRefill()     => beerDispenser   != null && HasGold(COST) && beerDispenser.refillInterval > REFILL_MIN;
    private bool CanUpgradeImpatience()
    {
        if (clientManager == null) return false;
        return HasGold(COST) && GameManager.Instance.currentWaitTime < clientManager.clientPrefabSettings.maxWaitTime;
    }

    // ========================
    //       Audio Helpers
    // ========================
    private void EnsureAudioSource()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // UI = 2D
        }
    }

    private void PlayError()
    {
        EnsureAudioSource();
        if (Error != null)
            audioSource.PlayOneShot(Error);
    }

    // Optionnel : à appeler si l'or change en dehors du shop pour MAJ immédiate
    public void RefreshShopUI() => RefreshButtons();
}
