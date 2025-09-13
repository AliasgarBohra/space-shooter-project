using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

    [SerializeField] private Transform topWall;
    [SerializeField] private Transform bottomWall;

    [Header("Follow Settings")]
    [Tooltip("How fast the camera interpolates to the target position.")]
    [SerializeField] private float smoothSpeed = 5f;

    [Tooltip("Vertical deadzone: camera will not move while player stays within this vertical distance from the camera.")]
    [SerializeField] private float yThreshold = 2f;

    private float initialZ;

    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
        initialZ = transform.position.z;

        FitCamera();
    }

    private void LateUpdate()
    {
        if (player == null) return;

        // --- X behaviour: follow player.x as before ---
        float targetX = player.position.x;

        // --- Y behaviour: only move when player is beyond threshold from camera ---
        float currentY = transform.position.y;
        float playerY = player.position.y;
        float deltaY = playerY - currentY;

        float desiredCamY = currentY; // default: don't move

        if (Mathf.Abs(deltaY) > yThreshold)
        {
            // Move camera so the player sits exactly at the threshold edge,
            // keeping the player inside the deadzone boundary.
            desiredCamY = playerY - Mathf.Sign(deltaY) * yThreshold;
        }

        // Clamp to walls if provided (works for orthographic camera)
        if (cam != null && topWall != null && bottomWall != null && cam.orthographic)
        {
            float halfHeight = cam.orthographicSize;
            float minY = bottomWall.position.y + halfHeight;
            float maxY = topWall.position.y - halfHeight;
            desiredCamY = Mathf.Clamp(desiredCamY, minY, maxY);
        }

        Vector3 targetPos = new Vector3(targetX, desiredCamY, initialZ);

        // Smoothly move camera
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothSpeed);
    }

    private void FitCamera()
    {
        if (topWall == null || bottomWall == null) return;

        if (cam == null || !cam.orthographic) return;

        float topY = topWall.position.y;
        float bottomY = bottomWall.position.y;

        cam.orthographicSize = (topY - bottomY) / 2f;
    }

    private void OnValidate()
    {
        if (yThreshold < 0f) yThreshold = 0f;
        if (smoothSpeed <= 0f) smoothSpeed = 1f;
    }
}
