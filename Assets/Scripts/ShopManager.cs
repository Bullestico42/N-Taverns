using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections;

public class ShopManager : MonoBehaviour
{
    public GameObject shopUI; // Doit contenir le RectTransform
    public GameObject firstSelectedButton;
    public TopDownMovement playerMovement;
    public PlayerInventory playerInventory;
    public ClientManager clientManager;
    public BeerDispenser beerDispenser;
    public float animationDuration = 0.5f;
    public AudioClip GlissementDeTerrainSound;
    public AudioClip Error;


    private AudioSource audioSource;
    private bool isShopOpen = false;
    private RectTransform shopRect;
    private Vector2 hiddenPosition;
    private Vector2 visiblePosition;
    private Coroutine currentAnimation;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        shopRect = shopUI.GetComponent<RectTransform>();

        // Position visible actuelle dans l’éditeur
        visiblePosition = shopRect.anchoredPosition;

        // Position cachée : en dessous de l’écran
        hiddenPosition = new Vector2(visiblePosition.x, -Screen.height);

        shopRect.anchoredPosition = hiddenPosition;
        shopUI.SetActive(false);
    }

    void Update()
    {
        if (!isShopOpen)
            clientManager.SetPaused(false);
    }

    public void ToggleShop()
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);
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
        clientManager.SetPaused(true);
        playerMovement.enabled = false;

        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            float t = elapsed / animationDuration;
            t = Mathf.SmoothStep(0, 1, t);
            shopRect.anchoredPosition = Vector2.Lerp(hiddenPosition, visiblePosition, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        shopRect.anchoredPosition = visiblePosition;

        // Focus bouton
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }

    private IEnumerator CloseShopAnimated()
    {
        isShopOpen = false;

        float elapsed = 0f;

        // Unfocus bouton
        EventSystem.current.SetSelectedGameObject(null);

        while (elapsed < animationDuration)
        {
            float t = elapsed / animationDuration;
            t = Mathf.SmoothStep(0, 1, t);
            shopRect.anchoredPosition = Vector2.Lerp(visiblePosition, hiddenPosition, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        shopRect.anchoredPosition = hiddenPosition;
        shopUI.SetActive(false);
        clientManager.SetPaused(false);
        playerMovement.enabled = true;
    }

    public void UpgradeInstantBeer()
    {
        if (GameManager.Instance.goldInRegister >= 50)
        {
            GameManager.Instance.PayAmountWithRegister(50);
            BoostManager.Instance.instantBeerUpgrade++;
        }
        else
        {
            if (Error != null)
                audioSource.PlayOneShot(Error);
        }
    }

    public void UpgradeSlowTime()
    {
        if (GameManager.Instance.goldInRegister >= 50)
        {
            GameManager.Instance.PayAmountWithRegister(50);
            BoostManager.Instance.slowTimeUpgrade++;
        }
        else
        {
            if (Error != null)
                audioSource.PlayOneShot(Error);
        }
    }

    public void UpgradeRemoveThief()
    {
        if (GameManager.Instance.goldInRegister >= 50)
        {
            GameManager.Instance.PayAmountWithRegister(50);
            BoostManager.Instance.removeThiefUpgrade++;
        }
        else
        {
            if (Error != null)
                audioSource.PlayOneShot(Error);
        }
    }

    public void CloseShop()
    {
        isShopOpen = false;
        shopUI.SetActive(false);
        clientManager.SetPaused(false);
        playerMovement.enabled = true;
        EventSystem.current.SetSelectedGameObject(null);
    }
}
