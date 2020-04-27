﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torch : ActivatorListener, IResetable
{

    public GameObject lightSource;
    public GameObject pointLight;
    public List<AudioClip> soundsOn;
    public List<AudioClip> soundsOff;
    public bool activeAtStart;
    public float lightRadius;
    public bool neverStatic = false;

    public bool isMute = true;
    public bool active;
    private AudioSource audioSource;
    private float targetRadius = 0.01f;
    private GameObject model;
    private GameObject flamme;
    private ParticleSystem particles;
    private bool isTorch = false; // et pas un chandelier

    private void Awake()
    {
        lightSource = transform.Find("LightSource").gameObject;
        lightSource.GetComponent<NewLightSource>().lightRadius = 0f;
        pointLight = lightSource.transform.Find("PointLight").gameObject;

        if (transform.Find("Model"))
        {
            isTorch = true;
            model = transform.Find("Model").gameObject;
            flamme = model.transform.Find("Flamme").gameObject;
            particles = flamme.GetComponent<ParticleSystem>();
        }
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        isMute = true;

        if (activeAtStart)
        {
            lightSource.GetComponent<NewLightSource>().lightRadius = 0f;
            OnActivate();
        }
        else
        {
            OnDeactivate();
        }

        isMute = false;
    }

    private void LateUpdate()
    {
        if (Mathf.Abs(targetRadius - lightSource.GetComponent<NewLightSource>().lightRadius) < 0.001f && !neverStatic)
            lightSource.GetComponent<NewLightSource>().GoStatic();

        lightSource.GetComponent<NewLightSource>().lightRadius = Mathf.Lerp(lightSource.GetComponent<NewLightSource>().lightRadius, targetRadius, Time.deltaTime * 10);
        pointLight.GetComponent<Light>().range = lightSource.GetComponent<NewLightSource>().lightRadius;
    }

    public override void OnActivate()
    {
        lightSource.GetComponent<LightCollider>().SetStatic(false);
        targetRadius = lightRadius;
        if (isTorch) particles.Play();
        active = true;

        if (audioSource != null && !isMute && soundsOn.Count > 0)
        {
            audioSource.PlayOneShot(soundsOn[Random.Range(0, soundsOn.Count-1)]);
        }
    }

    public override void OnDeactivate()
    {
        lightSource.GetComponent<LightCollider>().SetStatic(false);
        if (isTorch) particles.Stop();
        targetRadius = 0.01f;
        active = false;

        if (audioSource != null && !isMute && soundsOff.Count > 0)
        {
            audioSource.PlayOneShot(soundsOff[Random.Range(0, soundsOff.Count-1)]);
        }
    }

    public void Reset()
    {
        lightSource.GetComponent<LightCollider>().SetStatic(false);

        isMute = true;
        if (active && !activeAtStart)
            OnDeactivate();
        else if (!active && activeAtStart)
            OnActivate();
        isMute = false;
    }
}
