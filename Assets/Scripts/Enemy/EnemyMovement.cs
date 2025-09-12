using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Vector2 moveDirection = new Vector2(0, -1);
    [SerializeField] private float moveSpeed = 2f;

    private Vector2 moveDir = Vector2.down;
    private float speed = 2f;

    private void Start()
    {
        SetMoveDirection(moveDirection, moveSpeed);
    }
    private void SetMoveDirection(Vector2 dir, float moveSpeed)
    {
        moveDir = dir.normalized;
        speed = moveSpeed;
    }
    private void Update()
    {
        transform.Translate(moveDir * speed * Time.deltaTime, Space.World);

        if (transform.position.y < -20f)
        {
            Destroy(gameObject);
        }
    }
}