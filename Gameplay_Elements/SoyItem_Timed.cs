using UnityEngine;

[System.Serializable]
public class SoyItem_Timed : SoyItem
{
    public bool IsActive;
    public float startTime;
    public float endTime;

    public void Trigger(float duration)
    {
        float now = Time.time;
        if (IsActive && now < endTime)
            endTime += duration;
        else
        {
            startTime = now;
            endTime = now + duration;
            IsActive = true;
        }
    }

    public bool ShouldEnd() => IsActive && Time.time >= endTime;
    public void Reset() => IsActive = false;
}