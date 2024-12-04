using System.Collections;
using System.Collections.Generic;
using UnityEngine;


 public class GameManager : MonoBehaviour
{
        public static GameObject BattleCanvas;

    void Awake()
    {
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
}
