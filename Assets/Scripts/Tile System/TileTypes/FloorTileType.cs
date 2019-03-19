using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorTileType : TileType
{
    public float movementCost = 1;
    public float movementRatio = 1; 
    //Above 1 prefers direct routes, better for outdoor spaces 
    //Below 1 prefers indirect routes, better for windy corrodors. 

}
