using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Sound Effects (assign in Inspector)")]
    [Tooltip("Coin collection sound")]
    public AudioClip coinCollectClip;

    [Tooltip("Button click sound")]
    public AudioClip buttonClickClip;

    [Tooltip("Countdown warning beep")]
    public AudioClip countdownWarningClip;

    [Tooltip("Level success sound")]
    public AudioClip levelSuccessClip;

    [Tooltip("Level fail sound")]
    public AudioClip levelFailClip;

    [Header("Music (assign in Inspector)")]
    [Tooltip("Main menu background music")]
    public AudioClip menuMusicClip;

    [Tooltip("Gameplay background music")]
    public AudioClip gameplayMusicClip;

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float sfxVolume = 0.8f;
    [Range(0f, 1f)]
    public float musicVolume = 0.5f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudioSources()
    {
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        sfxSource.volume = sfxVolume;
        musicSource.volume = musicVolume;

        GenerateFallbackClips();
    }

    private void GenerateFallbackClips()
    {
        if (coinCollectClip == null)
            coinCollectClip = SimpleSFXGenerator.GenerateCoinCollect();
        if (buttonClickClip == null)
            buttonClickClip = SimpleSFXGenerator.GenerateButtonClick();
        if (countdownWarningClip == null)
            countdownWarningClip = SimpleSFXGenerator.GenerateCountdownWarning();
        if (levelSuccessClip == null)
            levelSuccessClip = SimpleSFXGenerator.GenerateLevelSuccess();
        if (levelFailClip == null)
            levelFailClip = SimpleSFXGenerator.GenerateLevelFail();
        if (menuMusicClip == null)
            menuMusicClip = SimpleSFXGenerator.GenerateMenuMusic();
        if (gameplayMusicClip == null)
            gameplayMusicClip = SimpleSFXGenerator.GenerateGameplayMusic();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
            sfxSource.PlayOneShot(clip, sfxVolume);
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip != null && musicSource != null)
        {
            if (musicSource.clip != clip)
            {
                musicSource.clip = clip;
                musicSource.volume = musicVolume;
                musicSource.Play();
            }
        }
    }

    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }

    public void PlayCoinCollect()
    {
        PlaySFX(coinCollectClip);
    }

    public void PlayButtonClick()
    {
        PlaySFX(buttonClickClip);
    }

    public void PlayCountdownWarning()
    {
        PlaySFX(countdownWarningClip);
    }

    public void PlayLevelSuccess()
    {
        PlaySFX(levelSuccessClip);
    }

    public void PlayLevelFail()
    {
        PlaySFX(levelFailClip);
    }

    public void PlayMenuMusic()
    {
        PlayMusic(menuMusicClip);
    }

    public void PlayGameplayMusic()
    {
        PlayMusic(gameplayMusicClip);
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
            sfxSource.volume = sfxVolume;
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
            musicSource.volume = musicVolume;
    }
}
