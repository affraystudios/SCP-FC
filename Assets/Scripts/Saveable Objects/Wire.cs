using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wire : Electronic
{
    public int distanceToGenerator;
    public List<Electronic> output;

    public new void Start()
    {
        base.Start();
        CheckConnections();
    }

    public override void Enable()
    {
        base.Enable();

        foreach(Electronic electronic in output)
        {
            electronic.Enable();
        }

        CheckConnections();
    }

    public override void Disable()
    {
        base.Disable();

        foreach (Electronic electronic in output)
        {
            electronic.Disable();
        }
    }

    private new void OnEnable()
    {
        Enable();
    }

    public void CheckConnections()
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);

                Vector3Int localPos = (Vector3Int.FloorToInt(transform.position) + pos) - GameManager.manager.tileManager.worldOrigin;

                if (pos.magnitude <= 1)
                {
                    //Check the Utility at this position
                    SaveableObject electronicObj = GameManager.manager.tileManager.utilityObjects[localPos.x, localPos.y];

                    if (electronicObj != null && electronicObj != this)
                    {
                        CheckObject(electronicObj, pos);
                    }

                    //Check the Object at this position
                    if (electronicObj != GameManager.manager.tileManager.objects[localPos.x, localPos.y])
                    {
                        electronicObj = GameManager.manager.tileManager.objects[localPos.x, localPos.y];
                        if (electronicObj != null && electronicObj != this)
                        {
                            CheckObject(electronicObj, pos);
                        }
                    }
                }
            }
        }
    }

    void CheckObject(SaveableObject electronicObj, Vector3Int pos)
    {

        //If the object is a generator and we have none, use the object
        Generator gen = electronicObj.GetComponent<Generator>();
        if (gen != null)
        {
            if(!input.Contains(gen))
                input.Add(gen);
            SetGenerator(gen);

            Debug.DrawRay(transform.position, pos, Color.green, 1);
        }
        else
        {
            Electronic electronic = (Electronic)electronicObj;

            //If the object is a wire, it will check it's connections to update other wires and/or add this wire to the list
            if (electronic.GetComponent<Wire>() != null && electronic.generator != null && electronic.generator != generator)
                //|| electronic.GetComponent<Wire>() != null && electronic.generator != null && electronic.generatorDistance <= generatorDistance)
            {
                ((Wire)electronic).CheckConnections();

                Debug.DrawRay(transform.position, pos, Color.blue, 1);
                return;
            }

            if (generator == null || input.Contains(electronic))
                return;

            //if the object has no generator and isn't already connected to our generator,
            if (electronic.generator == null || electronic.generator != generator)
            {
                electronic.input.Add(this);
                
                //Set the objects breaker to ours
                if(breaker != null)
                electronic.SetBreaker(breaker);

                //Set the objects distance
                electronic.generatorDistance = generatorDistance + 1;
                electronic.SetGenerator(generator);

                //Make sure the objects current is consistant
                if (on)
                {
                    electronic.Enable();
                }
                else
                {
                    electronic.Disable();
                }

                if (!output.Contains(electronic))
                    output.Add(electronic);

                Debug.DrawRay(transform.position, pos, Color.green, 1);
            }

            //This is to reconnect chains
            if(electronic.GetComponent<Wire>() != null)
            {
                ((Wire)electronic).CheckConnections();
            }

        }
    }

    public override ObjectData Save(ObjectData dataToUse)
    {
        WireData data = (WireData)base.Save(dataToUse);

        //Convert the references into something serializable
        data.output = new List<SerializableVector3>();
        for (int i = 0; i < output.Count; i++)
        {
            if(output[i] != null)
            data.output.Add(output[i].transform.position);
        }

        return data;
    }

    public override void Load(ObjectData dataToUse)
    {
        base.Load(dataToUse);
        WireData data = (WireData)dataToUse;

        //Grab the references from their positions
        output = new List<Electronic>();
        for (int i = 0; i < data.output.Count; i++)
        {
            Vector3Int pos = Vector3Int.FloorToInt(data.output[i]) - GameManager.manager.tileManager.worldOrigin;

            if (GameManager.manager.tileManager.utilityObjects[pos.x, pos.y] != null)
            {
                output.Add(GameManager.manager.tileManager.utilityObjects[pos.x, pos.y].GetComponent<Electronic>());
            }
            else if(GameManager.manager.tileManager.objects[pos.x, pos.y] != null)
            {
                output.Add(GameManager.manager.tileManager.objects[pos.x, pos.y].GetComponent<Electronic>());
            }
        }
    }

}
