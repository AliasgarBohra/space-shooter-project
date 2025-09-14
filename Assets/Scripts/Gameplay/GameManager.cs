using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public bool isMultiplayer { get; set; } = false;

    public string matchId { get; set; }
    public string playerId { get; set; }
    public string opponentId { get; set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartSinglePlayer()
    {
        isMultiplayer = false;
        SceneManager.LoadScene(1);
        WebGLMatchBootstrap.Instance.ResetURL();
    }
}