using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{

    TileManager tileManager;

    public GameObject explosionPrefab;

    public int damage = 100;
    public float radius = 10;

    public LayerMask mask;

    private void Start()
    {
        tileManager = GameManager.manager.tileManager;
    }

    private void Update()
    {
        if (Input.GetKeyDown("e"))
        {
            Explode();
        }
    }

    public void Explode()
    {
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        //Damaging characters and other objects
        foreach (Collider2D col in Physics2D.OverlapCircleAll(transform.position, radius, mask))
        {
            //Check if there is a direct line to the object
            RaycastHit2D hit = Physics2D.Linecast(transform.position, col.transform.position, mask);
            if (hit.collider == col)
            {
                Debug.DrawLine(transform.position, col.transform.position, Color.green, 2);
                float amount = damage * (1 - (hit.distance / radius));
                switch (col.tag)
                {
                    case "Character":
                        col.GetComponent<AIBase>().Damage((int)amount);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                Debug.DrawLine(transform.position, col.transform.position, Color.red, 2);
            }
        }
        Vector3Int vector = (Vector3Int.one * (int)radius);

        //Sweep from bottom left to top right, grabbing all the tiles that are inside the radius
        Vector3Int startPos = tileManager.SnapToGrid(transform.position) - vector - tileManager.worldOrigin;
        Vector3Int endPos = tileManager.SnapToGrid(transform.position) + vector - tileManager.worldOrigin;
        for (int y = startPos.y; y <= endPos.y; y++)
        {
            for (int x = startPos.x; x <= endPos.x; x++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                Vector3Int worldPos = tileManager.worldOrigin + pos;

                if (Vector3.Distance(transform.position, worldPos) < radius && tileManager.tilemaps[1].GetTile(worldPos) != null)
                {
                    Debug.DrawLine(transform.position, worldPos, Color.green, 2);
                    float amount = damage * (1 - (Vector3.Distance(transform.position, worldPos) / radius));
                    tileManager.DamageWall(worldPos, (int)amount);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
