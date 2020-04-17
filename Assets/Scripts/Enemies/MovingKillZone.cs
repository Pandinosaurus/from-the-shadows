﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingKillZone : MonoBehaviour, IResetable
{
    public GameObject killZone;
    [Header("Parent object containing the checkpoints")]
    public GameObject startingCheckpoint;

    private Transform targetPos;
    private float maxSpeed;
    private float timeToMaxSpeed;

    public float currentSpeed = 0;
    
    private MovingKillZoneCP currentResetCheckPoint;
    private MovingKillZoneCP currentCheckPoint;

    private Transform currentResetPoint;

    public Transform TargetPos
    {
        get { return targetPos; }
        set { targetPos = value; }
    }

    public float MaxSpeed
    {
        get { return maxSpeed; }
        set { maxSpeed = value; }
    }

    public float TimeToMaxSpeed
    {
        get { return timeToMaxSpeed; }
        set { maxSpeed = value; }
    }




    // Start is called before the first frame update
    //void Start()
    //{
    //    SetNewInfos(checkPoints[0].GetComponent<MovingKillZoneCP>());
    //}
    void OnEnable()
    {
        SetNewInfos(startingCheckpoint.GetComponent<MovingKillZoneCP>());
    }



    // Update is called once per frame
    void Update()
    {
        //on change jusqu'à la vitesse max en timeToMaxSpeed secondes
        if (Mathf.Abs(maxSpeed - currentSpeed) > 0.1f)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, maxSpeed, Time.deltaTime / timeToMaxSpeed);
        }

        //déplacement vers la cible
        killZone.transform.position = Vector3.MoveTowards(killZone.transform.position, targetPos.position, currentSpeed * Time.deltaTime);
    }

    public void SetNewInfos(MovingKillZoneCP cp)
    {
        currentCheckPoint = cp;

        this.targetPos = cp.targetPos;
        this.maxSpeed = cp. maxSpeed;
        this.timeToMaxSpeed = cp.timeToMaxSpeed;

        if (cp.mustReset)
        {
            currentResetPoint = cp.resetPoint;
            currentResetCheckPoint = cp;
        }
    }

    public void Reset()
    {
        SetNewInfos(currentResetCheckPoint);
        killZone.transform.position = currentResetPoint.position;
        currentSpeed = currentResetCheckPoint.resetSpeed;
    }
}