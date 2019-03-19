using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Electronic
{
    new Collider2D collider;

    new private void Awake()
    {
        base.Awake();
        collider = GetComponent<Collider2D>();
    }

    public override void Disable()
    {
        base.Disable();

        collider.enabled = true;
    }

    public override void Enable()
    {
        base.Enable();

        if(collider != null)
            collider.enabled = false;
    }
}
