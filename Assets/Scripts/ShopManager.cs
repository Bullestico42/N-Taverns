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

    public void UpgradeSpeed()
    {
        if (GameManager.Instance.goldInRegister >= 50)
        {
            GameManager.Instance.PayAmountWithRegister(50);
            playerMovement.speed += 1f;
        }
        else
        {
            Debug.Log("Pas de sous dans la banque");
            if (Error != null)
                audioSource.PlayOneShot(Error);
        }
    }

    public void UpgradeGoldCap()
    {
        if (GameManager.Instance.goldInRegister >= 50)
        {
            GameManager.Instance.PayAmountWithRegister(50);
            GameManager.Instance.maxGoldOnPlayer += 20;
            GameManager.Instance.UpdateGoldUI();
        }
        else
        {
            if (Error != null)
                audioSource.PlayOneShot(Error);
            Debug.Log("Pas de sous");
        }
    }

    public void UpgradeBeerCapacity()
    {
        if (GameManager.Instance.goldInRegister >= 50)
        {
            GameManager.Instance.PayAmountWithRegister(50);
            playerInventory.maxBeers += 1;
            Debug.Log("Capacité de bières augmentée !");
            GameManager.Instance.UpdateGoldUI();
            playerInventory.UpdateBeerUI();
        }
        else
        {
            if (Error != null)
                audioSource.PlayOneShot(Error);
            Debug.Log("t povr");
        }
    }

    public void UpgradeClientSpawnRate()
    {
        if (GameManager.Instance.goldInRegister >= 50)
        {
            if (clientManager.spawnInterval > 1f)
            {
                GameManager.Instance.PayAmountWithRegister(50);
                clientManager.spawnInterval += 0.2f;
                Debug.Log($"Nouveau délai entre spawns : {clientManager.spawnInterval}s");
            }
            else
            {
                if (Error != null)
                    audioSource.PlayOneShot(Error);
                Debug.Log("Intervalle minimum atteint !");
            }
        }
        else
        {
            if (Error != null)
                audioSource.PlayOneShot(Error);
            Debug.Log("Padsou");
        }
    }
    
    public void UpgradeDispenserRefillSpeed()
    {
        if (GameManager.Instance.goldInRegister >= 50)
        {
            if (beerDispenser.refillInterval > 0.5f)
            {
                GameManager.Instance.PayAmountWithRegister(50);
                beerDispenser.refillInterval -= 0.5f;
                Debug.Log($"Nouvel intervalle de recharge : {beerDispenser.refillInterval}s");
            }
            else
            {
                Debug.Log("Recharge déjà à la vitesse minimale !");
            }
        }
        else
        {
            if (Error != null)
                audioSource.PlayOneShot(Error);
            Debug.Log("Wayayouille");
        }
    }

    public void UpgradeDispenserCapacity()
    {
        if (GameManager.Instance.goldInRegister >= 50)
        {
            if (beerDispenser.maxBeers < 10)
            {
                GameManager.Instance.PayAmountWithRegister(50);
                beerDispenser.maxBeers += 1;
                Debug.Log($"Nouvelle capacité max du distributeur : {beerDispenser.maxBeers}");
            }
            else
            {
                Debug.Log("Capacité maximale atteinte !");
            }
        }
        else
        {
            if (Error != null)
                audioSource.PlayOneShot(Error);
            Debug.Log("waos");
        }
    }

    public void UpgradeClientImpatience()
    {
        if (GameManager.Instance.goldInRegister >= 50)
        {    
            if (clientManager.clientPrefabSettings.maxWaitTime > 3f)
            {
                GameManager.Instance.PayAmountWithRegister(50);
                GameManager.Instance.currentWaitTime *= 1.1f;
                Debug.Log($"UPGRADED OLALA {GameManager.Instance.currentWaitTime}");
            }
            else
            {
                Debug.Log("Les clients sont déjà très impatients !");
            }
        }
        else
        {
            if (Error != null)
                audioSource.PlayOneShot(Error);
            Debug.Log("Oofa");
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
