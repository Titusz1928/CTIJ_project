using GDS.Sample;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorManager : MonoBehaviour
{
    /*    public static readonly Dictionary<BaseId, (int armorvalue)> Effects = new()
        {
            { BaseId.WarriorHelmet, (6) },
            { BaseId.SteelBoots, (4) },
            { BaseId.LeatherArmor, (6) },
            { BaseId.SteelArmor, (10) }
        };*/

    public static readonly Dictionary<BaseId, int> Effects = new()
    {
        { BaseId.WarriorHelmet, 8 },
        { BaseId.SteelBoots, 6 },
        { BaseId.LeatherArmor, 8 },
        { BaseId.SteelArmor, 12 }
    };
}
