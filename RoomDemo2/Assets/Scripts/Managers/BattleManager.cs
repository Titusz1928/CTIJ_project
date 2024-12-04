using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [SerializeField] private GameObject battleCanvas;

    public void OnExitBattleButtonClick()
    {
        EndBattle();
        Debug.Log("Exited Battle");
        // Additional logic to reset the game state can go here if needed
    }

    public void EndBattle()
    {
        if (battleCanvas != null)
        {
            battleCanvas.SetActive(false);
        }
    }
}
