using GDS.Sample;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    //damage, crit multiplier, crit chance, stamina use, damage type index
    public static readonly Dictionary<BaseId, (float damage, float critmultiplier, float critchance, float staminaUse, int damageType)> Effects = new()
    {
        { BaseId.Club, (25f, 1.5f, 25f, 5f, 0) },       // Damage type 0: Blunt
        { BaseId.ShortSword, (50f, 1.25f, 40f, 10f, 1) }, // Damage type 1: Slash
        { BaseId.Spear, (35f, 1.5f, 30f, 7f, 2) } // Damage type 1: Pierce
    };
}
