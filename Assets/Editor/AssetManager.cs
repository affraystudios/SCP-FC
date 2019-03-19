using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AssetManager : MonoBehaviour {

    [MenuItem("Assets/Create/Tile Type/Tile Type")]
    public static void NewTile()
    {
        string file = EditorUtility.SaveFilePanelInProject("New Tile Type", "New Tile", "asset", "Create a new Tile Type");
        //Debug.Log(file);
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<TileType>(), file);
    }


    [MenuItem("Assets/Create/Tile Type/Wall Tile Type")]
    public static void NewWallTile()
    {
        string file = EditorUtility.SaveFilePanelInProject("New Wall Tile Type", "New Wall Tile", "asset", "Create a New Wall Tile Type");
        //Debug.Log(file);
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<WallTileType>(), file);
    }


    [MenuItem("Assets/Create/Tile Type/Floor Tile Type")]
    public static void NewFloorTile()
    {
        string file = EditorUtility.SaveFilePanelInProject("New Floor Tile Type", "New Floor Tile", "asset", "Create a new Floor Tile Type");
        //Debug.Log(file);
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<FloorTileType>(), file);
    }


    [MenuItem("Assets/Create/Tile Type/Object Tile Type")]
    public static void NewObjectTile()
    {
        string file = EditorUtility.SaveFilePanelInProject("New Object Tile Type", "New Object Tile", "asset", "Create a New Object Tile Type");
        //Debug.Log(file);
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<ObjectTileType>(), file);
    }

    [MenuItem("Assets/Create/Tile Type/Zone Tile Type")]
    public static void NewZoneTile()
    {
        string file = EditorUtility.SaveFilePanelInProject("New Zone Tile Type", "New Zone Tile", "asset", "Create a New Zone Tile Type");
        //Debug.Log(file);
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<ZoneTileType>(), file);
    }
    [MenuItem("Assets/Create/Tile Type/Utility Tile Type")]
    public static void NewUtilityTile()
    {
        string file = EditorUtility.SaveFilePanelInProject("New Utility Tile Type", "New Utility Tile", "asset", "Create a New Utility Tile Type");
        //Debug.Log(file);
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<UtilityTileType>(), file);
    }
}
