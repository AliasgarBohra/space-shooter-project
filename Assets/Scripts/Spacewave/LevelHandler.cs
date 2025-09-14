using EasyUI.Toast;
using Fusion;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelHandler : NetworkBehaviour, IPlayerJoined, IPlayerLeft
{
    public static LevelHandler Instance;

    [SerializeField] private GameObject playerPrefab;

    [Header("UIs")]
    [SerializeField] private TextMeshProUGUI codeText;
    [Space(10)]
    [SerializeField] private GameObject waitingPanel;
    [SerializeField] private GameObject spectatingPanel;
    [Space(10)]
    [SerializeField] private View eliminatedPanel;
    [SerializeField] private View gameEndPanel;
    [SerializeField] private CameraFollow camFollow;

    public bool isGameStarted { get; private set; } = false;
    private bool isGameEnded;
    private bool safeLeave = false;
    private bool isLocalPlayerWon = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    private void Start()
    {
        if (!GameManager.Instance.isMultiplayer)
        {
            Instantiate(playerPrefab, new Vector3(-33.32f, Random.Range(-4, 4), 0),
                playerPrefab.transform.rotation);

            isGameEnded = false;
            isGameStarted = true;

            codeText.transform.parent.gameObject.SetActive(false);
        }
        GameLife.Instance.isGameplayedFirstTime = true;
    }
    private void OnDestroy()
    {
        if (!safeLeave)
        {
            WebGLMatchBootstrap.Instance.OnMatchAbort_Report("Match Terminated!", "Something went wrong!");

            Toast.Show("Match Terminated!\nOpponent left! Or Something went wrong!");

            GameEnd();
        }
    }

    [Rpc]
    private void RPC_StartGame()
    {
        isGameEnded = false;
        isGameStarted = true;
    }

    public void GoToHome()
    {
        safeLeave = true;

        try
        {
            WebGLMatchBootstrap.Instance.ResetURL();

            if (GameManager.Instance.isMultiplayer && Runner != null)
            {
                Runner.Shutdown();
            }
            if (FusionLauncher.Instance != null)
                Destroy(FusionLauncher.Instance.gameObject);

            GameManager.Instance = null;
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }

        SceneManager.LoadScene("Menu");
    }

    #region Callbacks
    public override void Spawned()
    {
        if (Runner.ActivePlayers.Count() < 2)
        {
            waitingPanel.SetActive(true);
        }
        codeText.text = "CODE: " + Runner.SessionInfo.Name;
    }
    public void PlayerJoined(PlayerRef player)
    {
        if (Runner.ActivePlayers.Count() == 2)
        {
            if (!isGameStarted && Runner.IsSharedModeMasterClient)
            {
                Invoke(nameof(RPC_StartGame), 1);
            }
            waitingPanel.SetActive(false);
        }
    }
    public void PlayerLeft(PlayerRef player)
    {
        if (player != Runner.LocalPlayer)
        {
            WebGLMatchBootstrap.Instance.OnMatchAbort_Report("Match Terminated!", "Opponent Left!");
            GameEnd();
        }
    }
    #endregion

    #region Game End Handler
    public void OnLocalPlayerWon()
    {
        isLocalPlayerWon = true;

        if (!GameManager.Instance.isMultiplayer)
        {
            GameEnd();
        }
        else
        {
            GameEndRpc();
        }
    }
    public void OnLocalPlayerDied()
    {
        StartCoroutine(OnLocalPlayerDiedCoroutine());
    }
    private IEnumerator OnLocalPlayerDiedCoroutine()
    {
        if (GameManager.Instance.isMultiplayer)
            spectatingPanel.SetActive(true);

        yield return new WaitForSecondsRealtime(1);

        eliminatedPanel.Show();

        yield return new WaitForSecondsRealtime(2);

        eliminatedPanel.Hide();

        if (!GameManager.Instance.isMultiplayer)
            GameEnd();
        else
        {
            GameObject otherPlayerObject = GameObject.FindGameObjectWithTag("OtherPlayer");

            if (otherPlayerObject != null)
            {
                camFollow.player = otherPlayerObject.transform;
            }
            else
            {
                GameEndRpc();
            }
        }
    }
    [Rpc]
    private void GameEndRpc()
    {
        GameEnd();
    }

    private void GameEnd()
    {
        if (isGameEnded)
            return;

        eliminatedPanel.ForceHide();

        gameEndPanel.Show();
        isGameEnded = true;

        WebGLMatchBootstrap.Instance.OnMatchEnd_ReportWin(isLocalPlayerWon);
    }
    #endregion
}