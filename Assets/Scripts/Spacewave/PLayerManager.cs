using Fusion;
using TMPro;
using UnityEngine;

public class PLayerManager : NetworkBehaviour
{
    public static PLayerManager Local;

    [SerializeField] private GameObject shipCanvas;
    [SerializeField] private TextMeshProUGUI playerNameText;

    private void Start()
    {
        if (GameManager.Instance.isMultiplayer && !HasInputAuthority)
        {
            Color c = playerNameText.color;
            c.a = 0.5f;
            playerNameText.color = c;

            shipCanvas.SetActive(true);
            playerNameText.text = "Player " + Object.InputAuthority.PlayerId;

            gameObject.tag = "OtherPlayer";
        }
        else
        {
            Local = this;
            gameObject.tag = "Player";
        }
    }
    private void Update()
    {
        if (shipCanvas.activeSelf)
            shipCanvas.transform.rotation = Quaternion.Euler(0, 0, -90);
    }
}