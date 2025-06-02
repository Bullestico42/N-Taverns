using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ShopManager : MonoBehaviour
{
    public GameObject shopUI;
    public GameObject firstSelectedButton;
    public TopDownMovement playerMovement;
    public PlayerInventory playerInventory;
    public ClientManager clientManager;
    public BeerDispenser beerDispenser;

    private bool isShopOpen = false;

    void Update()
    {
        if (!isShopOpen)
            clientManager.SetPaused(false);
    }
    public void ToggleShop()
    {
        isShopOpen = !isShopOpen;
        shopUI.SetActive(isShopOpen);
        clientManager.SetPaused(true);
        playerMovement.enabled = !isShopOpen;

        if (isShopOpen)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
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
            Debug.Log("Pas de sous");
        }
    }

    public void UpgradeBeerCapacity()
    {
        if (GameManager.Instance.goldInRegister >= 100)
        {
            GameManager.Instance.PayAmountWithRegister(100);
            playerInventory.maxBeers += 1;
            Debug.Log("Capacité de bières augmentée !");
            GameManager.Instance.UpdateGoldUI();
            playerInventory.UpdateBeerUI();
        }
        else
        {
            Debug.Log("t povr");
        }
    }

    public void UpgradeClientSpawnRate()
    {
        if (GameManager.Instance.goldInRegister >= 100)
        {
            if (clientManager.spawnInterval > 1f)
            {
                GameManager.Instance.PayAmountWithRegister(100);
                clientManager.spawnInterval += 0.2f;
                Debug.Log($"Nouveau délai entre spawns : {clientManager.spawnInterval}s");
            }
            else
            {
                Debug.Log("Intervalle minimum atteint !");
            }
        }
        else
        {
            Debug.Log("Padsou");
        }
    }
    
    public void UpgradeDispenserRefillSpeed()
    {
        if (GameManager.Instance.goldInRegister >= 80)
        {
            if (beerDispenser.refillInterval > 0.5f)
            {
                GameManager.Instance.PayAmountWithRegister(80);
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
            Debug.Log("Wayayouille");
        }
    }

    public void UpgradeDispenserCapacity()
    {
        if (GameManager.Instance.goldInRegister >= 80)
        {
            if (beerDispenser.maxBeers < 10)
            {
                GameManager.Instance.PayAmountWithRegister(80);
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
            Debug.Log("waos");
        }
    }

    public void UpgradeClientImpatience()
    {
        if (GameManager.Instance.goldInRegister >= 100)
        {    
            if (clientManager.clientPrefabSettings.maxWaitTime > 3f)
            {
                GameManager.Instance.PayAmountWithRegister(100);
                clientManager.clientPrefabSettings.maxWaitTime += 1f;
                Debug.Log($"Nouvelle patience des clients : {clientManager.clientPrefabSettings.maxWaitTime}s");
            }
            else
            {
                Debug.Log("Les clients sont déjà très impatients !");
            }
        }
        else
        {
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
