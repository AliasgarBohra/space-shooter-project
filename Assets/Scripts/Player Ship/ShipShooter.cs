using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooter : NetworkBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference shootAction;
    [SerializeField] private Animator anim;
    [SerializeField] private Player playerObject;

    [Header("Shooting Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float cooldown = 0.5f;

    private float lastShootTime;
    private bool isHit = false;

    private void OnEnable()
    {
        if (shootAction != null)
            shootAction.action.Enable();
    }

    private void OnDisable()
    {
        if (shootAction != null)
            shootAction.action.Disable();
    }

    private void Update()
    {
        if (shootAction != null && shootAction.action.WasPerformedThisFrame())
        {
            if ((GameManager.Instance.isMultiplayer && !HasInputAuthority) || GameplayHandler.Instance.isLocalPlayerEliminated)
                return;

            TryShoot();
        }
    }

    private void TryShoot()
    {
        if (Time.time < lastShootTime + cooldown) return;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        Quaternion spawnRot = transform.rotation;

        if (GameManager.Instance.isMultiplayer)
        {
            RPC_SpawnProjectile(transform.up * projectileSpeed, spawnPos, (byte)Object.InputAuthority.PlayerId);
        }
        else
        {
            SpawnProjectile(transform.up * projectileSpeed, spawnPos, 0);
        }

        lastShootTime = Time.time;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isHit)
            return;

        if (other.CompareTag("EnemyShip"))
        {
            if (!GameManager.Instance.isMultiplayer)
            {
                GameplayHandler.Instance.OnLocalPlayerDied();

                anim.SetTrigger("Blast2");

                Destroy(gameObject, getClipLength("BlastAnimation2"));

                other.GetComponent<EnemyShip>().DestroyShip();

                isHit = true;
            }
            else if (HasInputAuthority)
            {
                GameplayHandler.Instance.OnLocalPlayerDied();

                RPC_DestroySelf();
                other.GetComponent<EnemyShip>().DestroyShip();
            }
        }
    }

    #region Projectile Sync
    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.All)]
    public void RPC_SpawnProjectile(Vector2 dirction, Vector2 spawnPos, byte playerId)
    {
        SpawnProjectile(dirction, spawnPos, playerId);
    }
    private void SpawnProjectile(Vector2 dirction, Vector2 spawnPos, byte id)
    {
        GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        projectile.GetComponent<Projectile>().ownerPlayerId = id;

        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = transform.up * projectileSpeed;
        }
    }

    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.All)]
    private void RPC_DestroySelf()
    {
        GameplayHandler.Instance.IncreamentEliminationCount();

        anim.SetTrigger("Blast2");

        playerObject.shipCanvas.SetActive(false);

        if (GameManager.Instance.isMultiplayer)
        {
            Invoke(nameof(DestroyShip), getClipLength("BlastAnimation2"));
        }

        isHit = true;
    }
    private void DestroyShip()
    {
        GetComponent<SpriteRenderer>().enabled = false;
    }
    public float getClipLength(string key)
    {
        foreach (var clip in anim.runtimeAnimatorController.animationClips)
        {
            if (clip.name.Equals(key))
            {
                return clip.length;
            }
        }
        return -1;
    }
    #endregion
}