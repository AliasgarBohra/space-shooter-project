using UnityEngine;

public class GameLife : MonoBehaviour
{
    public static GameLife Instance;

    public bool isGameplayedFirstTime { get; set; } = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}