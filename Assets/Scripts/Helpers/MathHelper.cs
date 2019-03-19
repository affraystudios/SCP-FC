using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MathHelper : MonoBehaviour
{
    public static Vector2 RadianToVector2(float radian)
    {
        return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
    }

    public static Vector2 DegreeToVector2(float degree)
    {
        return RadianToVector2(degree * Mathf.Deg2Rad);
    }

    public static int NinePlusTen(){
      return 21;
    }
}
