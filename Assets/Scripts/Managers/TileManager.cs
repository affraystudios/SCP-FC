using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using TMPro;
using sys = System;
using System.Linq;

public class TileManager : MonoBehaviour
{
    public enum ConstraintAxis
    {
        X,
        Y,
        Rectangle,
        Single
    }

    [System.Serializable]
    public struct Biome
    {
        public string name;
        public int[] tiles;
    }

    #region Variables

    [Header("References")]

    public GameObject ZoneIdentifier;
    public Transform objectTileGrid,
        utilityTileGrid;
    public Tilemap[] tilemaps,
        blueprintTileMaps;

    public Tilemap selectionTileMap,
        damageTileMap,
        powerTileMap,
        zoneTileMap,
        waterTileMap;

    InputManager input;

    public GameObject effectPrefab;

    [Header("Tile Settings")]

    public float tileSize = 1;
    public Tile selectionTile,
        removeTile;

    public TileType[] backgroundTiles;
    public FloorTileType[] floorTiles;
    public WallTileType[] wallTiles;
    public ObjectTileType[] objectTiles;
    public ObjectTileType[] placeableAI;
    public ZoneTileType[] zoneTiles;
    public Tile[] damageTiles;
    public TileBase[] waterTiles;
    public ObjectTileType resourceType;
    public UtilityTileType[] utilityTiles;

    public SaveableObject[,] objects;
    public SaveableObject[,] utilityObjects;

    public List<Electronic> electronics;

    public List<Vector3Int> selection;
    public Vector3 lastSelectionPos;
    Vector3Int startSelection;
    Vector3Int endSelection;
    public Vector3Int worldOrigin;

    public List<Zone> zones;

    [Header("World Settings")]

    public Vector2 worldSize = Vector2.one * 30;
    public int biome;
    public Biome[] biomes;
    public Vector2 offset;

    [Header("Data")]

    public TileData tileData;

    public bool placing;
    public bool removing;
    public int currentLayer,
        currentTile, currentRotation;

    public List<Task> availableTasks;

    public List<Vector3Int> waterToSimulate;

    TaskType buildTaskType,
        removeTaskType;
    int[,] tileTypesToBuild;
    TileType[] tiles;

    #endregion

    private void Start()
    {
        input = GameManager.manager.inputManager;
        GameManager.manager.tileManager = this;

        biome = GameManager.manager.worldSettings.biome;
        offset.x = Random.Range(-10000, 10000);
        offset.y = Random.Range(-10000, 10000);

        if (!GameManager.manager.saveManager.loading)
        {
            InitWorld();
        }
        UpdateLayer(currentLayer);
    }

    private void Update()
    {
        if (Time.timeScale <= 0)
            return;

        UpdateWater();

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(input.cursorPosition);

        if (!placing && !removing)
        {
            ClearSelection();
            AddToSelection(mousePos);
        }

        if (GameManager.manager.UIManager.radialMenuOpen)
            return;

        //Placing
        if (input.primaryPressed && !GameManager.manager.eventSystem.IsPointerOverGameObject())
        {
            ClearSelection();

            if (removing)
            {
                removing = false;
            }
            else
            {
                startSelection = SnapToGrid(mousePos);
                endSelection = startSelection;
                AddToSelection(mousePos);
                placing = true;
            }
        }

        if (placing && Vector3.Distance(lastSelectionPos, mousePos) > 0.5f)
        {
            if (!input.constrainSelection)
                AddToSelection(mousePos);
            endSelection = SnapToGrid(mousePos);
        }

        if (input.primaryReleased && placing)
        {
            CheckConstraints();
            AddToBuildList(selection, currentLayer, currentTile);
            ClearSelection();
            placing = false;
        }

        //Removing
        if (input.secondaryPressed && !GameManager.manager.eventSystem.IsPointerOverGameObject())
        {
            ClearSelection();
            if (placing)
            {
                placing = false;
            }
            else
            {
                startSelection = SnapToGrid(mousePos);
                endSelection = startSelection;
                AddToSelection(mousePos);
                removing = true;
            }
        }

        if (removing && Vector3.Distance(lastSelectionPos, mousePos) > 0.5f)
        {
            if (!input.constrainSelection)
                AddToSelection(mousePos);
            endSelection = SnapToGrid(mousePos);
        }

        if (input.secondaryReleased && removing)
        {
            CheckConstraints();
            AddToRemoveList(new List<Vector3Int>(selection), currentLayer);
            ClearSelection();
            removing = false;
        }
        CheckConstraints();

        //Rotating
        if (input.rotateTile != 0)
        {
            currentRotation += input.rotateTile;
            currentRotation = currentRotation % 4;
            if (currentRotation < 0)
                currentRotation = 3;
        }
    }

