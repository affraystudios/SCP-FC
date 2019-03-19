using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TaskType
{
    //Standard
    Avoid,
    Move,
    Enable,
    Disable,

    //Builder
    RemoveFloor,
    RemoveWall,
    RemoveObject,
    RemoveBackground,
    RemoveUtility,

    BuildFloor,
    BuildWall,
    BuildObject,
    BuildBackground,
    BuildUtility,

    Aquire,

    //Guard
    Kill,
    Detain,
    Escourt,
    Guard,
    Patrol,

    //D-boi
    Steal,

    //Heli
    Transport
}

//This is the base class for all AI characters. everything ranging from builder to guard to D-boi should inherit from this class
[RequireComponent(typeof(AIPathfinding))]
public class AIBase : SaveableEntity
{
    protected AIPathfinding pathfinding;
    protected AIManager manager;
    protected TileManager tileManager;

    [Header("Settings")]
    public int id;

    public float maxIdleTime;

    [Header("Data")]

    public string AIName = "D-Boi";
    public bool hasTask;
    protected float idleTimer;

    public List<Task> tasks = new List<Task> { };
    public Task task;

    public float loyalty;
    public int home;
    public AccessLevel accessLevel;
    public bool occupied;
    protected float delay;

    private new void Awake()
    {
        base.Awake();
        pathfinding = GetComponent<AIPathfinding>();
    }

    protected new void Start()
    {
        base.Start();
        manager = GameManager.manager.AIManager;
        tileManager = GameManager.manager.tileManager;
        manager.AIList.Add(this);
        AIName = manager.names.names[Random.Range(0, manager.names.names.Length - 1)];

        if (tooltip != null)
        {
            tooltip.SetProperty("Name", AIName.ToString());
            tooltip.SetProperty("Access Level", accessLevel.ToString());
            tooltip.SetProperty("Occupied", occupied.ToString());
            tooltip.SetProperty("Current Task", task.type.ToString());

            tooltip.SetCommand("Execute", Die);
        }
    }

    protected new void OnEnable()
    {
        base.OnEnable();
        if (manager != null)
        {
            manager.AIList.Add(this);
        }
    }

    protected new void OnDisable()
    {
        base.OnDisable();
        if (manager != null)
            manager.AIList.Remove(this);
    }

    protected new void Update()
    {
        base.Update();
        if (!occupied && tasks.Count > 0 && delay < Time.time)
        {
            GiveTask(tasks[0]);
            tasks.RemoveAt(0);
            idleTimer = 0;
        }
        else if (!occupied)
        {
            idleTimer += Time.deltaTime;
        }

        if (occupied && task != null && Vector3.Distance(transform.position, task.position) <= task.radius + 1)
        {
            ExecuteTask();
        }

        if (pathfinding.path.Count > 0)
        {
            Vector3Int relativePos = pathfinding.path[0] - tileManager.worldOrigin;
            if (tileManager.objects[relativePos.x, relativePos.y] != null)
            {
                SaveableObject _object = tileManager.objects[relativePos.x, relativePos.y];
                switch (_object.tag)
                {
                    case "Door":
                        if (_object.GetComponent<Door>().accessLevel <= accessLevel && !_object.GetComponent<Door>().on)
                        {
                            //Replace the current task with a door open task
                            Task openTask = new Task
                            {
                                type = TaskType.Enable,
                                priority = 10,
                                radius = 1,
                                delay = 0.5f,
                                target = _object.transform,
                                position = _object.transform.position
                            };
                            ReplaceTask(openTask);

                            //Add a door close task with a low priority
                            Task closeTask = new Task
                            {
                                type = TaskType.Disable,
                                priority = 7,
                                radius = 1,
                                delay = 0.5f,
                                target = _object.transform,

                                //Get the direction between the AI and the door, and use that to find the other side
                                position = _object.transform.position + ((_object.transform.position - transform.position).normalized * 2)
                            };
                            AddTask(closeTask);
                        }
                        break;
                }
            }
        }
    }

