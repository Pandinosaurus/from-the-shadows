﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class PressurePlate : Activator
{
    public AudioClip sound;
    private SoundPlayer soundPlayer;

    private void Start()
    {
        soundPlayer = GetComponent<SoundPlayer>();       
        child = transform.Find("Child").gameObject;
        Off();
    }

    /// <summary>
    /// Activate when an Object is on the Plate or a Player
    /// </summary>
    public void OnTriggerEnter2D(Collider2D collision)
    {        
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Object"))
        {
            On(false);
            if (soundPlayer != null)
                soundPlayer.PlaySoundAtLocation(sound, 1f);
        }            
    }

    /// <summary>
    /// Deactivate when the Object or the Player leaves the Plate
    /// </summary>
    public void OnTriggerExit2D(Collider2D collision)
    {        
        if (!hasTimer && (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Object")))
        {
            Off();
        }
    }
}
