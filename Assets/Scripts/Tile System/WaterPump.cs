using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterPump : MonoBehaviour {

    public float amount;

    private void Update()
    {
        Vector3Int position = GameManager.manager.tileManager.SnapToGrid(transform.position) - GameManager.manager.tileManager.worldOrigin;

        GameManager.manager.tileManager.AddWater(position, amount * Time.deltaTime);
    }

}
