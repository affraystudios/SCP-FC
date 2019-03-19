using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBuilder : AIBase
{
    public float buildDistance = 3;

    public int resources = 10;

    int[,] tileTypesToBuild;
    UnityEngine.Tilemaps.Tilemap blueprintTileMap;

    protected new void Start()
    {
        base.Start();
    }

    protected new void Update()
    {
        base.Update();

        if (!hasTask && tasks.Count < 1 && tileManager.availableTasks.Count > 0)
        {
            if (resources > 0)
            {
                GiveTarget();
            }
            else
            {
                FindResources();
            }
        }

        //If there are no resources, add an aquire task
        if (resources < 1)
        {
            Vector3Int taskPosition = new Vector3Int();
            bool foundTile = false;
            foreach (Zone zone in tileManager.zones)
            {
                if (zone.HasTile(tileManager.resourceType, out taskPosition))
                {
                    foundTile = true;
                    break;
                }
            }
            if (!foundTile)
            {

            }

            Task newTask = new Task
            {
                priority = 8,
                radius = 2,
                delay = 1,
                position = taskPosition,
                target = tileManager.objects[taskPosition.x, taskPosition.y].transform,
                type = TaskType.Aquire
            };
        }

    }

    void GiveTarget()
    {
        AddTask(tileManager.availableTasks[0]);
        tileManager.availableTasks.RemoveAt(0);
    }

    void FindResources()
    {

    }

    //
    //If any task is written here as well as in the base, it will be overridden
    //
    public override void ExecuteTask()
    {
        if (task == null)
            return;
        //base.ExecuteTask();

        Vector3Int pos = Vector3Int.FloorToInt(task.position);
        Vector3Int localPos = pos - tileManager.worldOrigin;

        Debug.Log("Executing task " + task.type);

        switch (task.type)
        {
            case TaskType.BuildFloor:
                resources--;
                tileManager.SetTile(pos, 0, tileManager.tileData.floorTileTypesToBuild[localPos.x, localPos.y]);
                break;
            case TaskType.BuildWall:
                resources--;
                tileManager.SetTile(pos, 1, tileManager.tileData.wallTileTypesToBuild[localPos.x, localPos.y]);
                break;
            case TaskType.BuildObject:
                resources--;
                tileManager.SetTile(pos, 2, tileManager.tileData.objectTileTypesToBuild[localPos.x, localPos.y], tileManager.tileData.tileRotation[localPos.x, localPos.y]);
                break;
            case TaskType.BuildBackground:
                resources--;
                tileManager.SetTile(pos, 3, tileManager.tileData.backgroundTileTypesToBuild[localPos.x, localPos.y]);
                break;
            case TaskType.BuildUtility:
                resources--;
                tileManager.SetTile(pos, 5, tileManager.tileData.utilityTileTypesToBuild[localPos.x, localPos.y]);
                break;

            case TaskType.RemoveFloor:
                tileManager.RemoveTile(pos, 0);
                break;
            case TaskType.RemoveWall:
                tileManager.RemoveTile(pos, 1);
                break;
            case TaskType.RemoveObject:
                tileManager.RemoveTile(pos, 2);
                break;
            case TaskType.RemoveBackground:
                tileManager.RemoveTile(pos, 3);
                break;
            case TaskType.RemoveUtility:
                tileManager.RemoveTile(pos, 5);
                break;

            case TaskType.Aquire:

                break;
            default:
                base.ExecuteTask();
                break;
        }
        delay = Time.time + task.delay;
        hasTask = false;
        task = null;
        occupied = false;
    }

    public override ObjectData Save(ObjectData dataToUse)
    {
        BuilderData data = (BuilderData)base.Save(dataToUse);
        data.buildDistance = buildDistance;

        return data;
    }

    public override void Load(ObjectData dataToUse)
    {
        base.Load(dataToUse);
        BuilderData data = (BuilderData)dataToUse;

        buildDistance = data.buildDistance;
    }
}
