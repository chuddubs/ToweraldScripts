using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoyAudioManager : Singletroon<SoyAudioManager>
{
    public AudioSource audioSource;
    public AudioClip   gemLanding;
    public AudioClip[] rockThuds;
    public AudioClip[] rockBreaks;
    public AudioClip   bugSplat;
    public AudioClip   godsonVoiceline;
    
    public void PlaySplat()
    {
        audioSource.PlayOneShot(bugSplat);
    }

    public void PlayGodson()
    {
        audioSource.PlayOneShot(godsonVoiceline);
    }

    public void Play(AudioClip clip, float volume = 1f)
    {
        audioSource.PlayOneShot(clip, volume);
    }

    public void PlayRockBreak()
    {
        audioSource.PlayOneShot(rockBreaks[Random.Range(0, rockBreaks.Length)]);
    }

    public void PlayMineralLanding(bool isGem, float volume = 1f)
    {
        if (isGem)
            audioSource.PlayOneShot(gemLanding, 0.6f);
        else
            audioSource.PlayOneShot(rockThuds[Random.Range(0, rockThuds.Length)], volume);
    }
}
