  ﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Tilemaps;

public class Interactable : SaveableEntity
{
    protected AudioSource audioSource;
    protected Animator anim;

    [Header("Settings")]
    [Header("Interactable")]

    public Sprite[] sprites;

    public Sprite[] frontSprites = new Sprite[2],
        backSprites = new Sprite[2],
        sideSprites = new Sprite[2];

    public AccessLevel accessLevel;

    public AudioClip enableSound,
    disableSound;

    [Header("Data")]
    public bool on = true;

    protected new void Awake()
    {
        base.Awake();
        audioSource = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
        Enable();
    }

    protected new void Start()
    {
        switch(rotation)
        {
            //Front
            case 0:
                sprites = frontSprites;
                break;
            //Left
            case 1:
                sprites = sideSprites;
                break;
            //Back
            case 2:
                sprites = backSprites;
                break;
            //Right
            case 3:
                sprites = sideSprites;
                break;
        }
        base.Start();

        if (tooltip != null)
        {
            tooltip.SetProperty("On", on.ToString());

            tooltip.SetCommand("Enable", Enable);
            tooltip.SetCommand("Disable", Disable);
            tooltip.SetCommand("Toggle", Interact);
        }
    }

    public void Interact()
    {
        if (!on)
            Enable();
        else
            Disable();
    }

    public virtual void Disable ()
    {
        on = false;
        anim.SetBool("On", on);
        spriteRenderer.sprite = sprites[0];
        PlaySound(disableSound);
        if (tooltip != null)
            tooltip.SetProperty("On", on.ToString());
    }

    public virtual void Enable ()
    {
        on = true;
        anim.SetBool("On", on);
        spriteRenderer.sprite = sprites[1];
        PlaySound(enableSound);
        if (tooltip != null)
            tooltip.SetProperty("On", on.ToString());
    }

    public override ObjectData Save(ObjectData dataToUse)
    {
        InteractableData data = (InteractableData)base.Save(dataToUse);
        data.on = on;

        return data;
    }

    public void Load(InteractableData data)
    {
        base.Load(data);
        on = data.on;
        if (on)
            Interact();
    }

    protected void PlaySound(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.pitch = UnityEngine.Random.Range(0.8f, 1.2f);
        audioSource.Play();
    }

    protected new void OnEnable()
    {
        base.OnEnable();
        Enable();
    }

    protected new void OnDisable()
    {
        base.OnDisable();
        Disable();
    }
}
