using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.AI.Navigation;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public GameObject roomPrefab; // Assign the Room prefab in the Inspector
    public GameObject floorTexturePrefab;
    public GameObject chestPrefab; // Assign the Chest prefab in the Inspector
    public GameObject guardEnemyPrefab;

    public GameObject herb1Prefab; // Healing herb prefab
    public GameObject herb2Prefab;
    public GameObject stonePrefab;

    public GameObject player;

    public int gridRows = 5;
    public int gridCols = 5;
    public float roomSize = 20f; // Adjust based on the prefab's size
    public NavMeshSurface navMeshSurface; // Assign the NavMeshSurface in the Inspector

    void Start()
    {
        GenerateGrid();
        BakeNavMesh();
    }

    void GenerateGrid()
    {
        for (int row = 0; row < gridRows; row++)
        {
            for (int col = 0; col < gridCols; col++)
            {
                // Calculate position for each room
                Vector3 position = new Vector3(col * roomSize, 3, row * roomSize);
                // Instantiate the room prefab
                GameObject room = Instantiate(roomPrefab, position, Quaternion.identity, transform);

                CreateFloorTexture(position);

                CreateCeilingTexture(position);

                // Tag walls and add mesh colliders
                TagWalls(room);
                AddMeshCollidersToWalls(room);

                // Attempt to generate a chest with a 10% chance
                TryGenerateChest(room);

                TryGenerateHerbs(room);

                TryGenerateStone(room);
            }
        }
    }

    void CreateFloorTexture(Vector3 roomPosition)
    {
        // Adjust the floor texture position to ensure Y is 0.1
        Vector3 floorPosition = new Vector3(roomPosition.x-10, 0.1f, roomPosition.z);

        // Instantiate the floor texture prefab
        Instantiate(floorTexturePrefab, floorPosition, Quaternion.identity, transform);
    }

    void CreateCeilingTexture(Vector3 roomPosition)
    {
        // Adjust the ceiling texture position to ensure Y is 5.9
        Vector3 ceilingPosition = new Vector3(roomPosition.x - 10, 5.9f, roomPosition.z);

        // Instantiate the ceiling texture prefab with a 180-degree rotation on the X-axis
        Quaternion ceilingRotation = Quaternion.Euler(180, 0, 0);
        Instantiate(floorTexturePrefab, ceilingPosition, ceilingRotation, transform);
    }

    void BakeNavMesh()
    {
        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
        }
        else
        {
            Debug.LogError("NavMeshSurface not assigned in the Inspector.");
        }
    }

    void AddMeshCollidersToWalls(GameObject room)
    {
        // Find all child objects of the room (assuming walls are children)
        Transform[] roomChildren = room.GetComponentsInChildren<Transform>();
        foreach (var child in roomChildren)
        {
            // Ensure only walls (objects that are not the room itself) have Mesh Colliders
            if (child.CompareTag("Wall")) // Assuming your walls are tagged with "Wall"
            {
                // Add a MeshCollider if the wall doesn't already have one
                if (child.GetComponent<MeshCollider>() == null)
                {
                    MeshCollider meshCollider = child.gameObject.AddComponent<MeshCollider>();
                    meshCollider.convex = false; // Convex setting for physics interactions
                }
            }
        }
    }

    void TagWalls(GameObject room)
    {
        // Find all child objects of the room (assuming walls are children)
        Transform[] roomChildren = room.GetComponentsInChildren<Transform>();
        foreach (var child in roomChildren)
        {
            // Tag walls with the "Wall" tag
            if (child.gameObject.GetComponent<MeshRenderer>() != null)
            {
                child.gameObject.tag = "Wall"; // Assign the "Wall" tag
            }
        }
    }

    void TryGenerateChest(GameObject room)
    {
        float chance = Random.Range(0f, 1f); // Generate a random number between 0 and 1
        if (chance <= 0.2f) // 10% chance to spawn a chest
        {
            // Determine the position for the chest within the room
            Vector3 chestPosition = room.transform.position + new Vector3(3, -2.7f, 9.5f); // Adjust the Y value
            Quaternion chestRotation = Quaternion.Euler(0, 0, 180); // Ensure upright orientation

            GameObject chest = Instantiate(chestPrefab, chestPosition, chestRotation, room.transform);

            // Optional: Log the position and rotation to debug
            Debug.Log($"Chest spawned at {chest.transform.position} with rotation {chest.transform.rotation}");

            SpawnGuardEnemies(room, chest);
        }
    }

    void SpawnGuardEnemies(GameObject room, GameObject chest)
    {
        // Determine how many guard enemies to spawn
        int guardCount = GetRandomGuardCount();

        for (int i = 0; i < guardCount; i++)
        {
            // Randomize spawn position around the chest within a small radius
            Vector3 randomOffset = new Vector3(
                Random.Range(-2f, 2f), // X offset
                0,                     // Keep guards on the ground
                Random.Range(-2f, 2f)  // Z offset
            );

            Vector3 guardPosition = chest.transform.position + randomOffset;
            Quaternion guardRotation = Quaternion.identity; // Default rotation

            // Instantiate the guard enemy
            GameObject guard = Instantiate(guardEnemyPrefab, guardPosition, guardRotation, room.transform);

            // Assign the chest's Transform as the guard's center of navigation
            GuardNavigation guardNav = guard.GetComponent<GuardNavigation>();
            if (guardNav != null)
            {
                guardNav.guardCenter = chest.transform; // Assign the chest's Transform
            }
            else
            {
                Debug.LogWarning("GuardNavigation script not found on guard enemy prefab!");
            }

        }
    }

    void TryGenerateHerbs(GameObject room)
    {
        float chance = Random.Range(0f, 1f); // Generate a random number between 0 and 1
        if (chance <= 0.4f) // 40% chance to spawn a herb
        {
            float x = Random.Range(0f, 16f);
            float z = Random.Range(0f, 16f);
            float herbState = Random.Range(0f, 2f);

            if (herbState > 1f)
            {
                GameObject herb = Instantiate(herb1Prefab, room.transform.position + new Vector3(x, -2.9f, z), Quaternion.identity, room.transform);

                // Ensure HerbAction component is attached to the herb prefab
                HerbAction herbAction = herb.GetComponent<HerbAction>();
                if (herbAction != null)
                {
                    // Assign the player health reference to the HerbAction component
                    herbAction.playerhealth = player.GetComponent<PlayerHealth>();  // Assign player health reference
                    herbAction.replacementPrefab = herb2Prefab;  // Assign the replacement prefab (Herb2)
                }
                else
                {
                    Debug.LogError("HerbAction component not found on herb1Prefab!");
                }

                // Optional: Log herb generation
                Debug.Log($"Herb1 generated at {herb.transform.position}. It will heal the player.");
            }
            else
            {
                // Create a Quaternion for 180 degrees rotation on the Y axis
                Quaternion rotation = Quaternion.Euler(0, 0, 180);
                GameObject herb = Instantiate(herb2Prefab, room.transform.position + new Vector3(x, -2.9f, z), rotation, room.transform);
            }
        }
    }

    void TryGenerateStone(GameObject room)
    {
        for (int i = 0; i < 5; i++)
        {
            float chance = Random.Range(0f, 1f); // Generate a random number between 0 and 1
            if (chance <= 0.5f) // 10% chance to spawn a stone
            {
                float x = Random.Range(0f, 16f);
                float z = Random.Range(0f, 16f);
                GameObject stone = Instantiate(stonePrefab, room.transform.position + new Vector3(x, -2.9f, z), Quaternion.identity, room.transform);

                // Ensure HerbAction component is attached to the herb prefab
                StoneAction stoneAction = stone.GetComponent<StoneAction>();

                Debug.Log($"stone generated at {stone.transform.position}");
            }
        }
    }


    int GetRandomGuardCount()
    {
        float chance = Random.Range(0f, 1f); // Generate a random value between 0 and 1

        if (chance < 0.4f)
            return 0; // 40% chance for 0
        else if (chance < 0.8f)
            return 1; // 40% chance for 1
        else
            return 2; // 20% chance for 2
    }
}
