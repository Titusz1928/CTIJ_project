using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class StickFigureSpawnManager : MonoBehaviour
{
    public Transform player;  // Reference to the player
    public GameObject enemyPrefab;  // The enemy prefab to spawn
    public float checkInterval = 5f;  // Time interval for checking the enemy count
    public float maxSpawnRadius = 100f;  // Max radius within which to spawn enemies around the player
    public float minSpawnRadius = 50f;   // Min radius outside which to spawn enemies from the player
    public int maxEnemies = 6;  // Max number of enemies to maintain around the player
    public int spawnCount = 3;  // Number of enemies to spawn if under maxEnemies

    private void Start()
    {
        StartCoroutine(CheckAndSpawnEnemies());
    }

    private IEnumerator CheckAndSpawnEnemies()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);  // Wait for the specified interval

            // Count the enemies within the specified radius around the player
            int enemyCount = 0;
            Collider[] hitColliders = Physics.OverlapSphere(player.position, maxSpawnRadius);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Enemy"))  // Assuming enemies have the tag "Enemy"
                {
                    enemyCount++;
                }
            }

            // Spawn enemies if count is below the maximum
            if (enemyCount < maxEnemies)
            {
                SpawnEnemies(maxEnemies - enemyCount);
            }
        }
    }

    private void SpawnEnemies(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 randomDirection;
            NavMeshHit hit;

            // Keep finding a valid spawn position until one is found on the NavMesh
            do
            {
                randomDirection = Random.insideUnitSphere * 100f;
                randomDirection += player.position;
            }
            while (Vector3.Distance(randomDirection, player.position) < 50f ||
                   !NavMesh.SamplePosition(randomDirection, out hit, 10f, NavMesh.AllAreas));

            // Instantiate at the valid NavMesh position
            GameObject enemy = Instantiate(enemyPrefab, hit.position, Quaternion.identity);
            Debug.Log("Stick Figure spawned");
        }
    }
}
