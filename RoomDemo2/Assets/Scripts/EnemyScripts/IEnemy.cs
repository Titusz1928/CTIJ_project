using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemy
{
    float MinPossibleHealth { get; }
    float MaxPossibleHealth { get; }
    float MinPossibleDamage { get; }
    float MaxPossibleDamage { get; }
    string getCurrentState();
}
