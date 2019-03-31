using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Electronic : Interactable
{
    [Header("Settings")]
    [Header("Electronic")]
    public int requiredPower;

    [Header("Data")]
    public Generator generator;
    public Breaker breaker;
    public List<Interactable> input;
    public int generatorDistance;

    public bool hasPower;

    protected new void Awake()
    {
        base.Awake();
    }

    new void Start()
    {
        base.Start();

        CheckForWires();

        if (tooltip != null)
        {
            tooltip.SetProperty("Required Power", requiredPower.ToString());
            tooltip.SetProperty("Has Power", hasPower.ToString());
        }
    }

    protected new void OnEnable()
    {
        CheckForWires();
        base.OnEnable();
        if (generator != null)
        {
            generator.connected.Add(this);
            generator.availablePower -= requiredPower;
            generator.CheckPower();

            if (tooltip != null)
                tooltip.SetProperty("Has Power", hasPower.ToString());
        }
    }

    protected new void OnDisable()
    {
        base.OnDisable();

        if (generator != null)
        {
            generator.connected.Remove(this);
            generator.availablePower += requiredPower;
            generator.CheckPower();

            if (tooltip != null)
                tooltip.SetProperty("Has Power", hasPower.ToString());
        }
    }

    public override void Disable()
    {
        if (!hasPower)
            return;
        base.Disable();
    }

    public override void Enable()
    {
        if (!hasPower)
            return;
        base.Enable();
    }

    public new void Update()
    {
        base.Update();
    }

    void CheckForWires()
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);

                Vector3Int localPos = (Vector3Int.FloorToInt(transform.position) + pos) - GameManager.manager.tileManager.worldOrigin;

                if (pos.magnitude <= 1 && pos.magnitude > 0)
                {
                    SaveableObject electronicObj = GameManager.manager.tileManager.utilityObjects[localPos.x, localPos.y];
                    if (electronicObj != null)
                    {
                        if (electronicObj.GetComponent<Electronic>() != null)
                        {
                            Electronic electronic = (Electronic)electronicObj;
                            Debug.Log(electronic.GetType());

                            if (!input.Contains(electronic))
                            {
                                //If the object is a wire, it will check it's connections to update other wires and/or add this wire to the list
                                if (electronic.GetType() == typeof(Wire) && electronic.generator != null)
                                {
                                    Debug.DrawRay(transform.position, pos, Color.blue, 1);
                                    ((Wire)electronic).CheckConnections();
                                    continue;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public void SetGenerator(Generator newGenerator)
    {
        //If we have an existing generator, give back the power to that one.
        //This should not happen often, but just in case we do not want someone to duplicate or lose power.
        if (generator != null)
        {
            generator.availablePower += requiredPower;
            generator.connected.Remove(this);
            generator.CheckPower();
        }

        //Remove power from the new generator
        generator = newGenerator;

        generator.availablePower -= requiredPower;
        generator.connected.Add(this);
        //Check with the breaker first if we have one to prevent a blackout
        if (breaker != null)
            breaker.CheckPower();
        generator.CheckPower();

        if (generator.on)
            hasPower = true;

        if (tooltip != null)
            tooltip.SetProperty("Has Power", hasPower.ToString());
    }

    public void SetBreaker(Breaker newBreaker)
    {
        //If we have an existing breaker, give back the power to that one.
        //This should not happen often, but just in case we do not want someone to duplicate or lose power.
        if (breaker != null)
        {
            breaker.takenPower -= requiredPower;
            breaker.connected.Remove(this);
            breaker.CheckPower();
        }

        breaker = newBreaker;

        //Remove power from the new breaker
        //Make the breaker check if it will blackout the generator
        breaker.takenPower += requiredPower;
        breaker.connected.Add(this);
        breaker.CheckPower();

        if (breaker.hasPower)
            hasPower = true;

        if (tooltip != null)
            tooltip.SetProperty("Has Power", hasPower.ToString());
    }

    public override ObjectData Save(ObjectData dataToUse)
    {
        ElectronicData data = (ElectronicData)base.Save(dataToUse);

        //Store the reference as a serializable position
        if(generator != null)
            data.generator = generator.transform.position;

        data.input = new List<SerializableVector3>();

        //Convert the references into something serializable
        foreach (Interactable interactable in input)
        {
            if (interactable != null)
                data.input.Add(interactable.transform.position);
        }

        if(breaker != null)
        data.breaker = breaker.transform.position;

        data.requiredPower = requiredPower;
        data.hasPower = hasPower;

        return data;
    }

    public override void Load(ObjectData dataToUse)
    {
        base.Load(dataToUse);

        ElectronicData data = (ElectronicData)dataToUse;

        //Grab the references from their positions
        if (data.generator != null)
        {

            Vector3Int generatorPos = Vector3Int.FloorToInt(data.generator) - GameManager.manager.tileManager.worldOrigin;
            generator = GameManager.manager.tileManager.utilityObjects[generatorPos.x, generatorPos.y].GetComponent<Generator>();
        }

        for (int i = 0; i < data.input.Count; i++)
        {
            Vector3Int inputPos = Vector3Int.FloorToInt(data.input[i]) - GameManager.manager.tileManager.worldOrigin;
            input.Add(GameManager.manager.tileManager.utilityObjects[inputPos.x, inputPos.y].GetComponent<Interactable>());
        }

        if (data.breaker != null)
        {
            Vector3Int breakerPos = Vector3Int.FloorToInt(data.breaker) - GameManager.manager.tileManager.worldOrigin;
            breaker = GameManager.manager.tileManager.utilityObjects[breakerPos.x, breakerPos.y].GetComponent<Breaker>();
        }

        requiredPower = data.requiredPower;
        hasPower = data.hasPower;

        if (on)
            Enable();
        else
            Disable();
    }

    private void OnDrawGizmos()
    {
        if (hasPower && generator != null && !generator.blackout)
        {
            //Greeney blue
            Gizmos.color = new Color(0, 0.5f, 1, 0.1f + (requiredPower / generator.totalPower));
            Gizmos.DrawCube(transform.position, Vector3.one);

            if (input != null)
            {
                Gizmos.color = new Color(0, 0.5f, 1, 1);
                foreach (Interactable inputObj in input)
                {
                    if(inputObj != null)
                        Gizmos.DrawLine(transform.position, inputObj.transform.position);
                }
            }
        }
    }
}
