using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class SceneMusicController : MonoBehaviour
{
    public AudioClip sceneMusic;
    public AudioClip battleMusic;
    public AudioClip bigBattleMusic;
    public AudioClip battleStartSound;
    public AudioClip gameoverSound;

    void Start()
    {
        playGameMusic();
    }

    public void playGameMusic()
    {
        if (AudioManager.Instance != null && sceneMusic != null)
        {
            AudioManager.Instance.PlayMusic(sceneMusic);
        }
    }

    public void playBattleMusic()
    {
        if (AudioManager.Instance != null && battleMusic != null)
        {
            AudioManager.Instance.PlayMusic(battleMusic);
        }
    }

    public void playBigBattleMusic()
    {
        if (AudioManager.Instance != null && bigBattleMusic != null)
        {
            AudioManager.Instance.PlayMusic(bigBattleMusic);
        }
    }
}

