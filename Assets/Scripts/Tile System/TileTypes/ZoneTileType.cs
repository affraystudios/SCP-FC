using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ZoneTileType : TileType
{
    public List<GameObject> RequiredObjects;
    public TileType[] requiredTiles;

    public bool requiresSurrounded;
}
