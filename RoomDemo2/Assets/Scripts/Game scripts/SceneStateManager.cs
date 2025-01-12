using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneStateManager : MonoBehaviour
{
    public static SceneStateManager Instance { get; private set; }
    public SceneState currentSceneState;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // Make sure this object persists across scenes
            Debug.Log("SceneStateManager initialized.");
        }
        else
        {
            Destroy(gameObject);
            Debug.Log("SceneStateManager already exists, destroying duplicate.");
        }
    }

}
