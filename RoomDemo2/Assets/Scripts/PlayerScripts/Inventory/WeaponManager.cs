using GDS.Sample;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public static readonly Dictionary<BaseId, int> Effects = new()
    {
        { BaseId.ShortSword, 30 },
    };
}
