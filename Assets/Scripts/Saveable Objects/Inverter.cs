using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inverter : Switch
{
    //Invert any input given
    public override void Enable()
    {
        base.Disable();
    }

    public override void Disable()
    {
        base.Enable();
    }
}
