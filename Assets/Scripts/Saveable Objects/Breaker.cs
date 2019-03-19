using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breaker : Switch
{
    public int takenPower;
    public bool blackout;
    public List<Electronic> connected;

    public void CheckPower ()
    {
        if (generator == null)
            return;
        if(generator.availablePower - takenPower < 0)
        {
            Blackout();
        }
        else if (blackout && generator.availablePower - takenPower >= 0)
        {
            EndBlackout();
        }
    }

    //If the objects ahead of the breaker would blackout the generator
    public void Blackout ()
    {
        blackout = true;
        foreach(Electronic electronic in connected)
        {
            //Only give power back to the generator if the electronic had power in the first place
            if (electronic.hasPower)
                generator.availablePower += electronic.requiredPower;
            electronic.hasPower = false;
        }
    }

    public void EndBlackout ()
    {
        blackout = false;
        foreach (Electronic electronic in connected)
        {
            if (!electronic.hasPower)
                generator.availablePower -= electronic.requiredPower;
            electronic.hasPower = true;
        }
    }

    public override ObjectData Save(ObjectData dataToUse)
    {
        BreakerData data = (BreakerData)base.Save(dataToUse);

        data.blackout = blackout;
        data.takenPower = takenPower;
        data.connected = new List<SerializableVector3>();

        //Convert the references into something serializable
        foreach (Electronic electronic in connected)
        {
            data.connected.Add(electronic.transform.position);
        }

        return data;
    }

    public override void Load(ObjectData dataToUse)
    {
        base.Load(dataToUse);

        BreakerData data = (BreakerData)dataToUse;

        blackout = data.blackout;
        takenPower = data.takenPower;
        connected = new List<Electronic>();

        //Grab the references from their positions
        for (int i = 0; i < data.connected.Count; i++)
        {
            Vector3Int pos = Vector3Int.RoundToInt(data.connected[i]);
            if (GameManager.manager.tileManager.utilityObjects[pos.x, pos.y] != null)
            {
                connected.Add(GameManager.manager.tileManager.utilityObjects[pos.x, pos.y].GetComponent<Electronic>());
            }
            else
            {
                connected.Add(GameManager.manager.tileManager.objects[pos.x, pos.y].GetComponent<Electronic>());
            }
        }
    }
}
