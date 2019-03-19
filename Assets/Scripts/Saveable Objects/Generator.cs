using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : Interactable
{
    public bool blackout;

    public int totalPower,
        availablePower;

    public List<Electronic> connected;

    new void Start()
    {
        base.Start();

        if (tooltip != null)
        {
            tooltip.SetProperty("Total Power Capacity", totalPower.ToString());
            tooltip.SetProperty("Available Power Capacity", availablePower.ToString());
            tooltip.SetProperty("Blackout In Process", blackout.ToString());
        }
    }

    //Check if the generator is being overdrawn
    public void CheckPower ()
    {
        if(availablePower < 0)
        {
            Blackout();
        }
        else if (availablePower > totalPower)
          availablePower = totalPower;

        if (tooltip != null)
        {
            tooltip.SetProperty("Total Power Capacity", totalPower.ToString());
            tooltip.SetProperty("Available Power Capacity", availablePower.ToString());
            tooltip.SetProperty("Blackout In Process", blackout.ToString());
        }
    }

    public void Blackout ()
    {
        //Disable this
        Disable();

        blackout = true;
    }

    public override void Enable()
    {
        base.Enable();

        blackout = false;

        foreach (Electronic electronic in connected)
        {
            if (electronic == null)
            {
                //connectedElectronics.Remove(electronic);
                continue;
            }

            electronic.hasPower = true;

            if (electronic.GetComponent<Tooltip>() != null)
                electronic.GetComponent<Tooltip>().SetProperty("Has Power", electronic.hasPower.ToString());

            //electronic.Enable();
        }
    }

    public override void Disable()
    {
        base.Disable();

        if (blackout)
            return;

        foreach (Electronic electronic in connected)
        {
            if (electronic == null)
            {
                //connectedElectronics.Remove(electronic);
                continue;
            }

            electronic.hasPower = false;

            if (electronic.GetComponent<Tooltip>() != null)
                electronic.GetComponent<Tooltip>().SetProperty("Has Power", electronic.hasPower.ToString());

            //electronic.Disable();
        }

    }

    public override ObjectData Save(ObjectData dataToUse)
    {
        GeneratorData data = (GeneratorData)base.Save(dataToUse);

        data.blackout = blackout;
        data.totalPower = totalPower;
        data.availablePower = availablePower;
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

        GeneratorData data = (GeneratorData)dataToUse;

        blackout = data.blackout;
        totalPower = data.totalPower;
        availablePower = data.availablePower;
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
