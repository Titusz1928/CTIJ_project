using GDS.Sample;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    // Define possible drops for each enemy type
    public static readonly Dictionary<string, List<(BaseId itemId, float dropChance)>> EnemyDrops = new()
    {
        {
            "Enemy", new List<(BaseId, float)>
            {
                (BaseId.Goldcoin, 0.8f) // 80% chance to drop a gold coin
            }
        },
        {
            "GuardEnemy", new List<(BaseId, float)>
            {
                (BaseId.Goldcoin, 0.5f), // 50% chance to drop a gold coin
                (BaseId.Apple, 0.2f)    // 20% chance to drop an apple
            }
        }
    };

    // Method to get drops for a specific enemy type
    public static List<(BaseId, float)> GetDrops(string enemyType)
    {
        return EnemyDrops.ContainsKey(enemyType) ? EnemyDrops[enemyType] : new List<(BaseId, float)>();
    }
}
