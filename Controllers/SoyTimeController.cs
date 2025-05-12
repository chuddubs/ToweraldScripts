using UnityEngine;

public class SoyTimeController : Singletroon<SoyTimeController>
{
    private float timeMultiplier = 1f;
    public float CurrentMultiplier => timeMultiplier;
    private float slowStartTime = 0f;
    private float slowEndTime = 0f;
    public float GetSprokeStartTime() => slowStartTime;
    public float GetSprokeEndTime() => slowEndTime;
    private SoyGameController gc;

    void Start()
    {
        gc = SoyGameController.Instance;
        enabled = false;
    }

    public void TriggerSlowTime()
    {
        float now = Time.time;

        if (timeMultiplier < 1f && now < slowEndTime)
        {
            slowEndTime +=  ToweraldStatic.GetDurationForItem(Item.Sproke);
        }
        else
        {
            slowStartTime = now;
            slowEndTime = now + ToweraldStatic.GetDurationForItem(Item.Sproke);
            timeMultiplier = ToweraldStatic.sprokeTimeFactor;
            enabled = true;
        }
    }


    void Update()
    {
        if (gc.IsPaused)
            return;

        if (timeMultiplier < 1f && Time.time >= slowEndTime)
        {
            timeMultiplier = 1f;
            SoyUI_ActiveItemsController.Instance.OnLoseSproke();
            enabled = false;
        }
    }


}

