using System.Collections.Generic;
using UnityEngine;

public static class EnemyResistanceManager
{
    // Dictionary to map script names to an array of resistance multipliers by index
    public static readonly Dictionary<string, float[]> EnemyResistances = new()
    {
        { "NavigationScript", new[] { 1.1f, 1.2f, 1.3f } }, // Lightly armored: Blunt = 1.1, Slash = 1.2
        { "GuardNavigation", new[] { 1.4f, 0.7f, 0.5f } }, // Heavily armored: Blunt = 1.4, Slash = 0.7
        { "DogNavigationScript", new[] { 0.9f, 1.5f, 1.7f } }    // Unarmored: Blunt = 0.9, Slash = 1.5
    };
}
