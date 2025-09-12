using Fusion;
using TMPro;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public static Player Local;

    public GameObject shipCanvas;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI playerScoreText;

    [Networked, OnChangedRender(nameof(OnScoreChanged))] public int Score { get; set; }

    private void OnScoreChanged()
    {
        playerScoreText.text = "Score: " + Score;
    }

    // Expose a controlled setter
    public void AddScore(int amount)
    {
        Score += amount;

        if (!HasInputAuthority)
        {
            if (GameplayHandler.Instance != null)
                GameplayHandler.Instance.opponentScore = Score;
        }
    }

    private void Start()
    {
        if (GameManager.Instance.isMultiplayer && !HasInputAuthority)
        {
            shipCanvas.SetActive(true);
            playerNameText.text = "Player " + Object.InputAuthority.PlayerId;
        }
    }
    private void Update()
    {
        if (shipCanvas.activeSelf)
            shipCanvas.transform.rotation = Quaternion.Euler(0, 0, -90);
    }
    public override void Spawned()
    {
        if (HasInputAuthority)
        {
            Local = this;
        }
    }
}