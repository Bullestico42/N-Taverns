using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class CashRegister : MonoBehaviour
{
    private PlayerInventory playerInvInRange;
    private AudioSource audioSource;

    public AudioClip depositSound;
    public ParticleSystem goldParticles;


    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var inv = other.GetComponent<PlayerInventory>();
        if (inv != null)
        {
            playerInvInRange = inv;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var inv = other.GetComponent<PlayerInventory>();
        if (inv != null && inv == playerInvInRange)
        {
            playerInvInRange = null;
        }
    }

    private void Update()
    {
        if (playerInvInRange == null)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryDeposit();
        }
    }

    private void TryDeposit()
    {
        var gm = GameManager.Instance;

        if (gm.goldOnPlayer < gm.maxGoldOnPlayer)
        {
            Debug.Log("Pas assez d’or pour déposer.");
            return;
        }

        gm.DepositGoldToRegister();

        // Instancier les particules à la position de la caisse
        if (goldParticles != null)
        {
            goldParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            goldParticles.Play();
        }

        if (depositSound != null)
            audioSource.PlayOneShot(depositSound);
    }
}
