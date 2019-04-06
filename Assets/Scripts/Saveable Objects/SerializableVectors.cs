using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializableVector3
{
    public float x,
        y,
        z;

    public SerializableVector3(float _x, float _y, float _z)
    {
        x = _x;
        y = _y;
        z = _z;
    }

    public static implicit operator SerializableVector3(Vector3 value)
    {
        return new SerializableVector3(value.x, value.y, value.z);
    }

    public static implicit operator Vector3(SerializableVector3 value)
    {
        return new Vector3(value.x, value.y, value.z);
    }
}


[System.Serializable]
public class SerializableVector2
{
    public float x,
        y;

    public SerializableVector2(float _x, float _y)
    {
        x = _x;
        x = _y;
    }

    public static implicit operator SerializableVector2(Vector2 value)
    {
        return new SerializableVector2(value.x, value.y);
    }

    public static implicit operator Vector2(SerializableVector2 value)
    {
        return new Vector2(value.x, value.y);
    }
}