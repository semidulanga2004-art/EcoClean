using UnityEngine;

[DisallowMultipleComponent]
public sealed class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip parkAmbience;
    [SerializeField] private AudioClip uiClick;
    [SerializeField] private AudioClip correctSort;
    [SerializeField] private AudioClip incorrectSort;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 0.75f;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.45f;
    [SerializeField, Range(0f, 1f)] private float ambienceVolume = 0.22f;

    private AudioSource musicSource;
    private AudioSource ambienceSource;
    private AudioSource effectsSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = CreateSource(true, musicVolume);
        ambienceSource = CreateSource(true, ambienceVolume);
        effectsSource = CreateSource(false, sfxVolume);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void PlayMusic() => PlayMusic(backgroundMusic);

    public void PlayMusic(AudioClip clip) => PlayLoop(musicSource, clip);

    public void StopMusic() => musicSource?.Stop();

    public void PlayAmbience() => PlayAmbience(parkAmbience);

    public void PlayAmbience(AudioClip clip) => PlayLoop(ambienceSource, clip);

    public void StopAmbience() => ambienceSource?.Stop();

    public void SetMusicVolume(float volume) => SetVolume(musicSource, ref musicVolume, volume);

    public void SetAmbienceVolume(float volume) => SetVolume(ambienceSource, ref ambienceVolume, volume);

    private AudioSource CreateSource(bool loop, float volume)
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = loop;
        source.volume = Mathf.Clamp01(volume);
        return source;
    }

    private static void PlayLoop(AudioSource source, AudioClip clip)
    {
        if (source == null || clip == null || (source.clip == clip && source.isPlaying))
        {
            return;
        }

        source.clip = clip;
        source.Play();
    }

    private static void SetVolume(AudioSource source, ref float configuredVolume, float volume)
    {
        configuredVolume = float.IsNaN(volume) ? 0f : Mathf.Clamp01(volume);
        if (source != null)
        {
            source.volume = configuredVolume;
        }
    }

    public void SetSfxVolume(float volume) => SetVolume(effectsSource, ref sfxVolume, volume);

    public void PlayUIClick() => PlaySoundEffect(uiClick);

    public void PlayCorrectSort() => PlaySoundEffect(correctSort);

    public void PlayIncorrectSort() => PlaySoundEffect(incorrectSort);

    public void PlaySoundEffect(AudioClip clip)
    {
        if (clip != null && effectsSource != null)
        {
            effectsSource.PlayOneShot(clip);
        }
    }
}