    void CheckConstraints()
    {
        //Constraining
        if (placing || removing)
        {
            if (currentLayer != 4 && tiles[currentTile].overrideConstraints)
            {
                if (tiles[currentTile].overrideAxis == ConstraintAxis.X)
                {
                    //If the selection distance is greater on the x
                    if (Mathf.Abs(startSelection.x - endSelection.x) > Mathf.Abs(startSelection.y - endSelection.y))
                    {
                        ConstrainSelection(ConstraintAxis.Y);
                    }
                    //If the selection distance is greater on the y
                    else
                    {
                        ConstrainSelection(ConstraintAxis.X);
                    }
                }
                else
                {
                    ConstrainSelection(tiles[currentTile].overrideAxis);
                }
                return;
            }

            switch (currentLayer)
            {
                //Floor
                case 0:
                    ConstrainSelection(ConstraintAxis.Rectangle);
                    break;

                //Walls
                case 1:
                    //If the selection distance is greater on the x
                    if (Mathf.Abs(startSelection.x - endSelection.x) > Mathf.Abs(startSelection.y - endSelection.y))
                    {
                        ConstrainSelection(ConstraintAxis.Y);
                    }
                    //If the selection distance is greater on the y
                    else
                    {
                        ConstrainSelection(ConstraintAxis.X);
                    }
                    break;
                //Objects
                case 2:
                    ConstrainSelection(ConstraintAxis.Single);
                    break;
                case 4:
                    ConstrainSelection(ConstraintAxis.Rectangle);
                    break;
                case 5:
                    ConstrainSelection(ConstraintAxis.Single);
                    break;
                case 6:
                    if (currentTile == 1) goto case 1;
                    ConstrainSelection(ConstraintAxis.Single);
                    break;
            }

        }
    }

    void AddToSelection(Vector3 tilePosition)
    {
        //Round the position to be the corner of a tile
        Vector3Int newSelection = SnapToGrid(tilePosition);

        //Add the position to the selection
        lastSelectionPos = newSelection + (Vector3.one * 0.5f);
        lastSelectionPos.z = -10;

        selectionTileMap.SetTile(newSelection, selectionTile);

        selection.Add(newSelection);
    }

    //Snap a float position to the corner of the grid
    public Vector3Int SnapToGrid(Vector3 rawPos)
    {
        Vector3Int snappedPos = new Vector3Int();

        snappedPos.x = (int)(Mathf.Floor(rawPos.x / tileSize) * tileSize);
        snappedPos.y = (int)(Mathf.Floor(rawPos.y / tileSize) * tileSize);
        snappedPos.z = 0;

        return snappedPos;
    }

    public void AddToBuildList(List<Vector3Int> positions, int layer, int tile, int rotation = 0, bool debug = false)
    {
        if (layer == 4)
        {
            AddToZone(new List<Vector3Int>(positions), tile);
            return;
        }

        if (layer == 6)
        {
            foreach (Vector3Int i in positions)
            {
                SetTile(i, layer, tile);
            }
            return;
        }

        for (int i = 0; i < positions.Count; i++)
        {
            Vector3Int pos = positions[i];
            Vector3Int localPosition = positions[i] - worldOrigin;

            if (GameManager.manager.playerData.money < tiles[tile].price)
                return;
            GameManager.manager.playerData.money -= tiles[tile].price;

            blueprintTileMaps[layer].SetTile(pos, tiles[tile].tiles[0]);

            tileData.tileRotation[localPosition.x, localPosition.y, layer] = rotation;

            //Add a new task to the list
            Task newTask = new Task
            {
                radius = 2,
                position = pos,
                type = buildTaskType,
                priority = 6,
                delay = 1,
            };

            availableTasks.Add(newTask);
            tileTypesToBuild[localPosition.x, localPosition.y] = tile;
            if (debug)
                Debug.Log("Added tile " + tile + " at " + positions + " to the build list.");
        }
    }

