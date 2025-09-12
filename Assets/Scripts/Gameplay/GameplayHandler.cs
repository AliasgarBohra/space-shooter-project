using EasyUI.Toast;
using Fusion;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayHandler : NetworkBehaviour, IPlayerJoined, IPlayerLeft
{
    public static GameplayHandler Instance;

    [SerializeField] private GameObject playerPrefab;

    [Header("Config")]
    [SerializeField] private int gameDuration = 60;
    [SerializeField] private Transform singlePlayerSpawnPoint;
    [SerializeField] private EnemyWaveSpawner enemySpawner;

    [Header("UIs")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI codeText;
    [SerializeField] private GameObject shareableURLButton;

    [Header("Panels")]
    [SerializeField] private GameObject waitingPanel;
    [SerializeField] private GameObject spectatingPanel;
    [Space(10)]
    [SerializeField] private View eliminatedPanel;
    [SerializeField] private View gameEndPanel;

    private float timer;
    private bool isTimerEnded = false;
    private bool isGameStarted = false;
    private bool isGameHasEnded;
    private bool isSpawned;
    private bool safeLeave = false;

    private int localPlayerScore = 0;
    private int eliminatedPlayers = 0;

    public bool isGameEnded { get; private set; } = false;
    public bool isLocalPlayerEliminated { get; private set; }
    public int opponentScore { get; set; } = 0;

    [Networked] private TickTimer roundTimer { get; set; }

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
            Instantiate(playerPrefab, new Vector3(singlePlayerSpawnPoint.position.x, singlePlayerSpawnPoint.position.y, playerPrefab.transform.position.z),
                playerPrefab.transform.rotation);

            enemySpawner.StartWaves(gameDuration, Random.Range(int.MinValue, int.MaxValue));

            StartTimer();

            isGameEnded = false;
            codeText.transform.parent.gameObject.SetActive(false);
        }
        GameLife.Instance.isGameplayedFirstTime = true;
    }
    public override void Spawned()
    {
        if (Runner.ActivePlayers.Count() < 2)
        {
            waitingPanel.SetActive(true);
        }
        isSpawned = true;
        codeText.text = "CODE: " + Runner.SessionInfo.Name;

        shareableURLButton.SetActive(!string.IsNullOrEmpty(WebGLMatchBootstrap.Instance.shareableLink));
    }
    [Rpc]
    private void RPC_StartGame(int seed)
    {
        enemySpawner.StartWaves(gameDuration, seed);

        isGameEnded = false;
        isGameStarted = true;
    }

    #region Callbacks
    private void OnDestroy()
    {
        if (!safeLeave)
        {
            WebGLMatchBootstrap.Instance.OnMatchAbort_Report("Match Terminated!", "Something went wrong!");

            GoToHome();
        }
    }
    public void PlayerJoined(PlayerRef player)
    {
        if (Runner.ActivePlayers.Count() == 2)
        {
            if (!isGameStarted && Runner.IsSharedModeMasterClient)
            {
                int seed = Random.Range(int.MinValue, int.MaxValue);

                RPC_StartGame(seed);

                StartTimer();

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
    public void onLocalPlayerScored()
    {
        localPlayerScore++;
        scoreText.text = "SCORE: " + localPlayerScore;

        if (Player.Local != null)
        {
            Player.Local.AddScore(1);
        }
    }
    #endregion

    #region Timer Handler
    private void Update()
    {
        if (isGameEnded)
            return;

        if (!isTimerEnded)
        {
            if (GameManager.Instance.isMultiplayer)
            {
                if (isSpawned)
                {
                    float timeLeft = 0f;
                    if (roundTimer.IsRunning)
                    {
                        timeLeft = roundTimer.RemainingTime(Runner).Value;
                        if (timeLeft < 0f)
                            timeLeft = 0f;

                        UpdateTimerUI(timeLeft);
                    }
                }
            }
            else
            {
                if (timer > 0)
                {
                    timer -= Time.deltaTime;
                }
                else
                {
                    timer = 0;
                    TimerEnd();
                    GameEnd();
                }
                UpdateTimerUI(timer);
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (isTimerEnded || !Runner.IsSharedModeMasterClient)
            return;

        if (roundTimer.Expired(Runner))
        {
            // Timer has ended according to the server / simulation
            isTimerEnded = true;
            TimerEnd();
            GameEndRpc();
        }
    }

    private void UpdateTimerUI(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    private void StartTimer()
    {
        if (GameManager.Instance.isMultiplayer)
        {
            roundTimer = TickTimer.CreateFromSeconds(Runner, gameDuration);
        }
        else
        {
            timer = gameDuration;
        }
        isTimerEnded = false;
    }
    private void TimerEnd()
    {
        isTimerEnded = true;
    }
    #endregion

    #region Buttons
    public void CopyCode()
    {
        TextEditor te = new TextEditor();
        te.text = Runner.SessionInfo.Name;
        te.SelectAll();
        te.Copy();

        Toast.Show("Copied");
    }
    public void CopyURL()
    {
        TextEditor te = new TextEditor();
        te.text = WebGLMatchBootstrap.Instance.shareableLink;
        te.SelectAll();
        te.Copy();

        Toast.Show("Copied URL");
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
    #endregion

    #region Game End Handler
    public void IncreamentEliminationCount()
    {
        eliminatedPlayers++;
        if (eliminatedPlayers >= 2)
        {
            GameEnd();
        }
    }
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
            UpdateTimerUI(0);
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
        if (isGameHasEnded)
            return;

        eliminatedPanel.ForceHide();

        gameEndPanel.Show();
        isGameEnded = true;

        UpdateTimerUI(0);

        isGameHasEnded = true;

        int winStat = 3;

        if (localPlayerScore > opponentScore)
        {
            winStat = 1;
        }
        else if (localPlayerScore < opponentScore)
        {
            winStat = 2;
        }
        WebGLMatchBootstrap.Instance.OnMatchEnd_ReportWin(localPlayerScore, winStat);
    }
    #endregion
}