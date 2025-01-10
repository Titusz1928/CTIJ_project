using GDS.Sample;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ConsumableManager
{
    // Store effects as (health, stamina)
    public static readonly Dictionary<BaseId, (int health, float stamina)> Effects = new() {
        { BaseId.Apple, (20, 20f) },
        { BaseId.CookedPorkchop,(40,10f) },
        {BaseId.RawDogMeat,(25,5f) }
        // Add more consumables here
    };

}
