﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skeleton : MonoBehaviour, IResetable
{
    public Transform[] points;

    public bool isSolo = false;

    public float timeBetweenAttacks;
    public float timeBetweenDoubleAttacks;
    public GameObject hands;
    public GameObject player1;
    public GameObject player2;
    public GameObject leftZone;
    public GameObject middleZone;
    public GameObject rightZone;
    public GameObject leftZoneBis;
    public GameObject rightZoneBis;
    public GameObject rightKillZone;
    public GameObject leftKillZone;
    public GameObject middleZoneSpikes;
    public GameObject middleZoneSpikesAnim;
    //public GameObject spawnGhostObject;

    public GameObject bottomKillZone;
    public GameObject endChapterTrigger;

    private int hp = 3;
    private int laneToAttack = 0;
    private string stringDirection;

    // Start is called before the first frame update
    void Start()
    {
        // InvokeRepeating("TriggerAttack", timeBetweenAttacks, timeBetweenAttacks);
    }

    public void Appear()
    {
        Debug.Log("Boss fight starting");
        transform.Find("SkeletonFBX").GetComponent<Animator>().SetTrigger("Appear");
        InvokeRepeating("TriggerAttack", 10, timeBetweenAttacks);
        //spawnGhostObject.GetComponent<SpawnGhost>().StartSpawningGhost();
    }

    public void TriggerAttack()
    {
        FindTarget();
        string trigger = "Attack" + stringDirection + laneToAttack;
        hands.transform.Find(stringDirection + "HandSkeleton").GetComponent<Animator>().SetTrigger(trigger);
    }

    public void TriggerDoubleAttack()
    {
        FindDoubleTarget();
        hands.transform.Find("LeftHandSkeleton").GetComponent<Animator>().SetTrigger("AttackLeft" + laneToAttack);
        hands.transform.Find("RightHandSkeleton").GetComponent<Animator>().SetTrigger("AttackRight" + laneToAttack);
    }

    public void FindDoubleTarget()
    {
        float minL = Mathf.Infinity;
        float minR = Mathf.Infinity;
        int laneR = -1;
        int laneL = -1;
        for (int i = 0; i < points.Length / 2; i++)
        {
            float minLeft = 0f;
            float minRight = 0f;

            if (isSolo)
            {
                minLeft = Vector3.Distance(player1.transform.position, points[i * 2].position);
                minRight = Vector3.Distance(player1.transform.position, points[i * 2 + 1].position);
            }
            else
            {
                minLeft = Mathf.Min(Vector3.Distance(player1.transform.position, points[i * 2].position),
                          Vector3.Distance(player2.transform.position, points[i * 2].position));
                minRight = Mathf.Min(Vector3.Distance(player1.transform.position, points[i * 2 + 1].position),
                                           Vector3.Distance(player2.transform.position, points[i * 2 + 1].position));
            }
            if (minLeft < minL)
            {
                minL = minLeft;
                laneL = i;
            }
            if (minRight < minR)
            {
                minR = minRight;
                laneR = i;
            }
        }

        if (laneR != 1)
            laneToAttack = laneR;
        else if (laneL != 1)
            laneToAttack = laneL;
        else
            laneToAttack = 1;
    }

    public void FindTarget()
    {
        float min = Mathf.Infinity;
        for (int i = 0; i < points.Length / 2; i++) {

            float minLeft = 0f;
            float minRight = 0f;

            if(isSolo)
            {
                minLeft = Vector3.Distance(player1.transform.position, points[i * 2].position);
                minRight = Vector3.Distance(player1.transform.position, points[i * 2 + 1].position);
            }
            else
            {
                minLeft = Mathf.Min(Vector3.Distance(player1.transform.position, points[i * 2].position),
                          Vector3.Distance(player2.transform.position, points[i * 2].position));
                minRight = Mathf.Min(Vector3.Distance(player1.transform.position, points[i * 2 + 1].position),
                                           Vector3.Distance(player2.transform.position, points[i * 2 + 1].position));
            }

            if (minLeft < min && minLeft < minRight)
            {
                min = minLeft;
                laneToAttack = i;
                stringDirection = "Left";
            } else if (minRight < min && minRight < minLeft)
            {
                min = minRight;
                laneToAttack = i;
                stringDirection = "Right";
            }
        }
    }
    
    public void GetHurt()
    {
        transform.Find("SkeletonFBX").GetComponent<Animator>().SetTrigger("Battlecry");
        hands.transform.Find("RightHandSkeleton").GetComponent<Animator>().SetTrigger("Die");
        hands.transform.Find("LeftHandSkeleton").GetComponent<Animator>().SetTrigger("Die");

        hp--;

        if (hp == 0)
        {
            Die();
            Invoke("DestroyMiddleZone", 3);
            Invoke("DestroyOtherZones", 4);
        }

        if (hp == 1)
        {
            //Cancel Trigger simple attack and start double attack
            CancelInvoke();
            Invoke("ActiveMiddleZoneSpikesAnim", 1);
            Invoke("StartFallingPlatform", 1.8f);
            Invoke("ActiveMiddleZoneSpikes", 4);

            InvokeRepeating("TriggerDoubleAttack", 5, timeBetweenDoubleAttacks);
        }

        if (hp == 2 || hp == 1)
        {
            if (stringDirection == "Left")
                Invoke("DestroyRightZone", 1);
            else if (stringDirection == "Right") 
                Invoke("DestroyLeftZone", 1);
        }
    }

    public void Die()
    {
        transform.Find("SkeletonFBX").GetComponent<Animator>().SetTrigger("Die");
        CancelInvoke();
    }

    public void Reset()
    {
        hp = 3;

        // Cancel hand attack
        hands.transform.Find("RightHandSkeleton").GetComponent<HandCollision>().StopHand();
        hands.transform.Find("LeftHandSkeleton").GetComponent<HandCollision>().StopHand();
        CancelInvoke();

        // Restart hand attack
        InvokeRepeating("TriggerAttack", timeBetweenAttacks, timeBetweenAttacks);

        //Reactivate destructible platforms
        leftZone.SetActive(true);
        rightZone.SetActive(true);
        DestructiblePlatform[] toActivateLeft = leftZone.GetComponentsInChildren<DestructiblePlatform>(true);
        DestructiblePlatform[] toActivateRight = rightZone.GetComponentsInChildren<DestructiblePlatform>(true);
        for (int i = 0; i < toActivateLeft.Length; i++)
        {
            toActivateLeft[i].Reset();
        }
        for (int i = 0; i < toActivateRight.Length; i++)
        {
            toActivateRight[i].Reset();
        }


        //Deactivate zone bis
        leftZoneBis.SetActive(false);
        rightZoneBis.SetActive(false);
        middleZoneSpikes.SetActive(false);
        middleZoneSpikesAnim.GetComponent<Animator>().ResetTrigger("Appear");
        middleZoneSpikesAnim.GetComponent<Animator>().SetTrigger("Reset");
        middleZoneSpikesAnim.SetActive(false);
        //Reactivate killzone
        leftKillZone.SetActive(true);
        rightKillZone.SetActive(true);
    }

    public void DestroyLeftZone()
    {
        leftKillZone.SetActive(false);
        hands.transform.Find("LeftHandSkeleton").GetComponent<HandCollision>().isDestructor = true;
        hands.transform.Find("LeftHandSkeleton").GetComponent<Animator>().SetTrigger("VerticalLeft");
        Invoke("ActiveLeftZoneBis", 2);
    }

    public void ActiveLeftZoneBis()
    {
        leftZoneBis.SetActive(true);
        leftZone.SetActive(false);
        hands.transform.Find("LeftHandSkeleton").GetComponent<HandCollision>().isDestructor = false;
    }

    public void DestroyRightZone()
    {
        rightKillZone.SetActive(false);
        hands.transform.Find("RightHandSkeleton").GetComponent<HandCollision>().isDestructor = true;
        hands.transform.Find("RightHandSkeleton").GetComponent<Animator>().SetTrigger("VerticalRight");
        Invoke("ActiveRightZoneBis", 2);
    }

    public void ActiveRightZoneBis()
    {
        rightZoneBis.SetActive(true);
        rightZone.SetActive(false);
        hands.transform.Find("RightHandSkeleton").GetComponent<HandCollision>().isDestructor = false;
    }

    public void DestroyMiddleZone()
    {
        bottomKillZone.SetActive(false);
        endChapterTrigger.SetActive(true);
        middleZone.SetActive(false);
        middleZoneSpikes.SetActive(false);
    }

    public void DestroyOtherZones()
    {
        leftZoneBis.SetActive(false);
        rightZoneBis.SetActive(false);
    }
    public void ActiveMiddleZoneSpikesAnim()
    {
        middleZoneSpikesAnim.SetActive(true);
        middleZoneSpikesAnim.GetComponent<Animator>().SetTrigger("Appear");
    }

    public void StartFallingPlatform()
    {
        middleZoneSpikesAnim.transform.GetChild(2).GetComponent<FallingPlatform>().StartFall();
    }

    public void ActiveMiddleZoneSpikes()
    {
        middleZoneSpikes.SetActive(true);
        middleZoneSpikesAnim.SetActive(false);
    }
}