    public override void Die()
    {
        pathfinding.moving = false;
        pathfinding.enabled = false;
        base.Die();
    }

    public override void Damage(int amount)
    {
        base.Damage(amount);

        //Add a run away task

        //AddTask()
    }

    public void GiveTask(Task taskToUse)
    {
        hasTask = true;

        task = taskToUse;

        pathfinding.acceptableTargetDistance = 1;
        StartCoroutine(pathfinding.GeneratePath(tileManager.SnapToGrid(task.position), true));

        occupied = true;

        if (tooltip != null)
        {
            tooltip.SetProperty("Occupied", occupied.ToString());
            tooltip.SetProperty("Current Task", task.type.ToString());
        }
    }

    public void ReplaceTask(Task taskToReplace)
    {
        tasks.Insert(0, task);
        GiveTask(taskToReplace);
    }

    public void AddTask(Task taskToAdd)
    {
        //If the task's priority is larger than the current tasks priority, replace it
        if (hasTask && task != null && taskToAdd.priority > task.priority)
        {
            ReplaceTask(taskToAdd);
            return;
        }

        //Go through the list, checking if the task has a higher priority
        for (int i = 0; i < tasks.Count; i++)
        {
            if (taskToAdd.priority > tasks[i].priority)
            {
                tasks.Insert(i, taskToAdd);
                return;
            }
        }

        //If the priority is lower than any in the list, add it to the end
        tasks.Add(taskToAdd);
    }

    public virtual void ExecuteTask()
    {
        switch (task.type)
        {
            case TaskType.Enable:
                if (!task.target.gameObject.GetComponent<Interactable>().on)
                    task.target.gameObject.GetComponent<Interactable>().Interact();
                break;

            case TaskType.Disable:
                if (task.target.gameObject.GetComponent<Interactable>().on)
                    task.target.gameObject.GetComponent<Interactable>().Interact();
                break;
            default:
                break;
        }
        delay = Time.time + task.delay;
        hasTask = false;
        task = null;
        occupied = false;

        if (tooltip != null)
        {
            tooltip.SetProperty("Occupied", occupied.ToString());
            tooltip.SetProperty("Current Task", task.type.ToString());
        }
    }

    public override ObjectData Save(ObjectData dataToUse)
    {
        CharacterData data = (CharacterData)base.Save(dataToUse);

        data.name = AIName;

        if(task != null)
            data.task = task;
        data.tasks = tasks.ConvertAll(x => (SerializableTask)x);

        data.id = id;
        data.loyalty = loyalty;
        data.home = home;
        data.accessLevel = accessLevel;

        return data;
    }

    public override void Load(ObjectData dataToUse)
    {
        base.Load(dataToUse);

        CharacterData data = (CharacterData)dataToUse;

        AIName = data.name;

        if (data.task != null)
            task = data.task;
        tasks = data.tasks.ConvertAll(x => (Task)x);

        id = data.id;
        loyalty = data.loyalty;
        home = data.home;
        accessLevel = data.accessLevel;
    }
}

[System.Serializable]
public class Task
{
    public int priority = 0;
    public TaskType type;
    public float delay = 0.5f;

    public Transform target;
    public Vector3 position;

    public float radius = 2;

    public Task (TaskType taskType)
    {
        type = taskType;
    }

    public Task ()
    {

    }
}

[System.Serializable]
public class SerializableTask
{
    public int priority = 0;
    public TaskType type;
    public float delay = 0.5f;

    public SerializableVector3 target;
    public SerializableVector3 position;

    public float radius = 2;

    public static implicit operator SerializableTask (Task value)
    {
        SerializableTask task = new SerializableTask
        {
            priority = value.priority,
            type = value.type,
            target = value.target != null ? value.target.position : Vector3.zero,
            position = value.position,
            radius = value.radius,
            delay = value.delay
        };
        return task;
    }

    public static implicit operator Task (SerializableTask value)
    {
        Task task = new Task
        {
            priority = value.priority,
            type = value.type,
            target = null,
            position = value.position,
            radius = value.radius,
            delay = value.delay
        };
        return task;
    }
}