    public void AddToRemoveList(List<Vector3Int> positions, int layer, bool debug = false)
    {
        if (layer == 4)
        {
            RemoveFromZone(new List<Vector3Int>(positions), currentTile);
            return;
        }

        for (int i = 0; i < positions.Count; i++)
        {
            Vector3Int pos = positions[i];
            Vector3Int localPosition = positions[i] - worldOrigin;

            Task existingTask;

            if (GetTaskAtPosition(pos, out existingTask))
            {
                blueprintTileMaps[layer].SetTile(pos, null);
                GameManager.manager.playerData.money += tileTypesToBuild[localPosition.x, localPosition.y];
                availableTasks.Remove(existingTask);
                continue;
            }

            if (layer != 2 && layer != 5 && tilemaps[layer].GetTile(positions[i]) == null)
            {
                positions.RemoveAt(i);
                i--;
                if (i == positions.Count)
                    return;
                continue;
            }
            blueprintTileMaps[layer].SetTile(pos, removeTile);

            //Add a new task to the list
            Task newTask = new Task
            {
                radius = 2,
                position = pos,
                type = removeTaskType,
                priority = 6,
                delay = 1,
            };

            availableTasks.Add(newTask);
            if (debug)
                Debug.Log("Added tile at " + pos + " to the remove list.");
        }
    }

    public bool GetTaskAtPosition(Vector3Int position, out Task task)
    {
        foreach (Task t in availableTasks)
        {
            if (t.position == position)
            {
                task = t;
                return true;
            }
        }
        task = new Task();
        return false;
    }

    public void SetTile(Vector3Int position, int layer, int tile, int rotation = 0, bool debug = true, bool effects = true)
    {
        if (layer != 3 && layer != 5 && effects)
            Instantiate(effectPrefab, position, Quaternion.identity, objectTileGrid);

        Vector3Int localPosition = position - worldOrigin;

        if (layer < 6)
            blueprintTileMaps[layer].SetTile(position, null);

        tileData.tileTypes[localPosition.x, localPosition.y, layer] = tile;
        tileData.tileRotation[localPosition.x, localPosition.y, layer] = rotation;

        switch (layer)
        {
            //Floor
            case 0:
                tilemaps[layer].SetTile(position, floorTiles[tile].tiles
                    [Random.Range(0, floorTiles[tile].tiles.Length)]);

                tileData.pathfindingCosts[localPosition.x, localPosition.y] = floorTiles[tile].movementCost;
                tileData.pathfindingRatios[localPosition.x, localPosition.y] = floorTiles[tile].movementRatio;
                break;
            //Walls
            case 1:
                tilemaps[layer].SetTile(position, wallTiles[tile].tiles
                    [Random.Range(0, wallTiles[tile].tiles.Length)]);

                damageTileMap.SetTile(position, null);
                tileData.wallHealth[localPosition.x, localPosition.y] = wallTiles[tile].strength;

                //Debug.Log("Set health at " + localPosition + " AKA " + position + " to " + wallHealth[localPosition.x, localPosition.y]);
                break;
            //Objects
            case 2:
                if (objects[localPosition.x, localPosition.y] != null)
                    Destroy(objects[localPosition.x, localPosition.y].gameObject);
                objects[localPosition.x, localPosition.y] = Instantiate(objectTiles[tile].prefab, worldOrigin + localPosition + (Vector3.one * 0.5f), Quaternion.identity, objectTileGrid).GetComponent<SaveableObject>();

                //Use the rotation as an index for the sprite
                objects[localPosition.x, localPosition.y].GetComponent<SaveableObject>().spriteRenderer.sprite = ((Tile)objectTiles[tile].tiles[rotation]).sprite;
                break;
            //Background
            case 3:
                tilemaps[layer].SetTile(position, backgroundTiles[tile].tiles[Random.Range(0, backgroundTiles[tile].tiles.Length)]);

                if (tilemaps[0].GetTile(position) == null)
                {
                    tileData.pathfindingCosts[localPosition.x, localPosition.y] = 2;
                    tileData.pathfindingRatios[localPosition.x, localPosition.y] = 1.5f;
                }
                break;
            //Utilities
            case 5:
                if (utilityObjects[localPosition.x, localPosition.y] != null)
                    Destroy(utilityObjects[localPosition.x, localPosition.y].gameObject);

                GameObject obj = Instantiate(utilityTiles[tile].prefab, position + (Vector3.one * 0.5f), Quaternion.identity, utilityTileGrid);

                utilityObjects[localPosition.x, localPosition.y] = obj.GetComponent<SaveableObject>();
                break;
            //AI
            case 6:
                GameObject aiObj = Instantiate(placeableAI[tile].prefab, worldOrigin + localPosition + (Vector3.one * 0.5f), Quaternion.identity);
                break;
        }

        for (int i = 0; i < zones.Count; i++)
            zones[i].UpdateTooltip();

        if (debug)
            Debug.Log("Set tile " + tile + " on layer " + layer + " at " + position + ".");
    }

