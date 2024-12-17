using GDS.Sample;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ConsumableManager
{
    // Store effects as (health, stamina)
    public static readonly Dictionary<BaseId, (int health, int stamina)> Effects = new() {
        { BaseId.Apple, (20, 5) } // Apple restores 20 health and 5 stamina
        // Add more consumables here
    };

}
