using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoyUI_MenuController : MonoBehaviour
{
    public GameObject homePanel;
    public GameObject gameOverPanel;
    public GameObject loseImage;
    public GameObject winImage;
    public GameObject helpPanel;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI scoreTextInline;
    public GameObject musicDisabledImg;
    public GameObject leftieToggle;

    public void Start()
    {
        SoySceneTransitionManager stm = SoySceneTransitionManager.Instance;
        if (stm.cameFromGameOver)
            ShowGameOver(stm.lastScore, false);
        else if (stm.cameFromWin)
            ShowGameOver(stm.lastScore, true);
        else
            homePanel.SetActive(true);
    }

    public void ShowHelp()
    {
        helpPanel.SetActive(true);
    }

    public void PlayGame()
    {
        SoyMusicContoller.Instance.PlayGameMusic();
        SceneManager.LoadScene("Main");
    }

    public void ShowGameOver(int score, bool win)
    {
        gameOverPanel.SetActive(true);
        winImage.SetActive(win);
        loseImage.SetActive(!win);
        scoreText.text = score.ToString();
        scoreTextInline.text = score.ToString();
    }

    public void ReturnToMainMenu()
    {
        homePanel.SetActive(true);
        gameOverPanel.SetActive(false);
        helpPanel.SetActive(false);
    }

    public void ToggleMusic()
    {
        SoyMusicContoller.Instance.ToggleMusic();
        musicDisabledImg.SetActive(!musicDisabledImg.activeSelf);
    }

    public void ToggleMobile(bool isMobile)
    {
        ToweraldStatic.isMobile = isMobile;
        leftieToggle.SetActive(isMobile);
    }

    public void ToggleLeftie(bool isLeftie)
    {
        ToweraldStatic.isLeftie = isLeftie;
    }
}
