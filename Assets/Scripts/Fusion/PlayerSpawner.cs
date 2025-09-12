using Fusion;
using UnityEngine;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined, IPlayerLeft
{
    [SerializeField] private GameObject PlayerPrefab;

    // [Networked] private NetworkDictionary<PlayerRef, Player> players => default;

    public void PlayerJoined(PlayerRef player)
    {
        if (player == Runner.LocalPlayer)
        {
            // Spawn the player object
            NetworkObject playerObject = Runner.Spawn(
                PlayerPrefab,
                new Vector3(-11.25f, Random.Range(-3, 3), 0),
                PlayerPrefab.transform.rotation,
                player
            );
            //players.Add(player, playerObject.GetBehaviour<Player>());
        }
    }

    public void PlayerLeft(PlayerRef player)
    {
        /*        if (players.TryGet(player, out Player playerObject))
                {
                    players.Remove(player);
                }*/
    }
}