    public void RemoveTile(Vector3Int position, int layer)
    {
        if (layer != 3)
            Instantiate(effectPrefab, position, Quaternion.identity, objectTileGrid);

        if (layer <= 5)
            blueprintTileMaps[layer].SetTile(position, null);

        Vector3Int localPosition = position - worldOrigin;

        tileData.tileTypes[localPosition.x, localPosition.y, layer] = -1;

        switch (layer)
        {
            //Floor
            case 0:
                tileData.pathfindingCosts[localPosition.x, localPosition.y] = 2;
                tileData.pathfindingRatios[localPosition.x, localPosition.y] = 1.5f;
                break;
            //Walls
            case 1:
                tileData.wallHealth[localPosition.x, localPosition.y] = 0;
                damageTileMap.SetTile(position, null);
                break;
            //Objects
            case 2:
                if (objects[localPosition.x, localPosition.y] != null)
                {
                    Destroy(objects[localPosition.x, localPosition.y]);
                }
                break;
            //Background
            case 3:
                break;
            //Utilities
            case 5:
                if (utilityObjects[localPosition.x, localPosition.y] != null)
                {
                    Destroy(utilityObjects[localPosition.x, localPosition.y]);
                }
                return;

        }

        if (layer != 2)
            tilemaps[layer].SetTile(position, null);
        Debug.Log("Removed tile at: " + position);
    }

    public void AddToZone(List<Vector3Int> positions, int type)
    {
        int zone = zones.Count;
        bool addToExisting = false;
        for (int i = 0; i < positions.Count; i++)
        {
            for (int ii = 0; ii < zones.Count; ii++)
            {
                if (zones[ii].type == type && zones[ii].tiles.Contains(positions[i]))
                {
                    zone = ii;
                    addToExisting = true;
                    positions.Remove(positions[i]);
                }
            }
        }

        if (addToExisting)
        {
            foreach (Vector3Int pos in positions)
            {
                zoneTileMap.SetTile(pos, zoneTiles[(int)type].tiles[0]);
                zones[zone].tiles.Add(pos);
            }
        }
        else
        {
            foreach (Vector3Int pos in positions)
            {
                zoneTileMap.SetTile(pos, zoneTiles[(int)type].tiles[0]);
            }
            zones.Add(new Zone(type, type.ToString(), positions));
        }
        zones[zone].Assess(addToExisting);
        zones[zone].UpdateTooltip();
    }

    public void RemoveFromZone(List<Vector3Int> positions, int type)
    {
        for (int i = 0; i < positions.Count; i++)
        {
            for (int ii = 0; ii < zones.Count; ii++)
            {
                if (zones[ii].type == type && zones[ii].tiles.Contains(positions[i]))
                {
                    zones[ii].tiles.Remove(positions[i]);
                    zoneTileMap.SetTile(positions[i], null);
                    zones[ii].Assess();
                }

                if (zones[ii].tiles.Count < 1)
                {
                    zones.RemoveAt(ii);
                    ii--;
                }
            }
        }
    }

