using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SceneState
{
    public Vector3 playerPosition;
    public List<Vector3> enemyPositions = new List<Vector3>();

    // Method to save the current scene state
    public void SaveState(Transform player, List<Transform> enemies)
    {
        Debug.Log("saving scene");
        playerPosition = player.position;
        enemyPositions.Clear();

        foreach (var enemy in enemies)
        {
            enemyPositions.Add(enemy.position);
        }
    }

    // Method to load the saved state
    public void LoadState(Transform player, List<Transform> enemies)
    {
        player.position = playerPosition;

        for (int i = 0; i < enemies.Count && i < enemyPositions.Count; i++)
        {
            enemies[i].position = enemyPositions[i];
        }
    }

}
