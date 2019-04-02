using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class Zone
{
    public string name;
    public int type;

    public bool isValid;

    public List<Vector3Int> tiles;

    public GameObject text;

    TileManager manager;

    public Zone(int zoneType, string zoneName, List<Vector3Int> zoneTiles)
    {
        type = zoneType;
        name = zoneName;
        tiles = zoneTiles;
        isValid = false;
        text = null;
    }

    public void Start()
    {
        manager = GameManager.manager.tileManager;
    }

    public bool[] CheckRequirements()
    {
        // Get required objects for the zone.
        bool[] objectsBoolList = new bool[manager.zoneTiles[type].requiredTiles.Length];

        for (int i = 0; (i < manager.zoneTiles[type].RequiredObjects.Count); i++)
        {
            Vector3Int position;
            objectsBoolList[i] = HasTile(manager.zoneTiles[type].requiredTiles[i], out position);
        }

        // True's/False's correspond to their index in requirements.
        return objectsBoolList;
    }

    public bool HasTile(TileType tileType, out Vector3Int position)
    {
        position = new Vector3Int();
        foreach (Vector3Int tile in tiles)
        {
            if (manager.objectTiles[manager.tileData.tileTypes[tile.x - manager.worldOrigin.x, tile.y - manager.worldOrigin.y, 2]] == tileType)
            {
                position = tile;
                return true;
            }
        }
        return false;
    }

    public Vector3Int FindClosestTile(Vector3 position, out float distance)
    {
        Vector3Int closestPos = tiles[0];
        distance = Vector3.Distance(tiles[0], position);
        foreach (Vector3Int tile in tiles)
        {
            if (Vector3.Distance(position, tile) <= distance)
            {
                distance = Vector3.Distance(position, tile);
                closestPos = tile;
            }
        }
        return closestPos;
    }

    public bool Indoors()
    {
        foreach (Vector3Int v in GetZoneBounds())
        {
            try
            {
                if (manager.wallTiles[manager.tileData.tileTypes[v.x - manager.worldOrigin.x, v.y - manager.worldOrigin.y, 1]] == null)
                {
                    if (manager.objects[v.x - manager.worldOrigin.x, v.y - manager.worldOrigin.y].name.Contains("Door"))
                    {
                        continue;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch
            {
                // If there aren't even enough walls to check, zone cannot possibly be indoors.
                return false;
            }
        }
        return true;
    }

    public List<Vector3Int> GetZoneBounds()
    {
        List<Vector3Int> returnList = new List<Vector3Int> { };
        Vector3Int BottomLeftPos = tiles[0],
            topRightPos = tiles[tiles.Count - 1];

        foreach (var i in tiles)
        {
            if ((i.x - BottomLeftPos.x < 0) && (i.y - BottomLeftPos.y < 0))
                BottomLeftPos = i;
            else if ((i.x - BottomLeftPos.x > 0) && (i.y - topRightPos.y > 0))
                topRightPos = i;
        }

        foreach (var i in tiles)
        {
            if ((i.x == BottomLeftPos.x || i.x == topRightPos.x) || (i.y == BottomLeftPos.y || i.y == topRightPos.y))
            {
                returnList.Add(i);
            }
        }
        return returnList;
    }

    public void UpdateTooltip()
    {
        Tooltip tooltip = text.GetComponentInChildren<Tooltip>();

        bool isIndoors = Indoors();
        if (!manager.zoneTiles[type].requiresSurrounded)
            isIndoors = true;

        tooltip.description = "";

        // Get all required objects, use a dictionary to monitor how many of each are needed.
        Dictionary<string, int> ObjectstoDisplay = new Dictionary<string, int>();
        bool[] lacking = CheckRequirements();
        List<string> lackingNames = new List<string>();

        var ro = manager.zoneTiles[type].RequiredObjects;
        for (int i = 0; i < ro.Count; i++)
        {
            if (lacking[i] == false) lackingNames.Add(ro[i].name);
            if (!ObjectstoDisplay.ContainsKey(ro[i].name)) ObjectstoDisplay.Add(ro[i].name, 1);
            else ObjectstoDisplay[ro[i].name]++;
        }

        // Display the information.
        if (ObjectstoDisplay.Keys.Count == 0 && isIndoors)
            tooltip.enabled = false;
        else
            tooltip.enabled = true;

        tooltip.header = manager.zoneTiles[type].name;
        foreach (KeyValuePair<string, int> i in ObjectstoDisplay)
        {
            int toGo = i.Value;
            foreach (var ii in lackingNames)
                if (i.Key == ii) toGo--;

            if (toGo == i.Value && isIndoors)
                tooltip.enabled = false;
            else
                tooltip.enabled = true;

            tooltip.description += "\n" + i.Key + ": " + toGo + "/" + i.Value + "\n";
        }
        if (manager.zoneTiles[type].requiresSurrounded)
            tooltip.description += "Zone must be indoors.";
    }

    public void Assess(bool addedToExisting = false)
    {
        manager = GameManager.manager.tileManager;
        if (text == null)
        {
            var i = GameObject.Instantiate(manager.ZoneIdentifier);
            text = i;
        }

        // First, find the bottom-left corner of the zone.
        int zoneSize = 0;
        Vector3Int BottomLeftPos = tiles[0],
            topRightPos = tiles[tiles.Count - 1];

        foreach (var i in tiles)
        {
            zoneSize++;
            if ((i.x - BottomLeftPos.x < 0) && (i.y - BottomLeftPos.y < 0))
            {
                BottomLeftPos = i;
            }
            else if ((i.x - BottomLeftPos.x > 0) && (i.y - topRightPos.y > 0))
            {
                topRightPos = i;
            }
        }
        text.transform.SetParent(manager.zoneTileMap.transform);

        RectTransform idrt = text.GetComponent<RectTransform>();
        Vector3Int size = topRightPos - (BottomLeftPos - Vector3Int.one);

        idrt.sizeDelta = new Vector2(size.x, size.y);

        idrt.position = BottomLeftPos;
        text.GetComponentInChildren<TextMeshProUGUI>().text = manager.zoneTiles[type].name;
    }
}

[System.Serializable]
public struct SerializableZone
{
    public string name;
    public int type;

    public bool isValid;

    public List<SerializableVector3> tiles;

    public SerializableZone(int zoneType, string zoneName, List<SerializableVector3> zoneTiles)
    {
        type = zoneType;
        name = zoneName;
        tiles = zoneTiles;
        isValid = false;
    }

    public static implicit operator SerializableZone(Zone value)
    {
        List<SerializableVector3> zoneTiles = value.tiles.ConvertAll(x => (SerializableVector3)(Vector3)x);
        return new SerializableZone(value.type, value.name, zoneTiles);
    }

    public static implicit operator Zone(SerializableZone value)
    {
        List<Vector3Int> zoneTiles = value.tiles.ConvertAll(x => Vector3Int.RoundToInt(x));
        return new Zone(value.type, value.name, zoneTiles);
    }
}
