using EasyUI.Toast;
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

public class WebGLMatchBootstrap : MonoBehaviour, INetworkRunnerCallbacks
{
    public static WebGLMatchBootstrap Instance;

    [SerializeField] private GameObject runnerPrefab;
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TMP_InputField codeField;

    private NetworkRunner runner;

    private string matchId;
    private string playerId;
    private string opponentId;

    private const string BASE_URL = "https://aliasgarbohra.github.io/space-shooter/";

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void PostMatchMessageJS(string json);

    [DllImport("__Internal")]
    private static extern void SetBrowserUrl(string url);
#endif

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        TryHandleUrlParamsAndStart();
    }

    private async void TryHandleUrlParamsAndStart()
    {
        ParseUrlParams();

        if (string.IsNullOrEmpty(matchId))
        {
            return;
        }

        if (codeField != null) codeField.text = matchId;

        loadingPanel?.SetActive(true);
        runner = Instantiate(runnerPrefab).GetComponent<NetworkRunner>();
        runner.AddCallbacks(this);

        GameManager.Instance.isMultiplayer = true;

        bool joined = await TryJoinSession(matchId);
        if (!joined)
        {
            await CreateSession(matchId);
        }
    }

    private void ParseUrlParams()
    {
        string url = Application.absoluteURL;
        if (string.IsNullOrEmpty(url))
        {
            return;
        }

        int q = url.IndexOf('?');
        if (q < 0) return;

        string qStr = url.Substring(q + 1);
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var part in qStr.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var kv = part.Split(new[] { '=' }, 2);
            if (kv.Length == 0) continue;
            string k = Uri.UnescapeDataString(kv[0]);
            string v = kv.Length > 1 ? Uri.UnescapeDataString(kv[1]) : "";
            dict[k] = v;
        }

        dict.TryGetValue("matchId", out matchId);
        dict.TryGetValue("playerId", out playerId);
        dict.TryGetValue("opponentId", out opponentId);

        if (!string.IsNullOrEmpty(playerId)) GameManager.Instance.playerId = playerId;
        if (!string.IsNullOrEmpty(opponentId)) GameManager.Instance.opponentId = opponentId;
        if (!string.IsNullOrEmpty(matchId)) GameManager.Instance.matchId = matchId;

        Debug.Log($"Parsed URL params: matchId={matchId}, playerId={playerId}, opponentId={opponentId}");
    }
    public string shareableLink;
    public void GenerateAndSetShareLink(string matchId, string playerId, string opponentId)
    {
        shareableLink = $"{BASE_URL}?matchId={matchId}&playerId={playerId}&opponentId={opponentId}";
        //Debug.Log("Share this match link: " + url);

#if UNITY_WEBGL && !UNITY_EDITOR
    SetBrowserUrl(shareableLink);
#endif
    }
    public void ResetURL()
    {
        shareableLink = "";

        matchId = "";
        playerId = "";
        opponentId = "";

        GameManager.Instance.matchId = "";
        GameManager.Instance.playerId = "";
        GameManager.Instance.opponentId = "";

        string url = BASE_URL;

        //Debug.Log("Share this match link: " + url);

#if UNITY_WEBGL && !UNITY_EDITOR
    SetBrowserUrl(url);
#endif
    }

    private async Task<bool> TryJoinSession(string sessionName)
    {
        var startArgs = new StartGameArgs
        {
            GameMode = GameMode.Shared,
            SessionName = sessionName,
            SceneManager = runner.GetComponent<NetworkSceneManagerDefault>(),
            EnableClientSessionCreation = false,
            PlayerCount = 2,
        };

        try
        {
            var result = await runner.StartGame(startArgs);
            if (result.Ok)
            {
                Debug.Log($"Joined session: {sessionName}");
                loadingPanel?.SetActive(false);
                return true;
            }
            else
            {
                Debug.LogWarning($"Join failed ({result.ShutdownReason})");
                loadingPanel?.SetActive(false);
                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Join threw: {ex}");
            loadingPanel?.SetActive(false);
            return false;
        }
    }

    private async Task CreateSession(string sessionName)
    {
        var startArgs = new StartGameArgs
        {
            GameMode = GameMode.Shared,
            SessionName = sessionName,
            SceneManager = runner.GetComponent<NetworkSceneManagerDefault>(),
            PlayerCount = 2,
        };

        try
        {
            var result = await runner.StartGame(startArgs);
            if (result.Ok)
            {
                Debug.Log($"Created session: {sessionName}");
                GenerateAndSetShareLink(sessionName, "1", "2");
            }
            else
            {
                Debug.LogError($"Failed to create session: {result.ShutdownReason}");
                Toast.Show(result.ShutdownReason.ToString());
                loadingPanel?.SetActive(false);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"CreateSession threw: {ex}");
            loadingPanel?.SetActive(false);
        }
    }

    public void OnMatchEnd_ReportWin(int score, int winStat)
    {
        string stat = "TIE";

        if (winStat == 1)
        {
            stat = "WON";
        }
        else if (winStat == 2)
        {
            stat = "LOST";
        }

        var envelope = new MatchResultEnvelope
        {
            type = "match_result",
            payload = new MatchResultPayload
            {
                matchId = GameManager.Instance.matchId,
                playerId = GameManager.Instance.playerId,
                opponentId = GameManager.Instance.opponentId,
                outcome = stat,
                score = score
            }
        };
        SendMessageToParent(envelope);
    }

    public void OnMatchAbort_Report(string message, string error = null, string errorCode = null)
    {
        var envelope = new MatchAbortEnvelope
        {
            type = "match_abort",
            payload = new MatchAbortPayload
            {
                message = message,
                error = error,
                errorCode = errorCode
            }
        };
        SendMessageToParent(envelope);
    }

    private void SendMessageToParent(object envelope)
    {
        string json = JsonUtility.ToJson(envelope);
#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            PostMatchMessageJS(json);
        }
        catch (Exception)
        {
            // fallback
            Application.ExternalEval($"window.parent.postMessage({json}, '*');");
        }
#else
        Debug.Log("Would post to parent: " + json);
#endif
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        throw new NotImplementedException();
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        throw new NotImplementedException();
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        throw new NotImplementedException();
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        throw new NotImplementedException();
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        throw new NotImplementedException();
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        throw new NotImplementedException();
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        throw new NotImplementedException();
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        throw new NotImplementedException();
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        throw new NotImplementedException();
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        throw new NotImplementedException();
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        throw new NotImplementedException();
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        throw new NotImplementedException();
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        throw new NotImplementedException();
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        throw new NotImplementedException();
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        throw new NotImplementedException();
    }

    [Serializable]
    private class MatchResultPayload
    {
        public string matchId;
        public string playerId;
        public string opponentId;
        public string outcome;
        public int score;
    }

    [Serializable]
    private class MatchResultEnvelope
    {
        public string type;
        public MatchResultPayload payload;
    }

    [Serializable]
    private class MatchAbortPayload
    {
        public string message;
        public string error;
        public string errorCode;
    }

    [Serializable]
    private class MatchAbortEnvelope
    {
        public string type;
        public MatchAbortPayload payload;
    }
}