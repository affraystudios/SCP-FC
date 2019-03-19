using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializableVector3
{
    public float[] vector = new float[3];

    public SerializableVector3(float x, float y, float z)
    {
        vector[0] = x;
        vector[1] = y;
        vector[2] = z;
    }

    public static implicit operator SerializableVector3(Vector3 value)
    {
        return new SerializableVector3(value.x, value.y, value.z);
    }

    public static implicit operator Vector3(SerializableVector3 value)
    {
        return new Vector3(value.vector[0], value.vector[1], value.vector[2]);
    }
}


[System.Serializable]
public class SerializableVector2
{
    public float[] vector = new float[2];

    public SerializableVector2(float x, float y)
    {
        vector[0] = x;
        vector[1] = y;
    }

    public static implicit operator SerializableVector2(Vector2 value)
    {
        return new SerializableVector2(value.x, value.y);
    }

    public static implicit operator Vector2(SerializableVector2 value)
    {
        return new Vector2(value.vector[0], value.vector[1]);
    }
}