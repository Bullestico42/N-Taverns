using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class TopDownMovement : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;

    private Transform visual;
    private SpriteRenderer sr;

    private Animator animator;
    private PlayerInventory inventory;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Récupère le GameObject enfant "Visual" et son SpriteRenderer
        visual = transform.Find("Visual");
        if (visual != null)
        {
            sr = visual.GetComponent<SpriteRenderer>();
            animator = visual.GetComponent<Animator>();
        }
        inventory = GetComponent<PlayerInventory>();
    }

    void Update()
    {
        moveInput = Vector2.zero;

        // 🎮 Clavier
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.wKey.isPressed) moveInput.y += 1;
            if (kb.sKey.isPressed) moveInput.y -= 1;
            if (kb.aKey.isPressed) moveInput.x -= 1;
            if (kb.dKey.isPressed) moveInput.x += 1;
        }

        // 🕹️ Manette / Stick arcade (XInput)
        var gp = Gamepad.current;
        if (gp != null)
        {
            Vector2 stick = gp.leftStick.ReadValue();
            if (stick.magnitude > 0.1f) // remplace le clavier si utilisé
                moveInput = stick;
        }

        moveInput = moveInput.normalized;
        UpdateAnimator();
    }

    void UpdateAnimator()
    {
        if (animator == null || inventory == null) return;

        animator.SetBool("HasBeer", inventory.currentBeers > 0);
    }

    void FixedUpdate()
    {
        Vector2 targetPos = rb.position + moveInput * speed * Time.fixedDeltaTime;
        rb.MovePosition(targetPos);

        // Flip sprite sur l'axe horizontal
        if (moveInput.x > 0)
            sr.flipX = false;
        else if (moveInput.x < 0)
            sr.flipX = true;
    }
}
