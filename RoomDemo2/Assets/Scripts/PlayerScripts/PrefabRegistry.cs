using System.Collections.Generic;
using UnityEngine;

public class PrefabRegistry : MonoBehaviour
{
    public static PrefabRegistry Instance;

    [SerializeField] private List<ItemPrefabMapping> itemPrefabs;

    private Dictionary<string, GameObject> prefabDictionary;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeRegistry();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeRegistry()
    {
        prefabDictionary = new Dictionary<string, GameObject>();

        foreach (var mapping in itemPrefabs)
        {
            if (!prefabDictionary.ContainsKey(mapping.ItemId))
            {
                prefabDictionary.Add(mapping.ItemId, mapping.Prefab);
            }
        }
    }

    public GameObject GetPrefab(string itemId)
    {
        if (prefabDictionary.TryGetValue(itemId, out var prefab))
        {
            return prefab;
        }

        Debug.LogWarning($"Prefab for item ID '{itemId}' not found!");
        return null;
    }
}

[System.Serializable]
public class ItemPrefabMapping
{
    public string ItemId;      // Corresponds to ItemBase.Id or Name
    public GameObject Prefab;  // Prefab associated with the item
}
