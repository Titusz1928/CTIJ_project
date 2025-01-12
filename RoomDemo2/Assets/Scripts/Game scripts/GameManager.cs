using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } // Singleton instance

    public int Seed { get; private set; }

    public static GameObject BattleCanvas;

    // Creative Mode state
    public bool CreativeMode { get; private set; } = false;

    void Awake()
    {
        // Ensure only one instance of GameManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroy duplicate instance
        }
        else
        {
            Instance = this; // Set instance to this GameManager
            DontDestroyOnLoad(gameObject); // Optional: keep GameManager between scene loads
        }

        // Use FindObjectsOfTypeAll to locate inactive objects
        BattleCanvas = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(), obj => obj.name == "BattleCanvas");

        if (BattleCanvas == null)
        {
            Debug.LogWarning("BattleCanvas not found in the scene.");
        }
        else
        {
            Debug.Log("BattleCanvas is found, even if inactive.");
        }
    }

    public void SetSeed(int seed)
    {
        Seed = seed;
        Debug.Log($"Seed set to: {Seed}");
    }

    public void GenerateRandomSeed()
    {
        Seed = Random.Range(0, int.MaxValue);
        Debug.Log($"Random seed generated: {Seed}");
    }

    public void ToggleCreativeMode()
    {
        CreativeMode = !CreativeMode;
        Debug.Log($"Creative Mode is now {(CreativeMode ? "enabled" : "disabled")}");
    }

}
