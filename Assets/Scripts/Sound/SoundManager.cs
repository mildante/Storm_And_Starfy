using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip level1Music;
    [SerializeField] private AudioClip level2Music;
    [SerializeField] private AudioClip level3Music;

    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip starSound;
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip loseSound;
    [SerializeField] private AudioClip buttonSound;

    private bool soundEnabled = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSoundState();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayMenuMusic()
    {
        PlayMusic(menuMusic);
    }

    public void PlayLevelMusic(int levelNumber)
    {
        switch (levelNumber)
        {
            case 1:
                PlayMusic(level1Music);
                break;
            case 2:
                PlayMusic(level2Music);
                break;
            case 3:
                PlayMusic(level3Music);
                break;
        }
    }

    private void PlayMusic(AudioClip clip)
    {
        if (!soundEnabled || clip == null || musicSource == null)
            return;

        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlayJump()
    {
        PlaySfx(jumpSound);
    }

    public void PlayStar()
    {
        PlaySfx(starSound);
    }

    public void PlayHurt()
    {
        PlaySfx(hurtSound);
    }

    public void PlayWin()
    {
        PlaySfx(winSound);
    }

    public void PlayLose()
    {
        PlaySfx(loseSound);
    }

    public void PlayButton()
    {
        PlaySfx(buttonSound);
    }

    private void PlaySfx(AudioClip clip)
    {
        if (!soundEnabled || clip == null || sfxSource == null)
            return;

        sfxSource.PlayOneShot(clip);
    }

    public void ToggleSound()
    {
        soundEnabled = !soundEnabled;

        if (!soundEnabled)
        {
            if (musicSource != null)
                musicSource.Stop();
        }
        else
        {
            PlayMenuMusic();
        }

        PlayerPrefs.SetInt("sound_enabled", soundEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadSoundState()
    {
        soundEnabled = PlayerPrefs.GetInt("sound_enabled", 1) == 1;
    }

    public bool IsSoundEnabled()
    {
        return soundEnabled;
    }
}