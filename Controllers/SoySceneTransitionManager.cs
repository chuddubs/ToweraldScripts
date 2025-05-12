using UnityEngine;

public class SoySceneTransitionManager : MonoBehaviour
{
    public static SoySceneTransitionManager Instance;
    public int lastScore = 0;
    public bool cameFromGameOver = false;
    public bool cameFromWin = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
