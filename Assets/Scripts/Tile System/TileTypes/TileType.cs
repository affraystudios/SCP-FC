using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileType : ScriptableObject
{
    public new string name = "Tile";
    [Multiline]
    public string description = "Tile Description";
    public int price = 100;
    public TileBase[] tiles = new TileBase[1];

    public bool overrideConstraints;
    public TileManager.ConstraintAxis overrideAxis;
}
