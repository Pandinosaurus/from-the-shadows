﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torch : ActivatorListener, IResetable
{

    public GameObject lightSource;
    public GameObject pointLight;
    public AudioClip soundOn;
    public AudioClip soundOff;
    public bool activeAtStart;
    public float lightRadius;

    private bool isMute = true;
    public bool active;
    private AudioSource audioSource;
    private float targetRadius = 0.01f;

    private void Awake()
    {
        lightSource = transform.Find("LightSource").gameObject;
        lightSource.GetComponent<NewLightSource>().lightRadius = 0f;
        pointLight = lightSource.transform.Find("PointLight").gameObject;
    }

    void Start()
    {
        if (transform.parent.GetComponentInChildren<Lever>() != null) transform.parent.GetComponentInChildren<Lever>().activeAtStart = activeAtStart;

        audioSource = GetComponent<AudioSource>();

        if (activeAtStart)
        {
            lightSource.GetComponent<NewLightSource>().lightRadius = targetRadius;
            OnActivate();
        }     

        isMute = false;
        active = activeAtStart;
    }

    private void LateUpdate()
    {
        if (Mathf.Abs(targetRadius - lightSource.GetComponent<NewLightSource>().lightRadius) < 0.001f)
            lightSource.GetComponent<LightCollider>().isStatic = true;

        lightSource.GetComponent<NewLightSource>().lightRadius = Mathf.Lerp(lightSource.GetComponent<NewLightSource>().lightRadius, targetRadius, Time.deltaTime*10);
        pointLight.GetComponent<Light>().range = lightSource.GetComponent<NewLightSource>().lightRadius * 4;
    }

    public override void OnActivate()
    {
        lightSource.GetComponent<LightCollider>().isStatic = false;

        targetRadius = lightRadius;
        active = true;
        if (audioSource != null && !isMute)
            audioSource.PlayOneShot(soundOn);
    }

    public override void OnDeactivate()
    {
        lightSource.GetComponent<LightCollider>().isStatic = false;

        targetRadius = 0.01f;
        active = false;
        if (audioSource != null && !isMute)
            audioSource.PlayOneShot(soundOff);
    }

    public void Reset()
    {
        lightSource.GetComponent<LightCollider>().isStatic = false;

        isMute = true;
        if (active && !activeAtStart)
            OnDeactivate();
        else if (!active && activeAtStart)
            OnActivate();
        isMute = false;
    }
}
