using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.AI.Navigation;
using UnityEngine;

public class AdvancedMapGenerator : MonoBehaviour
{
    public GameObject roomPrefab4D; // Assign the Room prefab in the Inspector
    public GameObject roomPrefab3D;
    public GameObject roomPrefab2Da;
    public GameObject roomPrefab2Db;
    public GameObject roomPrefab1D;
    public GameObject floorTexturePrefab;
    public GameObject chestPrefab; // Assign the Chest prefab in the Inspector
    public GameObject guardEnemyPrefab;

    public GameObject herb1Prefab; // Healing herb prefab
    public GameObject herb2Prefab;
    public GameObject stonePrefab;

    public GameObject player;

    public int gridRows = 11;
    public int gridCols = 11;
    public float roomSize = 20f; // Adjust based on the prefab's size
    public NavMeshSurface navMeshSurface; // Assign the NavMeshSurface in the Inspector

    private System.Random randomGenerator;

    void Start()
    {
        int seed = GameManager.Instance.Seed;

        if (seed == 0) // In case no seed is set, generate a random seed
        {
            seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            GameManager.Instance.SetSeed(seed); // Optionally update GameManager with this generated seed
        }
        Debug.Log($"Using Seed: {seed}");

        randomGenerator = new System.Random(seed);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        GenerateGrid();
        BakeNavMesh();
    }

    void GenerateGrid()
    {
        GameObject[,] grid = new GameObject[gridRows, gridCols]; // Track rooms

        for (int row = 0; row < gridRows; row++)
        {
            for (int col = 0; col < gridCols; col++)
            {
                // Calculate position for each room
                Vector3 position = new Vector3(col * roomSize, 3, row * roomSize);

                // Randomly select a room prefab
                GameObject selectedRoomPrefab = SelectRandomRoomPrefab(out bool needsRotation);

                // Instantiate the room prefab with a random rotation if needed
                Quaternion rotation = needsRotation
                    ? Quaternion.Euler(0, randomGenerator.Next(0, 4) * 90, 0)
                    : Quaternion.identity;

                GameObject room = Instantiate(selectedRoomPrefab, position, rotation, transform);

                grid[row, col] = room; // Store room in the grid

                CreateFloorTexture(position);
                CreateCeilingTexture(position);

                // Tag walls and add mesh colliders
                TagWalls(room);
                AddMeshCollidersToWalls(room);

                // Attempt to generate additional items
                TryGenerateChest(room);
                TryGenerateHerbs(room);
                TryGenerateStone(room);
            }
        }

        // Remove overlapping walls
        RemoveOverlappingWalls(grid);
    }

    GameObject SelectRandomRoomPrefab(out bool needsRotation)
    {
        // Define probabilities for each room type
        var roomPrefabs = new (GameObject prefab, float probability, bool needsRotation)[]
        {
        (roomPrefab4D, 0.5f, false),   // 50% chance, no rotation needed
        (roomPrefab3D, 0.2f, false),  // 20% chance, rotation allowed
        (roomPrefab2Da, 0.15f, false),  // 15% chance, rotation allowed
        (roomPrefab2Db, 0.1f, false), // 10% chance, rotation allowed
        (roomPrefab1D, 0.05f, false)  // 5% chance, rotation allowed
        };

        // Generate a random value between 0 and 1 using your seeded random generator
        float randomValue = (float)randomGenerator.NextDouble();

        // Iterate through room types and select based on cumulative probability
        float cumulative = 0;
        foreach (var room in roomPrefabs)
        {
            cumulative += room.probability;
            if (randomValue <= cumulative)
            {
                needsRotation = room.needsRotation;
                return room.prefab;
            }
        }

        // Fallback (should not happen if probabilities sum to 1)
        needsRotation = false;
        return roomPrefab4D;
    }

