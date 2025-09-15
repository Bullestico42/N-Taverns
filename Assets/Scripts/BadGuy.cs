using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class BadGuy : MonoBehaviour
{
    public Transform exitPoint;
    public Transform targetPoint;
    public float moveSpeed = 3f;
    public ParticleSystem goldParticles;
    public ParticleSystem bloodParticles;
    public enum animState { Down = 0, Up = 1, Sit = 2 }
    static readonly int StateHash = Animator.StringToHash("State");
    public Sprite walkingSpriteEnd;
    public Sprite walkingSpriteBegin;
    public Sprite sittingSprite;
    private SpriteRenderer sr;
    private Animator anim;
    public bool canBeHit = false;
    public bool wasHit = false;
    public AudioClip ExplosionDeCaca;
    public AudioClip ExplosionDeHit;
    public int hitNums = 0;
    public int stealLoop = 5;

    private AudioSource audioSource;
    private PlayerInventory playerInvInRange;

    private void OnTriggerEnter2D(Collider2D other)
    {
        var inv = other.GetComponent<PlayerInventory>();
        if (inv != null)
            playerInvInRange = inv;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var inv = other.GetComponent<PlayerInventory>();
        if (inv != null && inv == playerInvInRange)
            playerInvInRange = null;
    }

    void Start()
    {
        gameObject.SetActive(false);
        sr = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        if (!playerInvInRange)
            return;

        bool keyboardPress = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
        bool gamepadPress = Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame;

        if (keyboardPress || gamepadPress && canBeHit && !wasHit)
        {
            Debug.Log("AIE OUILLE ALED");
            hitNums++;
            audioSource.PlayOneShot(ExplosionDeHit);
            if (bloodParticles != null)
            {
                bloodParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                bloodParticles.Play();
            }
        }
        if (hitNums >= 3)
        {
            Debug.Log("ARRETE C BON");
            wasHit = true;
        }
    }

    public void StartFillipeRoutine()
    {
        gameObject.SetActive(true);
        Debug.Log("JARRRRRIIIIIIIIIIIVE");
        SwitchToDownwalk();
        StartCoroutine(MoveToTarget());
    }

    private IEnumerator MoveToTarget()
    {
        gameObject.SetActive(true);
        transform.position = exitPoint.position;
        stealLoop = 5;
        var gm = GameManager.Instance;

        yield return StartCoroutine(MoveTowards(targetPoint.position));
        canBeHit = true;
        if (ExplosionDeCaca != null)
            audioSource.PlayOneShot(ExplosionDeCaca);
        SwitchToSitting();
        if (goldParticles != null)
        {
            while (stealLoop > 1)
            {
                if (wasHit)
                    break;
                goldParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                goldParticles.Play();
                yield return new WaitForSeconds(0.5f);
                gm.StealFromRegister();
                stealLoop--;
            }
        }
        yield return new WaitForSeconds(0.5f);
        canBeHit = false;
        moveSpeed *= 2f;
        SwitchToUpwalk();
        yield return StartCoroutine(MoveTowards(exitPoint.position));
        gameObject.SetActive(false);
        moveSpeed = 3f;
        hitNums = 0;
        wasHit = false;
    }

    private IEnumerator MoveTowards(Vector3 destination)
    {
        while (Vector2.Distance(transform.position, destination) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }

    public IEnumerator TryTriggerBadGuy()
    {
        int randomNumber = Random.Range(1, 6);
        Debug.Log("HEYYYYY");
        if (randomNumber == 3)
        {
            int randomTimer = Random.Range(1, 15);
            yield return new WaitForSeconds(randomTimer);
            StartFillipeRoutine();
        }
    }

    private void SwitchToDownwalk()
    {
        if (anim != null) anim.SetInteger(StateHash, 0);
    }

    private void SwitchToUpwalk()
    {
        if (anim != null) anim.SetInteger(StateHash, 1);
    }

    private void SwitchToSitting()
    {
        if (anim != null) anim.SetInteger(StateHash, 2);
    }

    public void ForceLeave()
    {
        StopAllCoroutines();
        gameObject.SetActive(false);
        moveSpeed = 3f;
        hitNums = 0;
        wasHit = false;
        canBeHit = false;
    }
}

