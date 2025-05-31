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

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Récupère le GameObject enfant "Visual" et son SpriteRenderer
        visual = transform.Find("Visual");
        if (visual != null)
            sr = visual.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        var kb = Keyboard.current;
        moveInput = Vector2.zero;
        if (kb.wKey.isPressed) moveInput.y += 1;
        if (kb.sKey.isPressed) moveInput.y -= 1;
        if (kb.aKey.isPressed) moveInput.x -= 1;
        if (kb.dKey.isPressed) moveInput.x += 1;
        moveInput = moveInput.normalized;
    }

    void FixedUpdate()
    {
        Vector2 targetPos = rb.position + moveInput * speed * Time.fixedDeltaTime;
        rb.MovePosition(targetPos);

        // Flip horizontal selon la direction
        if (moveInput.x > 0)
            sr.flipX = false;
        else if (moveInput.x < 0)
            sr.flipX = true;
    }
}
