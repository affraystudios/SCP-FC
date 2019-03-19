using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveableObject : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer spriteRenderer;
    protected Tooltip tooltip;

    [Header("Data")]
    public bool loaded;
    public int rotation;

    protected void Awake()
    {
        tooltip = GetComponent<Tooltip>();
    }

    protected void Start()
    {
        if (GameManager.manager.saveManager.loading && !loaded)
            Destroy(gameObject);
    }

    protected void Update()
    {
        
    }

    protected void OnEnable()
    {
        
    }

    protected void OnDisable()
    {
        
    }

    public virtual ObjectData Save (ObjectData dataToUse)
    {
        ObjectData data = dataToUse;

        data.position = transform.position;

        return data;
    }

    public virtual void Load (ObjectData dataToUse)
    {
        loaded = true;
        transform.position = dataToUse.position;
    }
}
