using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class CameraFitSpriteFull : MonoBehaviour
{
    [Header("Target Sprite")]
    public SpriteRenderer targetSprite;

    [Header("Padding")]
    public float padding = 0.1f; // extra space around sprite

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        UpdateCameraSize();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        cam = GetComponent<Camera>();
        UpdateCameraSize();
    }
#endif

    private void Update()
    {
        if (Application.isPlaying)
        {
            UpdateCameraSize();
        }
    }

    private void UpdateCameraSize()
    {
        if (cam == null || targetSprite == null) return;

        Bounds bounds = targetSprite.bounds;

        // half-size of the sprite
        float halfHeight = bounds.size.y / 2f + padding;
        float halfWidth = bounds.size.x / 2f + padding;

        // convert horizontal size to vertical units
        float requiredOrthoSizeForWidth = halfWidth / cam.aspect;

        // orthographic size must fit both
        cam.orthographicSize = Mathf.Max(halfHeight, requiredOrthoSizeForWidth);

        // center camera on sprite
        cam.transform.position = new Vector3(bounds.center.x, bounds.center.y, cam.transform.position.z);
    }
}
