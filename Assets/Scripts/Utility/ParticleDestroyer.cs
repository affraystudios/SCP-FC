using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleDestroyer : MonoBehaviour {

    public float duration = 5;
    float time;

    void Start ()
    {
        time = Time.time;
    }

    void Update ()
    {
        if(Time.time > time + duration)
        {
            Destroy(gameObject);
        }
    }
}
