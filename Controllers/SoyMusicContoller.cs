using System.Collections;
using UnityEngine;

public class SoyMusicContoller : MonoBehaviour
{
    public static SoyMusicContoller Instance;
    public bool musicOn = true;
    public AudioSource musicSource;
    public AudioClip gameTrack;
    public AudioClip bossTrack;


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

    public IEnumerator FadeOutMusic(float duration)
    {
        float startVolume = musicSource.volume;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            yield return null;
        }

        musicSource.volume = 0f;
        musicSource.Stop();
    }

    public void PlayGameMusic()
    {
        musicSource.clip = gameTrack;
        if (musicOn)
        {
            musicSource.volume = 1f;
            musicSource.Play();
        }
    }

    public void PlayBossMusic()
    {
        musicSource.clip = bossTrack;
        if (musicOn)
        {
            musicSource.volume = 1f;
            musicSource.Play();
        }
    }
    public void PauseMusic()
    {
        if (!musicOn)
            return;
        musicSource.Pause();
    }

    public void UnpauseMusic()
    {
        if (!musicOn)
            return;
        musicSource.UnPause();
    }

    public void ToggleMusic()
    {
        musicOn = !musicOn;
        if (musicOn)
        {
            musicSource.UnPause();
        }
        else
        {
            musicSource.Pause();
        }
    }
}
