using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource bgmSource;

    [Header("Clips")]
    public AudioClip uiClick;
    public AudioClip cubeTouch;
    public AudioClip collision;
    public AudioClip winSound;
    public AudioClip bgm;

    [Header("UI Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider soundSlider;

    const string MUSIC_KEY = "MusicVolume";
    const string SOUND_KEY = "SoundVolume";

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        LoadVolume();
        SetupSliders();
    }

    // ==============================
    // SLIDER SETUP
    // ==============================
    void SetupSliders()
    {
        if (musicSlider)
        {
            musicSlider.value = bgmSource.volume;
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (soundSlider)
        {
            soundSlider.value = sfxSource.volume;
            soundSlider.onValueChanged.AddListener(SetSoundVolume);
        }
    }

    // ==============================
    // VOLUME CONTROL
    // ==============================
    public void SetMusicVolume(float value)
    {
        if (!bgmSource) return;

        bgmSource.volume = value;
        PlayerPrefs.SetFloat(MUSIC_KEY, value);
    }

    public void SetSoundVolume(float value)
    {
        if (!sfxSource) return;

        sfxSource.volume = value;
        PlayerPrefs.SetFloat(SOUND_KEY, value);
    }

    void LoadVolume()
    {
        float musicVol = PlayerPrefs.GetFloat(MUSIC_KEY, 1f);
        float soundVol = PlayerPrefs.GetFloat(SOUND_KEY, 1f);

        if (bgmSource) bgmSource.volume = musicVol;
        if (sfxSource) sfxSource.volume = soundVol;
    }

    // ==============================
    // PLAY METHODS (UNCHANGED)
    // ==============================
    public void PlayUIClick() => PlaySFX(uiClick);
    public void PlayCubeTouch() => PlaySFX(cubeTouch);
    public void PlayCollision() => PlaySFX(collision);
    public void PlayWinSound() => PlaySFX(winSound);

    public void PlayBGM()
    {
        if (!bgmSource || !bgm) return;

        bgmSource.clip = bgm;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    void PlaySFX(AudioClip clip)
    {
        if (!clip || !sfxSource) return;
        sfxSource.PlayOneShot(clip);
    }
}