    public void DamageWall(Vector3Int position, int amount)
    {
        Vector3Int localPosition = position - worldOrigin;

        //int originaHealth = wallHealth[localPosition.x, localPosition.y];

        //Debug.Log("Damaged wall at " + position + " for " + amount + " damage. Original health was " + originaHealth);

        tileData.wallHealth[localPosition.x, localPosition.y] -= amount;
        if (tileData.wallHealth[localPosition.x, localPosition.y] <= 0)
        {
            RemoveTile(position, 1);
            return;
        }

        int currentHealth = tileData.wallHealth[localPosition.x, localPosition.y];
        int tileHealth = wallTiles[tileData.tileTypes[localPosition.x, localPosition.y, 1]].strength;

        int tile;
        float healthPercent = ((float)currentHealth / (float)tileHealth);

        //Debug.Log(currentHealth + ", " + tileHealth  + ", " + healthPercent);

        tile = damageTiles.Length - (int)(healthPercent * damageTiles.Length) - 1;

        if (tile >= 0)
            damageTileMap.SetTile(position, damageTiles[tile]);
    }

    void ClearSelection()
    {
        foreach (Vector3Int currentSelection in selection)
        {
            selectionTileMap.SetTile(currentSelection, null);
        }
        selection.Clear();
    }

    void ConstrainSelection(ConstraintAxis axis)
    {
        switch (axis)
        {
            case ConstraintAxis.X:

                Vector3Int startPos = startSelection,
                    endPos = endSelection;

                ClearSelection();

                //We don't want the size to be negative, so we flip it around if so
                if (startPos.y > endPos.y)
                {
                    startPos.y = endSelection.y;
                    endPos.y = startSelection.y;
                }

                for (int y = startPos.y; y <= endPos.y; y++)
                {
                    AddToSelection(new Vector3Int(startPos.x, y, 0));
                }
                break;

            case ConstraintAxis.Y:

                startPos = startSelection;


                endPos = endSelection;

                ClearSelection();

                //We don't want the size to be negative, so we flip it around if so
                if (startPos.x > endPos.x)
                {
                    startPos.x = endSelection.x;
                    endPos.x = startSelection.x;
                }

                for (int x = startPos.x; x <= endPos.x; x++)
                {
                    AddToSelection(new Vector3Int(x, startPos.y, 0));
                }
                break;

            case ConstraintAxis.Rectangle:

                startPos = startSelection;
                endPos = endSelection;

                ClearSelection();

                //We don't want the size to be negative, so we flip it around if so
                if (startPos.x > endPos.x)
                {
                    startPos.x = endSelection.x;
                    endPos.x = startSelection.x;
                }
                if (startPos.y > endPos.y)
                {
                    startPos.y = endSelection.y;
                    endPos.y = startSelection.y;
                }

                for (int x = startPos.x; x <= endPos.x; x++)
                {
                    for (int y = startPos.y; y <= endPos.y; y++)
                    {
                        AddToSelection(new Vector3Int(x, y, 0));
                    }
                }
                break;
            case ConstraintAxis.Single:
                ClearSelection();
                AddToSelection(endSelection);
                break;
        }

    }

    public void UpdateLayer(int layer)
    {
        switch (layer)
        {
            //Floor
            case 0:
                buildTaskType = TaskType.BuildFloor;
                removeTaskType = TaskType.RemoveFloor;
                tiles = floorTiles;
                goto default;
            //Walls
            case 1:
                buildTaskType = TaskType.BuildWall;
                removeTaskType = TaskType.RemoveWall;
                tiles = wallTiles;
                goto default;
            //Objects
            case 2:
                buildTaskType = TaskType.BuildObject;
                removeTaskType = TaskType.RemoveObject;
                tiles = objectTiles;
                goto default;
            //Background
            case 3:
                buildTaskType = TaskType.BuildBackground;
                removeTaskType = TaskType.RemoveBackground;
                tiles = backgroundTiles;
                goto default;
            //Zones
            case 4:
                break;
            //Utilities
            case 5:
                powerTileMap.gameObject.GetComponent<TilemapRenderer>().enabled = true;
                buildTaskType = TaskType.BuildUtility;
                removeTaskType = TaskType.RemoveUtility;
                tiles = utilityTiles;

                break;
            default:
                powerTileMap.gameObject.GetComponent<TilemapRenderer>().enabled = false;
                break;
        }
        currentLayer = layer;
    }

