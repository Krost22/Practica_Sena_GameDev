using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// AudioManager central (singleton persistente).
/// Gestiona música ambiental + SFX con un Audio Mixer para volúmenes independientes
/// (Master, Music, SFX).
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Música")]
    [SerializeField] private AudioSource musicSource;

    [Header("SFX")]
    [SerializeField] private int sfxPoolSize = 10;
    private AudioSource[] sfxSources;
    private int sfxIndex = 0;

    [Header("Configuración de volúmenes")]
    [SerializeField] private float masterVolume = 1f;
    [SerializeField] private float musicVolume = 0.7f;
    [SerializeField] private float sfxVolume = 1f;

    public static AudioManager Instance { get; private set; }

    // Claves PlayerPrefs para persistir volúmenes
    private const string MasterKey = "Audio_Master";
    private const string MusicKey = "Audio_Music";
    private const string SfxKey = "Audio_SFX";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Cargar volúmenes guardados
        masterVolume = PlayerPrefs.GetFloat(MasterKey, masterVolume);
        musicVolume = PlayerPrefs.GetFloat(MusicKey, musicVolume);
        sfxVolume = PlayerPrefs.GetFloat(SfxKey, sfxVolume);

        SetupAudioSources();
        ApplyVolumes();
    }

    private void SetupAudioSources()
    {
        // Asegurar música source
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }
        musicSource.loop = true;
        musicSource.playOnAwake = false;

        // Pool de SFX sources
        sfxSources = new AudioSource[sfxPoolSize];
        for (int i = 0; i < sfxPoolSize; i++)
        {
            sfxSources[i] = gameObject.AddComponent<AudioSource>();
            sfxSources[i].playOnAwake = false;
            sfxSources[i].spatialBlend = 0f; // 2D por defecto
        }
    }

    private void ApplyVolumes()
    {
        if (audioMixer != null)
        {
            // Convertir [0,1] a dB (-80 a 0)
            audioMixer.SetFloat("Master", LinearToDb(masterVolume));
            audioMixer.SetFloat("Music", LinearToDb(musicVolume));
            audioMixer.SetFloat("SFX", LinearToDb(sfxVolume));
        }
    }

    private float LinearToDb(float linear)
    {
        if (linear <= 0.0001f) return -80f;
        return 20f * Mathf.Log10(linear);
    }

    // ---- API pública ----

    public void PlayMusic(AudioClip clip, float fadeDuration = 1f)
    {
        if (clip == null || musicSource == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;
        musicSource.clip = clip;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null) musicSource.Stop();
    }

    public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;

        AudioSource src = sfxSources[sfxIndex];
        src.clip = clip;
        src.volume = volume;
        src.pitch = pitch;
        src.spatialBlend = 0f; // 2D
        src.Play();

        sfxIndex = (sfxIndex + 1) % sfxPoolSize;
    }

    public void PlaySFX3D(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;

        AudioSource src = sfxSources[sfxIndex];
        src.clip = clip;
        src.volume = volume;
        src.pitch = pitch;
        src.spatialBlend = 1f; // 3D
        src.transform.position = position;
        src.Play();

        sfxIndex = (sfxIndex + 1) % sfxPoolSize;
    }

    // ---- Setters de volumen (para UI de settings) ----
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(MasterKey, masterVolume);
        ApplyVolumes();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(MusicKey, musicVolume);
        ApplyVolumes();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(SfxKey, sfxVolume);
        ApplyVolumes();
    }

    public float GetMasterVolume() => masterVolume;
    public float GetMusicVolume() => musicVolume;
    public float GetSFXVolume() => sfxVolume;
}
