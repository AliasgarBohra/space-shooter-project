using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference moveUpAction;
    [SerializeField] private SpriteRenderer spriteRend;
    [SerializeField] private ParticleSystem eliminationParticle;

    [Header("Movement Settings")]
    [SerializeField] private float forwardSpeed = 5f;
    [SerializeField] private float upForce = 10f;
    [SerializeField] private float fallSpeed = 5f;

    [Header("Ground / Ceiling Detection")]
    [SerializeField] private float rayDistance = 0.6f;
    [SerializeField] private float snapTolerance = 0.05f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Front Collision Detection")]
    [SerializeField] private Vector2 frontBoxSize = new Vector2(0.3f, 1f);
    [SerializeField] private Vector2 frontBoxOffset = new Vector2(0.5f, 0f);
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Player Size")]
    [SerializeField] private float halfHeight = 0.5f;

    [Header("Rotation Settings")]
    [SerializeField] private float tiltUpAngle = -60f;
    [SerializeField] private float tiltDownAngle = -120f;
    [SerializeField] private float tiltSpeed = 5f;
    [SerializeField] private float groundRotation = -90f;

    [Header("Trail Settings")]
    [SerializeField] private TrailRenderer trailRend;
    [SerializeField] private float minWidth = 0.1f;
    [SerializeField] private float maxWidth = 0.5f;
    [SerializeField] private float pulseSpeed = 2f;

    private Rigidbody2D rb;
    private bool isUpPressed;
    private bool isGrounded;
    private bool isAtCeiling;

    private bool isEliminated = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        trailRend = GetComponent<TrailRenderer>();
    }

    private void OnEnable()
    {
        moveUpAction.action.performed += OnUpPressed;
        moveUpAction.action.canceled += OnUpReleased;
    }

    private void OnDisable()
    {
        moveUpAction.action.performed -= OnUpPressed;
        moveUpAction.action.canceled -= OnUpReleased;
    }

    private void OnUpPressed(InputAction.CallbackContext ctx) => isUpPressed = true;
    private void OnUpReleased(InputAction.CallbackContext ctx) => isUpPressed = false;

    private void Update()
    {
        if (isEliminated)
            return;

        // Create oscillation between minWidth and maxWidth
        float t = (Mathf.Sin(Time.time * pulseSpeed * Mathf.PI * 2f) + 1f) / 2f;
        float width = Mathf.Lerp(minWidth, maxWidth, t);

        trailRend.startWidth = width;
        trailRend.endWidth = width * 0.5f; // taper to smaller end
    }
    private void FixedUpdate()
    {
        if (isEliminated)
            return;

        Vector2 pos = transform.position;

        // --- Raycasts ---
        RaycastHit2D hitDown = Physics2D.Raycast(pos, Vector2.down, rayDistance, groundLayer);
        RaycastHit2D hitUp = Physics2D.Raycast(pos, Vector2.up, rayDistance, groundLayer);

        isGrounded = hitDown.collider != null && Mathf.Abs(hitDown.distance - halfHeight) < snapTolerance;
        isAtCeiling = hitUp.collider != null && Mathf.Abs(hitUp.distance - halfHeight) < snapTolerance;

        Vector2 velocity = rb.linearVelocity;
        velocity.x = forwardSpeed;

        // --- Vertical control ---
        if (isUpPressed)
        {
            if (hitUp.collider != null && hitUp.distance <= halfHeight + snapTolerance)
            {
                // Snap under ceiling
                transform.position = new Vector2(pos.x, hitUp.point.y - halfHeight);
                velocity.y = 0f;
            }
            else
            {
                velocity.y = upForce;
            }
        }
        else
        {
            if (hitDown.collider != null && hitDown.distance <= halfHeight + snapTolerance)
            {
                // Snap onto ground
                transform.position = new Vector2(pos.x, hitDown.point.y + halfHeight);
                velocity.y = 0f;
            }
            else
            {
                velocity.y = -fallSpeed;
            }
        }

        rb.linearVelocity = velocity;

        // --- Rotation ---
        float targetAngle = groundRotation;

        if (isAtCeiling)
        {
            targetAngle = groundRotation; // flat on ceiling
        }
        else if (isUpPressed)
        {
            targetAngle = tiltUpAngle;
        }
        else if (!isGrounded)
        {
            targetAngle = tiltDownAngle;
        }
        else
        {
            targetAngle = groundRotation; // flat on ground
        }

        float angle = Mathf.LerpAngle(transform.eulerAngles.z, targetAngle, Time.fixedDeltaTime * tiltSpeed);
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // --- Front collision check ---
        Vector2 boxCenter = (Vector2)transform.position + frontBoxOffset;
        Collider2D hitFront = Physics2D.OverlapBox(boxCenter, frontBoxSize, 0f, obstacleLayer);

        if (hitFront != null && !isEliminated)
        {
            Eliminate();
        }
    }
    private void Eliminate()
    {
        isEliminated = true;

        eliminationParticle.Play();
        spriteRend.enabled = false;
        trailRend.enabled = false;

        rb.linearVelocity = Vector2.zero;

        Debug.Log("Player eliminated!");
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector2 pos = transform.position;
        Gizmos.DrawLine(pos, pos + Vector2.down * rayDistance);
        Gizmos.DrawLine(pos, pos + Vector2.up * rayDistance);

        // Draw front overlap box
        Gizmos.color = Color.red;
        Vector2 boxCenter = (Vector2)transform.position + frontBoxOffset;
        Gizmos.DrawWireCube(boxCenter, frontBoxSize);
    }
}