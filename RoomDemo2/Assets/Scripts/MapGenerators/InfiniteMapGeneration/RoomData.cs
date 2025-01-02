using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomData
{
    public string prefabName;
    public Vector3 position;
    public Quaternion rotation;
    public bool hasTopWall;
    public bool hasBottomWall;
    public bool hasLeftWall;
    public bool hasRightWall;
    public bool containsChest;
    public bool containsHerbs;
    public bool containsStones;
}
