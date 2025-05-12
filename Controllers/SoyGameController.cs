using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoyGameController : Singletroon<SoyGameController>
{
    [SerializeField] private SoyClimber soyClimber;
    [SerializeField] private Camera gameCam;
    private int score = 0;
    public bool IsPaused = false;

    #region UI_VARIABLES
    [Header("UI")]
    [SerializeField] private GameObject pauseScreen;
    [SerializeField] private TextMeshProUGUI scoreTxt;
    [SerializeField] private TextMeshProUGUI scoreTxtInline;
    public CanvasGroup fadeOverlay;
    #endregion

    #region TREADMILL_VARIABLES
    [Header("Treadmill")]
    [SerializeField] private Transform treadmill;
    public float baseScrollSpeed = 2.2f;
    public float scrollSpeed = 2.2f;
    public float maxScrollSpeed = 5f;
    private float currentScrollSpeed = 0f;
    public float overloadSpeedMultiplier = 1.6f;
    public float durationForMaxSpeed = 600f;
    private float scrollAcceleration = 2f;
    private Coroutine scrollRoutine = null;
    [SerializeField] private ObjectSpawner spawner;
    [SerializeField] private SoyRockZoneTracker topRockTracker;
    [SerializeField] private SoyRockZoneTracker bottomRockTracker;
    #endregion

    [Header("Active Items")]
    [SerializeField] private GameObject thremboMeterDisplay;
    private Dictionary<Item, SoyItem_Timed> timedItems = new();

    #region BOSS_VARIABLES
    [Header("Boss Battle")]
    public bool bossBattle = false;
    public float triggerBossBattleAtY = 321.3443f;
    public float playerTargetY = 351.226f;
    public float cameraTargetY = 355f;
    #endregion

    void Awake()
    {
        foreach (Item item in Enum.GetValues(typeof(Item)))
        {
            if (ToweraldStatic.IsTimedItem(item))
                timedItems[item] = new SoyItem_Timed();
        }
    }

    void Start()
    {
        scrollRoutine = StartCoroutine(ScrollUp());
    }

    private IEnumerator ScrollUp()
    {
        while (true)
        {
            if (IsPaused)
            {
                yield return null;
                continue;
            }

            float targetSpeed = GetTargetScrollSpeed();
            currentScrollSpeed = Mathf.MoveTowards(currentScrollSpeed, targetSpeed, scrollAcceleration * Time.deltaTime);

            // scroll at 20% speed if sproke is active
            float timeMultiplier = SoyTimeController.Instance.CurrentMultiplier < 1f ? 0.2f : 1f;

            treadmill.position += Vector3.up * 0.1f * currentScrollSpeed * Time.deltaTime * timeMultiplier;

            yield return null;
        }
    }

    private float GetTargetScrollSpeed()
    {
        // stop scrolling if meds are active or not enough rocks at the bottom of the screen
        if (timedItems[Item.Meds].IsActive || bottomRockTracker.LandedRockCount <= 3)
            return 0f;
        // ramp up scroll speed as game goes on
        float t = Mathf.Clamp01(Time.timeSinceLevelLoad / durationForMaxSpeed);
        float speed = Mathf.Lerp(baseScrollSpeed, maxScrollSpeed, t);
        // speed up if rocks stacked too high
        if (topRockTracker.LandedRockCount >= 3)
            speed *= overloadSpeedMultiplier;
        return speed;
    }

    public void IncreaseScore(int value)
    {
        score += value;
        string formattedScore = score.ToString("N0", CultureInfo.InvariantCulture).Replace(",", "\u2009");
        scoreTxt.text = formattedScore;
        scoreTxtInline.text = formattedScore;
    }

    public void TriggerTimedItem(Item item)
    {
        if (!timedItems.ContainsKey(item))
            return;
        timedItems[item].Trigger(ToweraldStatic.GetDurationForItem(item));

        switch (item)
        {
            case Item.Sproke:
                SoyTimeController.Instance.TriggerSlowTime();
                break;
            case Item.Ouchie:
                soyClimber.immune = true;
                break;
            case Item.Thrembometer:
                thremboMeterDisplay.SetActive(true);
                break;
        }
    }

    public bool HasActiveItem(Item item)
    {
        return timedItems.TryGetValue(item, out var entry) && entry.IsActive;
    }

    public bool TryGetTimedItem(Item item, out SoyItem_Timed timedItem)
    {
        return timedItems.TryGetValue(item, out timedItem);
    }

    public float GetTimedItemEndTime(Item item)
    {
        if (timedItems.TryGetValue(item, out var timedItem))
            return timedItem.endTime;
        return 0f;
    }


    public void LoseNutEarly()
    {
        if (timedItems.ContainsKey(Item.Nut))
        {
            timedItems[Item.Nut].Reset();
            SoyUI_ActiveItemsController.Instance.OnLoseNut();
            soyClimber.LoseNut();
        }
    }


    void Update()
    {
        if (IsPaused) return;

        if (!bossBattle && treadmill.position.y >= triggerBossBattleAtY)
        {
            bossBattle = true;
            StartCoroutine(StartBossBattle());
        }

        //check if a timed item just expired
        foreach (var kvp in timedItems)
        {
            if (!kvp.Value.ShouldEnd())
                continue;
            kvp.Value.Reset();
            switch (kvp.Key)
            {
                case Item.SoyMilk:
                    SoyUI_ActiveItemsController.Instance.OnLoseSoyMilk();
                    break;
                case Item.Magnet:
                    SoyUI_ActiveItemsController.Instance.OnLoseMagnet();
                    soyClimber.LoseMagnet();
                    break;
                case Item.Thrembometer:
                    thremboMeterDisplay.SetActive(false);
                    break;
                case Item.Ouchie:
                    soyClimber.immune = false;
                    SoyUI_ActiveItemsController.Instance.OnLoseOuchie();
                    break;
                case Item.Kebab:
                    SoyUI_ActiveItemsController.Instance.OnLoseKebab();
                    break;
                case Item.Nut:
                    SoyUI_ActiveItemsController.Instance.OnLoseNut();
                    soyClimber.LoseNut();
                    break;
                case Item.Meds:
                    SoyUI_ActiveItemsController.Instance.OnLoseMeds();
                    break;
            }
        }
    }

    private IEnumerator StartBossBattle()
    {
        Func<bool> isPaused = () => IsPaused;

        if (scrollRoutine != null) StopCoroutine(scrollRoutine);
        scrollRoutine = null;

        if (timedItems[Item.Thrembometer].IsActive)
        {
            timedItems[Item.Thrembometer].Reset();
            thremboMeterDisplay.SetActive(false);
        }

        ObjectSpawner.Instance.Stop();

        yield return CoroutineUtils.WaitWhilePaused(isPaused);
        yield return CoroutineUtils.WaitForUnpausedSeconds(0.5f, isPaused);
        yield return CoroutineUtils.WaitWhilePaused(isPaused);
        yield return StartCoroutine(FadeToWhite(0.5f));

        foreach (GameObject rock in GameObject.FindGameObjectsWithTag("Rock"))
            Destroy(rock);

        yield return CoroutineUtils.WaitWhilePaused(isPaused);
        yield return StartCoroutine(SoyMusicContoller.Instance.FadeOutMusic(1f));

        Camera.main.transform.SetParent(null);
        Camera.main.transform.position = new Vector3(0, cameraTargetY, -10);
        soyClimber.transform.position = new Vector3(soyClimber.transform.position.x, playerTargetY, -1);

        yield return CoroutineUtils.WaitWhilePaused(isPaused);
        yield return StartCoroutine(FadeFromWhite(1.0f));

        SoyAudioManager.Instance.PlayGodson();
        
        yield return CoroutineUtils.WaitWhilePaused(isPaused);
        yield return CoroutineUtils.WaitForUnpausedSeconds(3.5f, isPaused);

        SoyMusicContoller.Instance.PlayBossMusic();
        Godson.Instance.Init();
    }

    IEnumerator FadeToWhite(float duration)
    {
        Func<bool> isPaused = () => IsPaused;
        fadeOverlay.gameObject.SetActive(true);
        float t = 0f;
        while (t < duration)
        {
            if (!isPaused())
            {
                t += Time.unscaledDeltaTime;
                fadeOverlay.alpha = Mathf.Lerp(0, 1, t / duration);
            }
            yield return null;
        }
        fadeOverlay.alpha = 1;
    }

    IEnumerator FadeFromWhite(float duration)
    {
        Func<bool> isPaused = () => IsPaused;
        float t = 0f;
        while (t < duration)
        {
            if (!isPaused())
            {
                t += Time.deltaTime;
                fadeOverlay.alpha = Mathf.Lerp(1, 0, t / duration);
            }
            yield return null;
        }
        fadeOverlay.alpha = 0;
        fadeOverlay.gameObject.SetActive(false);
    }

    public void WinGame()
    {
        StopAllCoroutines();
        StartCoroutine(BackToMenu(true));
    }

    public void GameOver()
    {
        spawner.StopAllCoroutines();
        StartCoroutine(BackToMenu(false));
    }

    public IEnumerator BackToMenu(bool win = false)
    {
        if (win)
            yield return StartCoroutine(FadeToWhite(1.25f));
        yield return new WaitForSeconds(2f);
        SoySceneTransitionManager.Instance.cameFromGameOver = !win;
        SoySceneTransitionManager.Instance.cameFromWin = win;
        SoySceneTransitionManager.Instance.lastScore = score;
        SceneManager.LoadScene("Menu");
    }

    public void TogglePause()
    {
        IsPaused = !IsPaused;
        pauseScreen.SetActive(IsPaused);
        if (IsPaused)
            SoyMusicContoller.Instance.PauseMusic();
        else
            SoyMusicContoller.Instance.UnpauseMusic();
        Time.timeScale = IsPaused ? 0f : 1f;
    }
}