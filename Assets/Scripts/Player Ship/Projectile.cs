using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float lifetime = 3f;
    public int ownerPlayerId { get; set; }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("EnemyShip"))
        {
            if (GameManager.Instance.isMultiplayer)
            {
                if (ownerPlayerId == FusionLauncher.Instance.runner.LocalPlayer.PlayerId)
                {
                    GameplayHandler.Instance.onLocalPlayerScored();
                }
            }
            else
            {
                GameplayHandler.Instance.onLocalPlayerScored();
            }

            other.GetComponent<EnemyShip>().DestroyShip();

            Destroy(gameObject);
        }
    }
}