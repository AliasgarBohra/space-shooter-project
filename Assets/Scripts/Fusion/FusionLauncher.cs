using EasyUI.Toast;
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FusionLauncher : MonoBehaviour, INetworkRunnerCallbacks
{
    public static FusionLauncher Instance;

    [SerializeField] private NetworkRunner runnerPrefab;
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TMP_InputField codeField;

    public NetworkRunner runner { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    #region Game Creation
    public async void StartMultiplayer()
    {
        loadingPanel.SetActive(true);

        runner = Instantiate(runnerPrefab);
        runner.AddCallbacks(this);

        await StartGame();
    }
    private async Task StartGame()
    {
        GameManager.Instance.isMultiplayer = true;

        SceneRef sceneRef = SceneRef.FromIndex(1);
        string matchCode = UnityEngine.Random.Range(100, 999) + "" + UnityEngine.Random.Range(10, 99);

        var startGameArgs = new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = matchCode,
            SceneManager = runner.GetComponent<NetworkSceneManagerDefault>(),
            PlayerCount = 2,
            Scene = sceneRef,
        };

        try
        {
            var result = await runner.StartGame(startGameArgs);
            if (result.Ok)
            {
                //Debug.Log("Game started successfully.");
                WebGLMatchBootstrap.Instance.GenerateAndSetShareLink(matchCode, "1", "2");
            }
            else
            {
                Debug.LogError($"Failed to start game: {result.ShutdownReason}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"StartGame threw an exception: {ex}");
            loadingPanel.SetActive(false);
        }
    }
    #endregion

    #region Game Joining
    public async void JoinWithCode()
    {
        if (string.IsNullOrEmpty(codeField.text))
        {
            Toast.Show("Enter Code!");
            return;
        }
        GameManager.Instance.isMultiplayer = true;

        loadingPanel.SetActive(true);

        runner = Instantiate(runnerPrefab);
        runner.AddCallbacks(this);

        await JoinWithMatchCode();
    }
    private async Task JoinWithMatchCode()
    {
        loadingPanel.SetActive(true);

        var startGameArgs = new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = codeField.text,
            SceneManager = runner.GetComponent<NetworkSceneManagerDefault>(),
            EnableClientSessionCreation = false,
            PlayerCount = 2,
        };

        try
        {
            var result = await runner.StartGame(startGameArgs);
            if (result.Ok)
            {
                Debug.Log("Joined existing session successfully.");
                WebGLMatchBootstrap.Instance.GenerateAndSetShareLink(codeField.text, "2", "1");
                // proceed
            }
            else
            {
                Toast.Show(result.ShutdownReason.ToString());
                loadingPanel.SetActive(false);
                Debug.LogError($"Failed to join game: {result.ShutdownReason}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"StartGame threw an exception: {ex}");
            loadingPanel.SetActive(false);
        }
    }
    #endregion

    #region Callbacks
    public void OnSceneLoadDone(NetworkRunner runner)
    {
        Debug.Log("Scene load finished, hiding loading panel.");
        loadingPanel.SetActive(false);
    }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        SceneManager.LoadScene("Menu");
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {

    }
    #endregion
}