    void RemoveOverlappingWalls(GameObject[,] grid)
    {
        for (int row = 0; row < gridRows; row++)
        {
            for (int col = 0; col < gridCols; col++)
            {
                GameObject room = grid[row, col];
                if (room == null) continue;

                // Check right neighbor
                if (col < gridCols - 1 && grid[row, col + 1] != null)
                {
                    RemoveWall(room, "Right");
                    RemoveWall(grid[row, col + 1], "Left");
                }

                // Check top neighbor
                if (row < gridRows - 1 && grid[row + 1, col] != null)
                {
                    RemoveWall(room, "Top");
                    RemoveWall(grid[row + 1, col], "Bottom");
                }
            }
        }
    }

    void RemoveWall(GameObject room, string direction)
    {
        // Find the wall corresponding to the direction and deactivate it
        Transform wall = room.transform.Find($"Wall_{direction}"); // Assuming walls are named appropriately
        if (wall != null)
        {
            wall.gameObject.SetActive(false);
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
        // Generate a random chance using the seeded random generator
        float chance = (float)randomGenerator.NextDouble(); // Random value between 0 and 1
        if (chance <= 0.1f) // 20% chance to spawn a chest
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

    int GetRandomGuardCount()
    {
        // Generate a random number between 0 and 100 to represent percentage chances
        int randomValue = randomGenerator.Next(0, 100);  // Random integer between 0 and 99

        if (randomValue < 40)  // 40% chance for 0 guards
        {
            return 0;
        }
        else if (randomValue < 70)  // 30% chance for 1 guard (70% - 40% = 30%)
        {
            return 1;
        }
        else  // 30% chance for 2 guards (100% - 70% = 30%)
        {
            return 2;
        }
    }


    void SpawnGuardEnemies(GameObject room, GameObject chest)
    {
        // Determine how many guard enemies to spawn
        int guardCount = GetRandomGuardCount();

        for (int i = 0; i < guardCount; i++)
        {
            // Randomize spawn position around the chest within a small radius using the seeded random generator
            Vector3 randomOffset = new Vector3(
                (float)(randomGenerator.NextDouble() * 4 - 2), // X offset: random value between -2 and 2
                0,                                            // Keep guards on the ground
                (float)(randomGenerator.NextDouble() * 4 - 2) // Z offset: random value between -2 and 2
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
        // Generate a random number between 0 and 1 using the seeded random generator
        float chance = (float)randomGenerator.NextDouble();
        if (chance <= 0.4f) // 40% chance to spawn a herb
        {
            float x = (float)(randomGenerator.NextDouble() * 16f); // Random X position between 0 and 16
            float z = (float)(randomGenerator.NextDouble() * 16f); // Random Z position between 0 and 16
            float herbState = (float)randomGenerator.NextDouble() * 2f; // Random state between 0 and 2

            if (herbState > 1f)
            {
                GameObject herb = Instantiate(herb1Prefab, room.transform.position + new Vector3(x, -2.9f, z), Quaternion.identity, room.transform);

                // Ensure HerbAction component is attached to the herb prefab
                HerbAction herbAction = herb.GetComponent<HerbAction>();
                if (herbAction != null)
                {
                    // Assign the player health reference to the HerbAction component
                    herbAction.playerhealth = player.GetComponent<PlayerHealth>();
                    herbAction.replacementPrefab = herb2Prefab; // Assign the replacement prefab (Herb2)
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
            // Generate a random number between 0 and 1 using the seeded random generator
            float chance = (float)randomGenerator.NextDouble();
            if (chance <= 0.5f) // 50% chance to spawn a stone
            {
                float x = (float)(randomGenerator.NextDouble() * 16f); // Random X position between 0 and 16
                float z = (float)(randomGenerator.NextDouble() * 16f); // Random Z position between 0 and 16
                GameObject stone = Instantiate(stonePrefab, room.transform.position + new Vector3(x, -2.9f, z), Quaternion.identity, room.transform);

                // Ensure StoneAction component is attached to the stone prefab
                StoneAction stoneAction = stone.GetComponent<StoneAction>();
                if (stoneAction != null)
                {
                    Debug.Log($"Stone generated at {stone.transform.position}");
                }
                else
                {
                    Debug.LogWarning("StoneAction component not found on stonePrefab!");
                }
            }
        }
    }


}
