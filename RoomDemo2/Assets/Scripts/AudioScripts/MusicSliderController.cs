using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    public Slider musicSlider;
    public Slider soundEffectsSlider;

    private void Start()
    {
        if (AudioManager.Instance != null)
        {
            // Set initial slider values to match current volume
            musicSlider.value = AudioManager.Instance.GetMusicVolume();
            soundEffectsSlider.value = AudioManager.Instance.GetSoundEffectsVolume();

            // Assign the sliders' onValueChanged events to update the AudioManager
            musicSlider.onValueChanged.AddListener(AudioManager.Instance.SetMusicVolume);
            soundEffectsSlider.onValueChanged.AddListener(AudioManager.Instance.SetSoundEffectsVolume);
        }
    }

    private void OnDestroy()
    {
        // Remove listeners to avoid memory leaks
        musicSlider.onValueChanged.RemoveAllListeners();
        soundEffectsSlider.onValueChanged.RemoveAllListeners();
    }
}

