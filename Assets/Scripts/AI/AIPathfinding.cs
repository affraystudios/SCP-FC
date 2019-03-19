using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class AIPathfinding : MonoBehaviour
{
    [System.Serializable]
    public struct Tile
    {
        public Vector3Int position;
        public Vector3Int parent;
        public float score;
        public float distanceFromStart;
        public float distanceToEnd;

        public Tile(Vector3Int tilePosition, Vector3Int tileParent, float tileDistanceFromStart, float tileDistanceToEnd)
        {
            position = tilePosition;
            parent = tileParent;
            distanceFromStart = tileDistanceFromStart;
            distanceToEnd = tileDistanceToEnd;
            score = distanceFromStart + distanceToEnd;
        }

        public Tile(Vector3Int tilePosition, Vector3Int tileParent)
        {
            position = tilePosition;
            parent = tileParent;
            distanceFromStart = 0;
            distanceToEnd = 0;
            score = distanceFromStart + distanceToEnd;
        }
    }

    AIBase AIController;
    TileManager tileManager;

    [Header("References")]
    public Transform target;

    [Header("Settings")]
    public int maxScore = 300;
    public Vector2Int gridSize = Vector2Int.one * 50;
    public Vector3Int origin;
    public bool useCorners,
        waitPerFrame;
    public float acceptableTargetDistance;
    public float speed = 1, maxAcceptableDistance = 0.05f;
    public bool flying;

    [Header("Data")]
    public List<Vector3Int> availableTiles;
    public List<Vector3Int> closedTiles;
    public Tile[,] tiles;
    public List<Vector3Int> path;
    public Tile destinationTile;
    Vector3Int targetPosition;

    public bool moving;

    private void Awake()
    {
        AIController = GetComponent<AIBase>();
    }

    private void Start()
    {
        tileManager = GameManager.manager.tileManager;
        //GeneratePath(tileManager.SnapToGrid(target.position));
    }

    private void Update()
    {
        //If we have a path to walk and we want to move
        if (moving && path.Count == 0)
        {
            //Debug.Log("AI '" + gameObject.name + "' has reached the end of the path.");
            moving = false;
        }
        else if (moving && path.Count > 0)
        {
            transform.position = Vector3.MoveTowards(transform.position, path[0] + (Vector3.one * 0.5f), speed * Time.deltaTime);

            //See if we are close enough to the next one to move to it
            if (Vector3.Distance(transform.position, path[0] + (Vector3.one * 0.5f)) < maxAcceptableDistance)
            {
                path.RemoveAt(0);
                if (path.Count > 0 && tileManager.tilemaps[1].GetTile(path[0]) != null)
                {
                    moving = false;
                    StartCoroutine(GeneratePath(tileManager.SnapToGrid(targetPosition), true));
                    Debug.LogWarning("AI '" + gameObject.name + "' seeking new route.");
                }
            }
        }
    }

    public IEnumerator GeneratePath(Vector3Int destination, bool startMoving = false)
    {
        closedTiles = new List<Vector3Int>();
        availableTiles = new List<Vector3Int>();
        tiles = new Tile[gridSize.x, gridSize.y];
        origin = tileManager.SnapToGrid(transform.position - new Vector3Int(gridSize.y / 2, gridSize.y / 2, 0));

        targetPosition = destination;
        destinationTile.position = destination - origin;

        Vector3Int startingPosition = new Vector3Int(gridSize.y / 2, gridSize.y / 2, 0);
        Tile startTile = new Tile(startingPosition, startingPosition, 0, (int)(destinationTile.position - (startingPosition)).magnitude);

        tiles[startingPosition.x, startingPosition.y] = startTile;
        closedTiles.Add(startingPosition);

        Tile currentTile = startTile;
        bool foundPath = false;
        bool stuck = false;
        while (!foundPath && !stuck)
        {
            if (currentTile.score > maxScore)
            {
                stuck = true;
                Debug.LogWarning("AI '" + gameObject.name + "' was stuck. Minimum path score was too high.");
                continue;
            }
            //Sweep from bottom left to top right, grabbing all the tiles that are walkable
            Vector3Int startPos = currentTile.position + Vector3Int.left + Vector3Int.down;
            Vector3Int endPos = currentTile.position + Vector3Int.right + Vector3Int.up;

            for (int y = startPos.y; y <= endPos.y && y < gridSize.y && y > 0; y++)
            {
                for (int x = startPos.x; x <= endPos.x && x < gridSize.x && x > 0; x++)
                {
                    Vector3Int pos = new Vector3Int(x, y, 0);
                    if ((pos - currentTile.position).magnitude == 1 || useCorners && (pos - currentTile.position).magnitude > 0)
                    {
                        Tile tile = new Tile(new Vector3Int(x, y, 0), currentTile.position);

                        float movementCost = 1;
                        float movementRatio = 1;

                        if (!flying)
                        {
                            //Get the movement cost of the tile beneath
                            Vector3Int tileMapPos = origin + tile.position - tileManager.worldOrigin;
                            movementCost = tileManager.tileData.pathfindingCosts[tileMapPos.x, tileMapPos.y];
                            movementRatio = tileManager.tileData.pathfindingRatios[tileMapPos.x, tileMapPos.y];

                            //If there is an object on this tile
                            if (tileManager.tilemaps[1].GetTile(origin + tile.position) != null)
                            {
                                movementCost += tileManager.tileData.wallHealth[tileMapPos.x, tileMapPos.y] + 10;
                            }

                            if (tileManager.objects[x, y] != null)
                            {
                                SaveableObject _object = tileManager.objects[x, y];
                                switch (_object.tag)
                                {
                                    case "Door":
                                        if (!_object.GetComponent<Interactable>().on)
                                        {
                                            if (_object.GetComponent<Door>().accessLevel <= AIController.accessLevel)
                                            {
                                                movementCost += 1;
                                            }
                                            else
                                            {
                                                movementCost += 100;
                                            }
                                        }
                                        break;
                                    default:
                                        movementCost += 1;
                                        break;
                                }
                            }
                        }

                        //Score should be the distance it took to get there + the shortest distance to the goal
                        tile.distanceFromStart = currentTile.distanceFromStart + ((pos - currentTile.position).magnitude * movementCost);
                        tile.distanceToEnd = (int)(destinationTile.position - (tile.position)).magnitude * movementCost * movementRatio;
                        tile.score = tile.distanceToEnd + tile.distanceFromStart;

                        //If the tile is the same as the destination; 
                        if (Vector3Int.Distance(tile.position, destinationTile.position) <= acceptableTargetDistance)
                        {
                            destinationTile.parent = tile.position;
                            foundPath = true;
                        }

                        //Check if this tile is not in the closed list, and is empty
                        if (!closedTiles.Contains(tile.position))
                        {

                            //Check if the tile is already in the open list
                            if (availableTiles.Contains(tile.position))
                            {
                                Tile t = tiles[tile.position.x, tile.position.y];
                                if (tile.score < t.score)
                                {
                                    tiles[tile.position.x, tile.position.y] = tile;
                                }
                            }
                            else
                            {
                                tiles[tile.position.x, tile.position.y] = tile;
                                //Add it to the list of positions we can use
                                availableTiles.Add(tile.position);
                            }
                        }
                    }
                }
            }

            if (waitPerFrame)
                yield return new WaitForEndOfFrame();

            //Go through the different positions in the open list, and deside which to use
            closedTiles.Add(currentTile.position);
            availableTiles.Remove(currentTile.position);

            if (availableTiles.Count > 0)
            {
                Vector3Int nextTile = availableTiles[0];
                foreach (Vector3Int tilePos in availableTiles)
                {
                    if (tiles[tilePos.x, tilePos.y].score < tiles[nextTile.x, nextTile.y].score)
                    {
                        nextTile = tilePos;
                    }
                }
                currentTile = tiles[nextTile.x, nextTile.y];
            }
            else
            {
                stuck = true;
                StartCoroutine(GeneratePath(targetPosition, true));
                Debug.LogWarning("AI '" + gameObject.name + "' was stuck.");
                continue;
            }
        }

        path = new List<Vector3Int>();

        Vector3Int position = destinationTile.parent;

        //Add each position to the path for later
        while (position != tiles[position.x, position.y].parent)
        {
            path.Insert(0, origin + position);
            position = tiles[position.x, position.y].parent;
        }

        //If we are told to start moving after completing, start moving
        moving = startMoving;
    }

    private void OnDrawGizmosSelected()
    {
        Color color = Color.red;
        if (closedTiles != null)
        {
            color.a = 0.3f;
            Gizmos.color = color;
            foreach (Vector3Int pos in closedTiles)
            {
                Gizmos.DrawCube(origin + pos + (Vector3.one * 0.5f), Vector3.one);
            }
        }

        if (availableTiles != null)
        {
            color = Color.green;
            color.a = 0.3f;
            Gizmos.color = color;
            foreach (Vector3Int pos in availableTiles)
            {
                Gizmos.DrawCube(origin + pos + (Vector3.one * 0.5f), Vector3.one);
            }
        }

        color = Color.yellow;
        color.a = 0.3f;
        Gizmos.color = color;
        Gizmos.DrawCube(origin + destinationTile.position + (Vector3.one * 0.5f), Vector3.one);

        for (int i = 0; i < path.Count - 1; i++)
        {
            Debug.DrawLine(path[i] + (Vector3.one * 0.5f), path[i + 1] + (Vector3.one * 0.5f), Color.green);
        }
        if (path.Count > 0)
            Debug.DrawLine(transform.position, path[0]);
    }
}