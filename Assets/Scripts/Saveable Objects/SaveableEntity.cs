using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveableEntity : SaveableObject
{
    public bool dead;
    public int health = 100;

    protected new void Start ()
    {
        base.Start();
        if (tooltip != null)
        {
            tooltip.SetProperty("Health", health.ToString());
            tooltip.SetProperty("Dead", dead.ToString());
        }
    }

    public virtual void Damage(int amount)
    {
        if(tooltip != null)
        {
            tooltip.SetProperty("Health", health.ToString());
        }
        health -= amount;
        if (health <= 0)
        {
            Die();
        }
    }

    public virtual void Die()
    {
        this.enabled = false;
        dead = true;
        if (tooltip != null)
        {
            tooltip.SetProperty("Health", 0.ToString());
            tooltip.SetProperty("Dead", dead.ToString());
        }
    }

    public override ObjectData Save(ObjectData dataToUse)
    {
        EntityData data = (EntityData)base.Save(dataToUse);
        data.health = health;
        data.dead = dead;

        return data;
    }

    public override void Load(ObjectData dataToUse)
    {
        base.Load(dataToUse);

        EntityData data = (EntityData)dataToUse;

        health = data.health;
    }
}
