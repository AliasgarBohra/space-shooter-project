using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpaceShipMotion : NetworkBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float stopDist = 1f;

    private float forwardInput;
    private Vector3 mousePos;
    private Camera mainCam;

    private void Awake()
    {
        mainCam = Camera.main;
    }
    private void OnEnable()
    {
        moveAction?.action?.Enable();
    }
    private void OnDisable()
    {
        moveAction?.action?.Disable();
    }
    private void FixedUpdate()
    {
        if (GameManager.Instance.isMultiplayer) return;

        float dist = Vector3.Distance(transform.position, mousePos);

        if (dist > stopDist)
        {
            Vector3 forwardDir = transform.up;
            transform.position += forwardDir * forwardInput * moveSpeed * Time.fixedDeltaTime;
        }
    }
    private void Update()
    {
        Vector2 rawInput = moveAction != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;
        forwardInput = Mathf.Clamp01(rawInput.y);

        mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        if (!GameManager.Instance.isMultiplayer)
        {
            Vector3 dir = mousePos - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!GameManager.Instance.isMultiplayer) return;

        if (Object.HasInputAuthority)
        {
            Vector3 dir = mousePos - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            float dist = Vector3.Distance(transform.position, mousePos);

            if (dist > stopDist)
            {
                Vector3 forwardDir = transform.up;
                transform.position += forwardDir * forwardInput * moveSpeed * Runner.DeltaTime;
            }
        }
    }
}