using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

    [SerializeField] private Transform topWall;
    [SerializeField] private Transform bottomWall;

    [Header("Follow Settings")]
    [SerializeField] private float smoothSpeed = 5f;

    private float initialY;
    private float initialZ;

    private void Start()
    {
        initialY = transform.position.y;
        initialZ = transform.position.z;

        FitCamera();
    }
    private void LateUpdate()
    {
        if (player == null) return;

        float targetX = player.position.x;
        Vector3 targetPos = new Vector3(targetX, initialY, initialZ);

        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothSpeed);
    }
    private void FitCamera()
    {
        if (topWall == null || bottomWall == null) return;

        Camera cam = Camera.main;
        if (cam == null || !cam.orthographic) return;

        float topY = topWall.position.y;
        float bottomY = bottomWall.position.y;

        cam.orthographicSize = (topY - bottomY) / 2f;
    }
}