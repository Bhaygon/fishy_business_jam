using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioSource musicSource, themeSource, sfxSource;
    [Space]
    public AudioClip mainMusic;
    public AudioClip gameOverMusic;
    public AudioClip gotHitTheme;
    public AudioClip menuMusic;

    public void GameOverSound()
    {
        musicSource.Stop();
        themeSource.clip = gameOverMusic;
        sfxSource.clip = gotHitTheme;
        
        themeSource.Play();
        sfxSource.Play();
    }

    public void StartGameAudio()
    {
        musicSource.clip = mainMusic;
        musicSource.Play();
        themeSource.Stop();
        sfxSource.Stop();
    }
}
