﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Lever : Activator, IResetable
{
    public List<AudioClip> soundsOn;
    public List<AudioClip> soundsOff;
    public AudioClip soundTimer;

    public Material baseMat;
    public Material timerMat;

    public bool hasTimer;
    public float timer;
    public bool activeAtStart;
    public bool doNotReset = false;

    public bool withAnimation = false;

    private bool canPlayer1Activate = false;
    private bool canPlayer2Activate = false;
    private AudioSource audioSource;
    private GameObject child;
    private bool isMute = true;

    Material[] leverMaterials;
    Material[] leverMaterialsTimer;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        child = transform.Find("Child").gameObject;
        active = activeAtStart;

        isMute = true;

        leverMaterials = child.transform.GetChild(0).GetComponent<MeshRenderer>().materials;
        leverMaterialsTimer = child.transform.GetChild(0).GetComponent<MeshRenderer>().materials;
        leverMaterialsTimer[1] = timerMat;

        if (activeAtStart)
        {
            if (withAnimation)
            {
                GetComponentInChildren<Animator>().SetBool("OFF", false);
            }
        }
        else
        {
            if (withAnimation)
            {
                GetComponentInChildren<Animator>().SetBool("OFF", true);
            }
        }

        isMute = false;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            int idPlayer = collision.gameObject.GetComponent<PlayerInput>().id;
            if (idPlayer == 1)
                canPlayer1Activate = true;
            else if (idPlayer == 2)
                canPlayer2Activate = true;
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            int idPlayer = collision.gameObject.GetComponent<PlayerInput>().id;
            if (idPlayer == 1)
                canPlayer1Activate = false;
            else if (idPlayer == 2)
                canPlayer2Activate = false;
        }
    }

    /// <summary>
    /// Activate or deactivate the lever when a player interracts if he is in the collider
    /// </summary>
    public void Update()
    {
        if (canPlayer1Activate && InputManager.GetActionPressed(1, InputAction.Interact))//Input.GetButtonDown("X_1"))
        {
            if (!active)
                On(false);
            else
                Off();
        }
        if (canPlayer2Activate && InputManager.GetActionPressed(2, InputAction.Interact))//Input.GetButtonDown("X_2"))
        {
            if (!active)
                On(false);
            else
                Off();
        }
    }

    /// <summary>
    /// Activate the lever
    /// </summary>
    /// <param name="ignoreTimer"> Ignore timer reset (when the lever is active at the beginning of the level</param>
    protected void On(bool ignoreTimer)
    {
        if (TryActivate != null)
        {
            if (withAnimation)
            {
                GetComponentInChildren<Animator>().SetBool("OFF", false);
            }

            active = true;
            TryActivate();
            if (audioSource != null && !isMute && soundsOn.Count > 0)
                audioSource.PlayOneShot(soundsOn[Random.Range(0, soundsOn.Count - 1)]);

            CancelInvoke();
            StopCoroutine("Flash");
            if (hasTimer && !ignoreTimer)
            {
                if (audioSource != null && !isMute && soundTimer != null)
                {
                    audioSource.clip = soundTimer;
                    audioSource.loop = true;
                    audioSource.Play();
                }
                Invoke("Off", timer);
                StartCoroutine("Flash");
            }
        }
    }

    /// <summary>
    /// Deactivate the lever
    /// </summary>
    protected void Off()
    {
        if (TryDeactivate != null)
        {
            //RENDYYYYYYYYYYYYYYYYYYYY
            if (withAnimation)
            {
                GetComponentInChildren<Animator>().SetBool("OFF", true);
            }

            active = false;

            StopCoroutine("Flash");

            if(hasTimer)
            {
                child.transform.GetChild(1).GetComponent<MeshRenderer>().material = baseMat;
                child.transform.GetChild(0).GetComponent<MeshRenderer>().materials = leverMaterials;
            }

            TryDeactivate();
            if (audioSource != null)
            {
                audioSource.Stop();
                audioSource.loop = false;
                audioSource.clip = null;
            }
            if (audioSource != null && !isMute && soundsOff.Count > 0)
                audioSource.PlayOneShot(soundsOff[Random.Range(0, soundsOff.Count - 1)]);

        }
    }

    public void Reset()
    {
        if (!doNotReset)
        {
            isMute = true;
            if (active && !activeAtStart)
                Off();
            else if (!active && activeAtStart)
                On(true);
            isMute = false;

            canPlayer1Activate = false;
            canPlayer2Activate = false;
        }
    }

    private IEnumerator Flash()
    {
        if (child != null)
        {
            while (true)
            {
                child.transform.GetChild(1).GetComponent<MeshRenderer>().material = baseMat;
                child.transform.GetChild(0).GetComponent<MeshRenderer>().materials = leverMaterials;

                yield return new WaitForSeconds(0.2f);

                child.transform.GetChild(1).GetComponent<MeshRenderer>().material = timerMat;
                child.transform.GetChild(0).GetComponent<MeshRenderer>().materials = leverMaterialsTimer;

                yield return new WaitForSeconds(0.2f);
            }
        }
    }

}
