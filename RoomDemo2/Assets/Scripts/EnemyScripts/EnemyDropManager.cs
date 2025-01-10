using GDS.Sample;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDropManager : MonoBehaviour
{
    // Define possible drops for each enemy type
    public static readonly Dictionary<string, List<(BaseId itemId, float dropChance)>> EnemyDrops = new()
    {
        {
            "Enemy", new List<(BaseId, float)>
            {
                (BaseId.Goldcoin, 0.2f),
                (BaseId.Apple, 0.5f)
            }
        },
        {
            "GuardEnemy", new List<(BaseId, float)>
            {
                (BaseId.Goldcoin, 0.5f), 
                (BaseId.Apple, 0.5f)  , 
                (BaseId.CookedPorkchop, 0.5f)   
            }
        },
        {
            "DogEnemy", new List<(BaseId, float)>
            {
                (BaseId.RawDogMeat, 0.8f)    
            }
        }
    };

    // Method to get drops for a specific enemy type
    public static List<(BaseId, float)> GetDrops(string enemyType)
    {
        return EnemyDrops.ContainsKey(enemyType) ? EnemyDrops[enemyType] : new List<(BaseId, float)>();
    }
}