    public void AddWater(Vector3Int position, float amount)
    {
        tileData.waterLevels[position.x, position.y] += amount;

        int tile = (int)Mathf.Floor(Mathf.Clamp01(tileData.waterLevels[position.x, position.y]) * waterTiles.Length) - 1;

        if (tileData.waterLevels[position.x, position.y] >= 0.1f)
        {
            waterTileMap.SetTile(worldOrigin + position, waterTiles[tile]);
            if (!waterToSimulate.Contains(position))
            {
                waterToSimulate.Add(position);
            }
        }
        else
        {
            waterToSimulate.Remove(position);
            waterTileMap.SetTile(worldOrigin + position, null);
        }
    }

    void UpdateWater()
    {
        if (waterToSimulate.Count < 1)
            return;

        List<Vector3Int> waterSim = new List<Vector3Int>(waterToSimulate);

        foreach (Vector3 position in waterSim)
        {
            Vector3Int pos = SnapToGrid(position);
            if (tileData.waterLevels[pos.x, pos.y] > 0)
            {
                float waterLevel = tileData.waterLevels[pos.x, pos.y];
                float amount = waterLevel * Time.deltaTime;
                if (amount < 0.005f) return;
                List<Vector3Int> targets = new List<Vector3Int>();

                //Loop through all the surrounding tiles and check if they are available.
                for (int xx = -1; xx <= 1; xx++)
                {
                    for (int yy = -1; yy <= 1; yy++)
                    {
                        if (pos.x > 0 && pos.y > 0 && pos.x < worldSize.x - 1 && pos.y < worldSize.y - 1 &&
                         tileData.waterLevels[pos.x + xx, pos.y + yy] < waterLevel && tilemaps[1].GetTile(worldOrigin + new Vector3Int(pos.x + xx, pos.y + yy, 0)) == null)
                        {
                            targets.Add(new Vector3Int(pos.x + xx, pos.y + yy, 0));
                        }
                    }
                }

                //Loop through the available positions, and distribute 1 unit across them per second.
                foreach (Vector3Int target in targets)
                {
                    AddWater(target, amount / targets.Count);
                }
                AddWater(new Vector3Int(pos.x, pos.y, 0), -amount);
            }
        }
    }

    public TileData Save()
    {
        TileData data = new TileData();

        tileData.tasks = availableTasks.ConvertAll(x => (SerializableTask)x);

        tileData.waterToSimulate = waterToSimulate.ConvertAll(x => (SerializableVector3)(Vector3)x);

        tileData.zones = zones.ConvertAll(x => (SerializableZone)x);

        data = tileData;

        data.objects = new List<Data>();

        for (int x = 0; x < worldSize.x; x++)
        {
            for (int y = 0; y < worldSize.y; y++)
            {
                if (objects[x, y] != null)
                {
                    switch (objects[x, y].tag)
                    {
                        case "Door":
                            goto case "Electronic";

                        case "Interactable":
                            data.SaveObject(objects[x, y].GetComponent<Interactable>().Save(new InteractableData()));
                            break;

                        case "Electronic":
                            data.SaveObject(objects[x, y].GetComponent<Electronic>().Save(new ElectronicData()));
                            break;

                        default:
                            data.SaveObject(objects[x, y].GetComponent<SaveableObject>().Save(new ObjectData()));
                            break;
                    }
                }

                if(utilityObjects[x, y] != null)
                {
                    //EHHHHHHHH
                    // We have to use this to seperate them.
                    switch (utilityObjects[x, y].tag)
                    {
                        case "Wire":
                            data.SaveObject(utilityObjects[x, y].GetComponent<Wire>().Save(new WireData { useUtilityList = true }));
                            break;

                        case "Generator":
                            data.SaveObject(utilityObjects[x, y].GetComponent<Generator>().Save(new GeneratorData { useUtilityList = true }));
                            break;

                        case "Breaker":
                            data.SaveObject(utilityObjects[x, y].GetComponent<Breaker>().Save(new BreakerData { useUtilityList = true }));
                            break;
                    }
                }
            }
        }

        return data;
    }

