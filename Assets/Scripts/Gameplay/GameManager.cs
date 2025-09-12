using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public bool isMultiplayer = false;

    public string matchId;
    public string playerId;
    public string opponentId;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject); // destroy the old instance
        }

        Instance = this;                  // assign the new instance
        DontDestroyOnLoad(gameObject);    // keep it alive across scenes
    }

    public void StartSinglePlayer()
    {
        isMultiplayer = false;
        SceneManager.LoadScene("SpaceArena");
    }
    public void StartMultiplayer()
    {
        isMultiplayer = true;
        FusionLauncher.Instance.StartMultiplayer();
    }
}