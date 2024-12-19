using GDS.Sample;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    //damage,critmultiplier, critchance
    public static readonly Dictionary<BaseId, (float damage,float critmultiplier, float critchance, float staminaUse)> Effects = new()
    {
        { BaseId.Club, (25f,1.5f,25f,5f) },
        { BaseId.ShortSword, (50f,1.25f,40f,10f) }
    };
}
