using System;
using System.Collections;
using UnityEngine;

public static class CoroutineUtils
{
    public static IEnumerator WaitForUnpausedSeconds(float seconds, Func<bool> isPaused)
    {
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            if (!isPaused())
                elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    public static IEnumerator WaitWhilePaused(Func<bool> isPaused)
    {
        while (isPaused())
            yield return null;
    }
}

