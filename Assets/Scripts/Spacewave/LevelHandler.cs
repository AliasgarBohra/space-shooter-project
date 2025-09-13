using Fusion;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelHandler : NetworkBehaviour
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

    private bool isGameStarted = false;
    private bool isGameEnded;
    private bool isSpawned;
    private bool safeLeave = false;
    private bool isLocalPlayerEliminated;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    [Rpc]
    private void RPC_StartGame()
    {
        isGameEnded = false;
        isGameStarted = true;
    }

    #region Callbacks
    public void PlayerJoined(PlayerRef player)
    {
        if (Runner.ActivePlayers.Count() == 2)
        {
            if (!isGameStarted && Runner.IsSharedModeMasterClient)
            {
                RPC_StartGame();

                isGameEnded = false;
                isGameStarted = true;
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
    public void OnLocalPlayerDied()
    {
        StartCoroutine(OnLocalPlayerDiedCoroutine());
    }
    private IEnumerator OnLocalPlayerDiedCoroutine()
    {
        spectatingPanel.SetActive(true);
        isLocalPlayerEliminated = true;

        if (!GameManager.Instance.isMultiplayer)
        {
            isGameEnded = true;
        }
        yield return new WaitForSecondsRealtime(1);

        eliminatedPanel.Show();

        yield return new WaitForSecondsRealtime(2);

        eliminatedPanel.Hide();

        if (!GameManager.Instance.isMultiplayer)
            GameEnd();
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

        /*        int winStat = 3;

                if (localPlayerScore > opponentScore)
                {
                    winStat = 1;
                }
                else if (localPlayerScore < opponentScore)
                {
                    winStat = 2;
                }
                WebGLMatchBootstrap.Instance.OnMatchEnd_ReportWin(localPlayerScore, winStat);*/
    }
    #endregion

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
}