using DG.Tweening;
using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : NetworkBehaviour
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

    private bool isUpPressed;
    private bool isGrounded;
    private bool isAtCeiling;
    private bool isEliminated = false;

    private bool isMultiplayer = false;

    private void Awake()
    {
        if (trailRend == null)
            trailRend = GetComponent<TrailRenderer>();
    }

    private void OnEnable()
    {
        if (moveUpAction != null)
        {
            moveUpAction.action.performed += OnUpPressed;
            moveUpAction.action.canceled += OnUpReleased;
        }
    }

    private void OnDisable()
    {
        if (moveUpAction != null)
        {
            moveUpAction.action.performed -= OnUpPressed;
            moveUpAction.action.canceled -= OnUpReleased;
        }
    }

    public override void Spawned()
    {
        if (HasInputAuthority)
        {
            if (trailRend != null) trailRend.enabled = true;
        }
        else
        {
            if (trailRend != null) trailRend.enabled = false;

            if (spriteRend != null)
            {
                Color c = spriteRend.color;
                c.a = 0.5f;
                spriteRend.color = c;
            }
        }

        if (GameManager.Instance != null)
            isMultiplayer = GameManager.Instance.isMultiplayer;
    }

    #region Input
    private void OnUpPressed(InputAction.CallbackContext ctx) => isUpPressed = true;
    private void OnUpReleased(InputAction.CallbackContext ctx) => isUpPressed = false;
    #endregion

    #region Movement
    private void Update()
    {
        if (isEliminated) return;

        float t = (Mathf.Sin(Time.time * pulseSpeed * Mathf.PI * 2f) + 1f) / 2f;
        float width = Mathf.Lerp(minWidth, maxWidth, t);

        if (trailRend != null)
        {
            trailRend.startWidth = width;
            trailRend.endWidth = width * 0.5f;
        }
    }

    private void FixedUpdate()
    {
        if (isMultiplayer)
            return;

        SimulateMovement(Time.fixedDeltaTime);
    }

    public override void FixedUpdateNetwork()
    {
        if (!isMultiplayer)
            return;

        if (isEliminated)
            return;

        if (!(HasInputAuthority || Object.HasStateAuthority))
            return;

        float tickDelta = Runner.DeltaTime;
        SimulateMovement(tickDelta, useRunnerDelta: true);
    }

    private void SimulateMovement(float deltaTime, bool useRunnerDelta = false)
    {
        if (isEliminated) return;

        Vector2 pos = transform.position;

        RaycastHit2D hitDown = Physics2D.Raycast(pos, Vector2.down, rayDistance, groundLayer);
        RaycastHit2D hitUp = Physics2D.Raycast(pos, Vector2.up, rayDistance, groundLayer);

        isGrounded = hitDown.collider != null && Mathf.Abs(hitDown.distance - halfHeight) < snapTolerance;
        isAtCeiling = hitUp.collider != null && Mathf.Abs(hitUp.distance - halfHeight) < snapTolerance;

        if (LevelHandler.Instance != null && LevelHandler.Instance.isGameStarted)
        {
            float vx = forwardSpeed;
            float vy;

            if (isUpPressed)
            {
                if (hitUp.collider != null && hitUp.distance <= halfHeight + snapTolerance)
                {
                    pos.y = hitUp.point.y - halfHeight;
                    vy = 0f;
                }
                else
                {
                    vy = upForce;
                }
            }
            else
            {
                if (hitDown.collider != null && hitDown.distance <= halfHeight + snapTolerance)
                {
                    pos.y = hitDown.point.y + halfHeight;
                    vy = 0f;
                }
                else
                {
                    vy = -fallSpeed;
                }
            }

            pos.x += vx * deltaTime;
            pos.y += vy * deltaTime;

            transform.position = new Vector3(pos.x, pos.y, transform.position.z);
        }

        float targetAngle = groundRotation;

        if (isAtCeiling)
            targetAngle = groundRotation;
        else if (isUpPressed)
            targetAngle = tiltUpAngle;
        else if (!isGrounded)
            targetAngle = tiltDownAngle;
        else
            targetAngle = groundRotation;

        float t = deltaTime;
        float angle = Mathf.LerpAngle(transform.eulerAngles.z, targetAngle, t * tiltSpeed);
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // front collision detect
        if (LevelHandler.Instance.isGameStarted)
        {
            Vector2 boxCenter = (Vector2)transform.position + frontBoxOffset;
            Collider2D hitFront = Physics2D.OverlapBox(boxCenter, frontBoxSize, 0f, obstacleLayer);

            if (hitFront != null && !isEliminated)
            {
                Debug.Log("Player eliminated!");

                if (LevelHandler.Instance != null)
                    LevelHandler.Instance.OnLocalPlayerDied();

                if (GameManager.Instance.isMultiplayer)
                {
                    RPC_Eliminate();
                }
                else
                {
                    EliminatePlayer();
                }
            }
        }
    }
    #endregion

    #region Elimination
    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_Eliminate()
    {
        EliminatePlayer();
    }
    private void EliminatePlayer()
    {
        if (isEliminated) return;

        isEliminated = true;

        if (eliminationParticle != null)
            eliminationParticle.Play();

        if (spriteRend != null)
            spriteRend.enabled = false;

        if (trailRend != null)
            trailRend.enabled = false;

        if (HasStateAuthority)
        {
            Invoke(nameof(DestroySelf), eliminationParticle.main.duration);
        }
    }
    #endregion

    #region Winning
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("WinTrigger"))
        {
            if (trailRend != null) trailRend.enabled = false;

            if (LevelHandler.Instance != null)
                LevelHandler.Instance.OnLocalPlayerWon();

            if (GameManager.Instance.isMultiplayer)
            {
                RPC_Win();
            }
            else
            {
                transform.DOScale(0, 0.1f).SetEase(Ease.InOutBounce);
                Destroy(gameObject, 0.1f);
            }
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_Win()
    {
        transform.DOScale(0, 0.1f).SetEase(Ease.InOutBounce);

        if (HasStateAuthority)
        {
            Invoke(nameof(DestroySelf), 0.1f);
        }
    }
    private void DestroySelf()
    {
        Runner.Despawn(Object);
    }
    #endregion

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector2 pos = transform.position;
        Gizmos.DrawLine(pos, pos + Vector2.down * rayDistance);
        Gizmos.DrawLine(pos, pos + Vector2.up * rayDistance);

        Gizmos.color = Color.red;
        Vector2 boxCenter = (Vector2)transform.position + frontBoxOffset;
        Gizmos.DrawWireCube(boxCenter, frontBoxSize);
    }
}