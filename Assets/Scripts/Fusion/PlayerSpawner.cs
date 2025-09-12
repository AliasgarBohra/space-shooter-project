using Fusion;
using UnityEngine;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    [SerializeField] private GameObject PlayerPrefab;

    public void PlayerJoined(PlayerRef player)
    {
        if (player == Runner.LocalPlayer)
        {
            NetworkObject playerObject = Runner.Spawn(
                PlayerPrefab,
                new Vector3(-11.25f, Random.Range(-3, 3), 0),
                PlayerPrefab.transform.rotation,
                player
            );
        }
    }
}