using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;       // For background music
    public AudioSource soundEffectsSource; // For sound effects

    [Header("Volume Settings")]
    [Range(0, 1)] public float musicVolume = 1f;         // Music volume (0 to 1)
    [Range(0, 1)] public float soundEffectsVolume = 1f;  // Sound effects volume (0 to 1)

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
            Debug.Log("AudioManager initialized in Awake");
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate
            Debug.Log("Duplicate AudioManager destroyed");
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }

    public void SetSoundEffectsVolume(float volume)
    {
        soundEffectsVolume = Mathf.Clamp01(volume);
        if (soundEffectsSource != null)
        {
            soundEffectsSource.volume = soundEffectsVolume;
        }
    }

    public void ToggleMusic(bool isOn)
    {
        if (musicSource != null)
        {
            musicSource.mute = !isOn;
        }
    }

    public void ToggleSoundEffects(bool isOn)
    {
        if (soundEffectsSource != null)
        {
            soundEffectsSource.mute = !isOn;
        }
    }

    public void PlaySoundEffect(AudioClip clip)
    {
        if (soundEffectsSource != null && clip != null)
        {
            soundEffectsSource.PlayOneShot(clip, soundEffectsVolume);
            Debug.Log("Sound effect played: " + clip.name);
        }
        else
        {
            Debug.LogError("Sound effect could not be played! AudioSource or clip is missing.");
        }
    }

    public void PlayMusic(AudioClip newMusic, float fadeDuration = 1f)
    {
        if (musicSource.clip == newMusic) return; // Avoid restarting the same music

        StartCoroutine(FadeMusic(newMusic, fadeDuration));
    }

    private IEnumerator FadeMusic(AudioClip newMusic, float fadeDuration)
    {
        // Fade out current music
        if (musicSource.isPlaying)
        {
            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                musicSource.volume = Mathf.Lerp(musicVolume, 0f, t / fadeDuration);
                yield return null;
            }
            musicSource.Stop();
        }

        // Switch to the new music
        musicSource.clip = newMusic;
        musicSource.Play();

        // Fade in new music
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(0f, musicVolume, t / fadeDuration);
            yield return null;
        }
    }

    public float GetMusicVolume()
    {
        return musicVolume;
    }

    public float GetSoundEffectsVolume()
    {
        return soundEffectsVolume;
    }
}
