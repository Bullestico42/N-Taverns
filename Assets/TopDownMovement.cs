using UnityEngine;
using UnityEngine.InputSystem;  // ou Input.GetAxis si vous êtes en ancien Input System

[RequireComponent(typeof(Rigidbody2D))]
public class TopDownMovement : MonoBehaviour
{
    public float speed = 5f;
    Rigidbody2D rb;
    Vector2 moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void Update()
    {
        // Lecture simple du clavier (nouveau Input System)
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
        // Version MovePosition pour collisions plus fiables
        Vector2 targetPos = rb.position + moveInput * speed * Time.fixedDeltaTime;
        rb.MovePosition(targetPos);

        // --- OU, si vous préférez la vélocité : ---
        // rb.velocity = moveInput * speed;
    }
}