    void InitWorld()
    {
        Debug.Log("Initializing world from settings");
        tileData = new TileData(GameManager.manager.worldSettings);

        worldOrigin = new Vector3Int();
        worldOrigin.x = (int)(worldSize.x * -0.5f);
        worldOrigin.y = (int)(worldSize.y * -0.5f);

        int xSize = (int)worldSize.x;
        int ySize = (int)worldSize.y;

        objects = new SaveableObject[xSize, ySize];
        utilityObjects = new SaveableObject[xSize, ySize];

        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                Vector2 pos = offset + new Vector2(x, y);
                float smallNoise = Mathf.PerlinNoise((pos.x * 0.1f), (pos.y * 0.1f));
                float bigNoise = Mathf.PerlinNoise((pos.x * 0.02f), (pos.y * 0.02f));

                int noise = (int)(((smallNoise + bigNoise) / 2) * biomes[biome].tiles.Length);

                SetTile(new Vector3Int(worldOrigin.x + x, worldOrigin.y + y, 0), 3, biomes[biome].tiles[noise], 0, false);
            }
        }
    }

    public void Load()
    {
        tileData = GameManager.manager.saveManager.SaveData.worldData.tileData;
        worldSize = GameManager.manager.worldSettings.size;
        worldOrigin = new Vector3Int();
        worldOrigin.x = (int)(worldSize.x * -0.5f);
        worldOrigin.y = (int)(worldSize.y * -0.5f);

        int xSize = (int)worldSize.x;
        int ySize = (int)worldSize.y;

        objects = new SaveableObject[xSize, ySize];
        utilityObjects = new SaveableObject[xSize, ySize];

        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                Vector3Int pos = new Vector3Int(worldOrigin.x + x, worldOrigin.y + y, 0);

                if (tileData.tileTypes[x, y, 0] >= 0)
                    SetTile(pos, 0, tileData.tileTypes[x, y, 0], 0, false, false);

                if (tileData.tileTypes[x, y, 1] >= 0)
                {
                    SetTile(pos, 1, tileData.tileTypes[x, y, 1], 0, false, false);
                    DamageWall(pos, 0);
                }

                if (tileData.tileTypes[x, y, 2] >= 0)
                    SetTile(pos, 2, tileData.tileTypes[x, y, 2], 0, false, false);

                if (tileData.tileTypes[x, y, 3] >= 0)
                    SetTile(pos, 3, tileData.tileTypes[x, y, 3], 0, false, false);

                if (tileData.tileTypes[x, y, 5] >= 0)
                    SetTile(pos, 5, tileData.tileTypes[x, y, 5], 0, true, false);

            }
        }

        List<Data> datas = tileData.objects;

        foreach (Data data in datas)
        {
            System.Type type;
            ObjectData dataToUse = data.Load(out type);
            Vector3Int pos = Vector3Int.FloorToInt(dataToUse.position) - worldOrigin;

            if (dataToUse.useUtilityList)
            {
                utilityObjects[pos.x, pos.y].GetComponent<SaveableObject>().Load(dataToUse);
            }
            else
            {
                objects[pos.x, pos.y].GetComponent<SaveableObject>().Load(dataToUse);
            }
        }

        //Load zones
        foreach (Zone zone in tileData.zones)
        {
            List<Vector3Int> tiles = zone.tiles.ConvertAll(x => Vector3Int.RoundToInt(x));

            AddToZone(tiles, zone.type);
        }

        availableTasks = tileData.tasks.ConvertAll(x => (Task)x);

        waterToSimulate = tileData.waterToSimulate.ConvertAll(x => Vector3Int.RoundToInt(x));
    }

    private void OnDrawGizmosSelected()
    {
        if (tileData.wallHealth != null)
        {
            for (int x = 0; x < worldSize.x; x++)
            {
                for (int y = 0; y < worldSize.y; y++)
                {
                    if (tileData.wallHealth[x, y] > 0)
                    {
                        //Gizmos.color = Color.white;
                        Gizmos.color = new Color(1, 0, 0, 1 - (float)tileData.wallHealth[x, y] / wallTiles[tileData.tileTypes[x, y, 1]].strength);
                        Gizmos.DrawCube(worldOrigin + new Vector3(x + 0.5f, y + 0.5f, 0), Vector3.one);
                        //Debug.Log("X: " + x + " Y: " + y + " Health: " + wallHealth[x, y]);
                    }
                }
            }
        }

    }
}
