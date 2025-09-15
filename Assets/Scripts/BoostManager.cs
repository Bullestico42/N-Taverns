using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public enum BoostType
{
    InstantBeer,
    SlowTime,
    RemoveThief
}

public class BoostManager : MonoBehaviour
{
    public static BoostManager Instance { get; private set; }

    private readonly bool[] charged = new bool[3];

    [Header("Upgrades")]
    public int instantBeerUpgrade = 0;
    public int slowTimeUpgrade = 0;
    public int removeThiefUpgrade = 0;

    private void Awake()
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

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            ActivateInstantBeer();
        if (Keyboard.current.digit2Key.wasPressedThisFrame)
            ActivateSlowTime();
        if (Keyboard.current.digit3Key.wasPressedThisFrame)
            ActivateRemoveThief();
    }

    public void AddCharge(BoostType type)
    {
        charged[(int)type] = true;
        // Mise à jour de l'UI éventuelle ici
    }

    public void ActivateInstantBeer()
    {
        if (!charged[(int)BoostType.InstantBeer]) return;

        var clients = FindObjectsOfType<Client>();
        int served = 0;
        foreach (var c in clients)
        {
            if (c.IsWaitingBeer)
            {
                c.ForceServe();
                served++;
            }
        }

        if (instantBeerUpgrade > 0 && served > 0)
            GameManager.Instance.AddGoldToPlayer(instantBeerUpgrade * 2 * served);

        charged[(int)BoostType.InstantBeer] = false;
    }

    public void ActivateSlowTime()
    {
        if (!charged[(int)BoostType.SlowTime]) return;
        float duration = 5f + slowTimeUpgrade * 2f;
        StartCoroutine(SlowTimeRoutine(duration));
        charged[(int)BoostType.SlowTime] = false;
    }

    private IEnumerator SlowTimeRoutine(float duration)
    {
        Time.timeScale = 0.5f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }

    public void ActivateRemoveThief()
    {
        if (!charged[(int)BoostType.RemoveThief]) return;

        var badGuy = FindAnyObjectByType<BadGuy>();
        if (badGuy != null && badGuy.gameObject.activeSelf)
        {
            badGuy.ForceLeave();

            if (removeThiefUpgrade > 0)
                GameManager.Instance.AddGoldToPlayer(20 * removeThiefUpgrade);
        }

        charged[(int)BoostType.RemoveThief] = false;
    }
}

