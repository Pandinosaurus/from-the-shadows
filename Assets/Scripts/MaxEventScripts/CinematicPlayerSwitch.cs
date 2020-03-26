﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinematicPlayerSwitch : MonoBehaviour
{
    public SkinnedMeshRenderer mesh;
    public GameObject lightSourceGO;

    public PlayerSwitch playerSwitch;

    [HideInInspector]
    public string playerState = "";

    private void OnEnable()
    {
        if (playerSwitch.GetCurrentPlayer() == "Shadow")
        {
            PlayShadow();
        }
        else
        {
            PlayLight();
        }
    }

    private void PlayLight()
    {
        mesh.material = playerSwitch.lightMat;
        lightSourceGO.SetActive(true);
        playerState = "Light";
    }

    private void PlayShadow()
    {
        mesh.material = playerSwitch.shadowMat;
        lightSourceGO.SetActive(false);
        playerState = "Shadow";
    }
}
