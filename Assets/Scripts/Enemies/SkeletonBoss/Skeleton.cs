﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skeleton : MonoBehaviour, IResetable
{
    // public Transform[] points;  old system
    public GameObject[] targetZones;

    public bool isSolo = false;

    public float timeBeforeFirstAttack = 10;
    public float timeBeforeAttack = 12;
    public float timeBeforeDoubleAttack = 10;
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
    public GameObject bottomKillZone;
    public GameObject cinematicTrigger;

    public GameObject targetForPlayer;

    private AudioSource audioSource;

    public AudioClip soundHit;
    public List<AudioClip> soundPrepareAttack;
    public AudioClip soundDeath;


    [HideInInspector]
    public int idTargetZone;
    [HideInInspector]
    public int hp = 3;
    private int laneToAttack = 0;
    private string stringDirection;
    [HideInInspector]
    public GameObject playerTarget;
    [HideInInspector]
    public bool isTargetting = false;
    private Camera camera;
    [HideInInspector]
    public GUIStyle gui;
    private float iconSize = 140f;

    void Start()
    {
        // InvokeRepeating("TriggerAttack", timeBetweenAttacks, timeBetweenAttacks);    old system
        camera = Camera.main;

        audioSource = GetComponent<AudioSource>();

        // Deactive all particles and initialise targetZones
        foreach (GameObject zone in targetZones)
        {
            zone.GetComponent<TargetZone>().skeleton = this.gameObject;
            //zone.transform.GetChild(0).gameObject.SetActive(false);
        }
        targetForPlayer.SetActive(false);
    }

    void Update()
    {
        //Update laneToAttack and stringDirection to match the playerTarget actual position
        if(playerTarget != null && isTargetting)
        {
            switch (idTargetZone)
            {
                case 0:
                    laneToAttack = 0;
                    stringDirection = "Left";
                    break;
                case 1:
                    laneToAttack = 0;
                    stringDirection = "Right";
                    break;
                case 2:
                    laneToAttack = 1;
                    stringDirection = "Left";
                    break;
                case 3:
                    laneToAttack = 1;
                    stringDirection = "Right";
                    break;
                case 4:
                    laneToAttack = 2;
                    stringDirection = "Left";
                    break;
                case 5:
                    laneToAttack = 2;
                    stringDirection = "Right";
                    break;
            }

            // Display particle on targeted player
            targetForPlayer.transform.position = playerTarget.transform.position;
            targetForPlayer.SetActive(true);

        }
        else
        {
            // Deactivate particles 
            targetForPlayer.SetActive(false);
            foreach (GameObject zone in targetZones)
            {
                zone.transform.GetChild(0).gameObject.SetActive(false);
            }
        }

        // A SUPPRIMER
        if(Input.GetKeyDown(KeyCode.G))
        {
            Invoke("GetHurt", 0);
        }
    }

    /*public void OnGUI()
    {
        if (playerTarget != null && isTargetting)
        {
            Vector2 targetForPlayerPosition = new Vector2(camera.WorldToScreenPoint(playerTarget.transform.position).x,
                Screen.height - camera.WorldToScreenPoint(playerTarget.transform.position).y);
            targetForPlayerPosition.y -= iconSize * 0.8f;
            targetForPlayerPosition.x -= iconSize / 2;
            GUI.Box(new Rect(targetForPlayerPosition.x, targetForPlayerPosition.y, iconSize,iconSize),
                    targetForPlayer, gui);
        }
    } */

    public void Appear()
    {
        // Debug.Log("Boss fight starting");
        transform.Find("SkeletonFBX").GetComponent<Animator>().SetTrigger("Appear");
        Invoke("PrepareAttack", timeBeforeFirstAttack);

        // Make hands appear here
        Invoke("EnableHands", 6f);
        // InvokeRepeating("PrepareAttack", timeBeforeFirstAttack, timeBetweenAttacks); old system
    }

    public void EnableHands()
    {
        Invoke("EnableHand1", 0.5f);
        Invoke("EnableHand2", 1f);
        Invoke("EnableHand3", 1.5f);
        Invoke("EnableHand4", 2f);
    }

    public void EnableHand1()
    {
        hands.transform.GetChild(0).gameObject.SetActive(true);
        hands.transform.GetChild(0).GetComponent<HandCollision>().audioSource.PlayOneShot(hands.transform.GetChild(0).GetComponent<HandCollision>().soundHandStart[Random.Range(0, 1)]);
    }
    public void EnableHand2()
    {
        hands.transform.GetChild(3).GetChild(0).gameObject.SetActive(true);
        hands.transform.GetChild(1).GetComponent<HandCollision>().audioSource.PlayOneShot(hands.transform.GetChild(1).GetComponent<HandCollision>().soundHandStart[Random.Range(0, 1)]);

    }
    public void EnableHand3()
    {
        hands.transform.GetChild(2).GetChild(0).gameObject.SetActive(true);
        hands.transform.GetChild(0).GetComponent<HandCollision>().audioSource.PlayOneShot(hands.transform.GetChild(0).GetComponent<HandCollision>().soundHandStart[Random.Range(0, 1)]);

    }
    public void EnableHand4()
    {
        hands.transform.GetChild(1).gameObject.SetActive(true);
        hands.transform.GetChild(1).GetComponent<HandCollision>().audioSource.PlayOneShot(hands.transform.GetChild(1).GetComponent<HandCollision>().soundHandStart[Random.Range(0, 1)]);
    }


    public void PrepareAttack()
    {
        isTargetting = true;
        ChoosePlayerTarget();
        Invoke("TriggerAttack", timeBeforeAttack);
    }

    public void TriggerAttack()
    {
        isTargetting = false;
        string trigger = "Attack" + stringDirection + laneToAttack;
        hands.transform.Find(stringDirection + "HandSkeleton").GetComponent<Animator>().SetTrigger(trigger);

        StartCoroutine(PlayHandAttackSound(hands.transform.Find(stringDirection + "HandSkeleton").GetComponent<HandCollision>()));
        Invoke("PrepareAttack", 7);
        // timing may need adjustement
    }

    IEnumerator PlayHandAttackSound(HandCollision handCollision)
    {
        AudioClip randomPlayedClip = handCollision.soundHandStart[Random.Range(0, 1)];

        handCollision.audioSource.PlayOneShot(randomPlayedClip);

        yield return new WaitForSeconds(randomPlayedClip.length - 0.5f);

        handCollision.audioSource.PlayOneShot(handCollision.soundHandEnd);
    }


    public void PrepareDoubleAttack()
    {
        isTargetting = true;
        ChoosePlayerTarget();
        Invoke("TriggerDoubleAttack", timeBeforeDoubleAttack);
    }

    public void TriggerDoubleAttack()
    {
        isTargetting = false;
        hands.transform.Find("LeftHandSkeleton").GetComponent<Animator>().SetTrigger("AttackLeft" + laneToAttack);
        hands.transform.Find("RightHandSkeleton").GetComponent<Animator>().SetTrigger("AttackRight" + laneToAttack);

        hands.transform.GetChild(1).GetComponent<HandCollision>().audioSource.PlayOneShot(hands.transform.GetChild(1).GetComponent<HandCollision>().soundHandStart[Random.Range(0, 1)]);
        hands.transform.GetChild(0).GetComponent<HandCollision>().audioSource.PlayOneShot(hands.transform.GetChild(0).GetComponent<HandCollision>().soundHandStart[Random.Range(0, 1)]);
        hands.transform.Find("LeftHandSkeleton").GetComponent<HandCollision>().audioSource.PlayOneShot(hands.transform.Find("LeftHandSkeleton").GetComponent<HandCollision>().soundVerticalDestruction);


        Invoke("PrepareDoubleAttack", 6);
        // timing may need adjustement
    }

    /*  Old system for choosing lane to attack and direction
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
    */

    public void ChoosePlayerTarget()
    {
        // If solo mode the target is player1
        if (isSolo)
        {
            playerTarget = player1;
        }

        // Else in duo mode        
        else
        {
            // If it is the first attack the choice is random
            if (playerTarget == null)
            {
                int rand = Random.Range(0, 2);
                if (rand == 0)
                    playerTarget = player1;
                else
                    playerTarget = player2;
            }
            else
            {
                // Else the target alternates between the two players
                if (playerTarget == player1)
                    playerTarget = player2;
                else
                    playerTarget = player1;
            }
        }

        audioSource.PlayOneShot(soundPrepareAttack[Random.Range(0, soundPrepareAttack.Count - 1)]);
        // Debug.Log("The target is player "+ playerTarget.GetComponent<PlayerInput>().id);           
    }

    /*  Old system for choosing nearest player
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
    */

    public void GetHurt()
    {
        transform.Find("SkeletonFBX").GetComponent<Animator>().SetTrigger("Battlecry");
        //hands.transform.Find("RightHandSkeleton").GetComponent<Animator>().SetTrigger("Die");
        //hands.transform.Find("LeftHandSkeleton").GetComponent<Animator>().SetTrigger("Die");

        hp--;

        audioSource.PlayOneShot(soundHit);

        if (hp == 0)
        {
            Die();
            //Invoke("DestroyMiddleZone", 3);
            //Invoke("DestroyOtherZones", 4);
        }

        if (hp == 1)
        {
            //Cancel Trigger simple attack and start double attack
            CancelInvoke();
            Invoke("ActiveMiddleZoneSpikesAnim", 1);
            Invoke("StartFallingPlatform", 1.8f);
            Invoke("ActiveMiddleZoneSpikes", 4);

            Invoke("PrepareDoubleAttack", timeBeforeDoubleAttack);
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

        audioSource.PlayOneShot(soundDeath);

        hands.transform.Find("RightHandSkeleton").gameObject.SetActive(false);
        hands.transform.Find("LeftHandSkeleton").gameObject.SetActive(false);

        foreach (DestructiblePlatform destructiblePlatform in middleZoneSpikes.GetComponentsInChildren<DestructiblePlatform>())
        {
            destructiblePlatform.Destruct();
        }
        cinematicTrigger.SetActive(true);
        CancelInvoke();
    }
    
    public void Reset()
    {
        hp = 3;

        // Cancel hand attack
        hands.transform.Find("RightHandSkeleton").GetComponent<HandCollision>().Reset();
        hands.transform.Find("LeftHandSkeleton").GetComponent<HandCollision>().Reset();

        audioSource.Stop();


        CancelInvoke();
        playerTarget = null;
        isTargetting = false;

        // Restart hand attack
        Invoke("PrepareAttack", 5f);

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

        // Deactive all particles and initialise targetZones
        foreach (GameObject zone in targetZones)
        {
            zone.transform.GetChild(0).gameObject.SetActive(false);
        }
        targetForPlayer.SetActive(false);
    }

    public void DestroyLeftZone()
    {
        leftKillZone.SetActive(false);
        //hands.transform.Find("LeftHandSkeleton").GetComponent<HandCollision>().isDestructor = true;
        hands.transform.Find("LeftHandSkeleton").GetComponent<Animator>().SetTrigger("VerticalLeft");
        hands.transform.Find("LeftHandSkeleton").GetComponent<HandCollision>().audioSource.PlayOneShot(hands.transform.Find("LeftHandSkeleton").GetComponent<HandCollision>().soundVerticalDestruction);
        Invoke("ActiveLeftZoneBis", 4.5f);
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
        //hands.transform.Find("RightHandSkeleton").GetComponent<HandCollision>().isDestructor = true;
        hands.transform.Find("RightHandSkeleton").GetComponent<Animator>().SetTrigger("VerticalRight");
        hands.transform.Find("RightHandSkeleton").GetComponent<HandCollision>().audioSource.PlayOneShot(hands.transform.Find("RightHandSkeleton").GetComponent<HandCollision>().soundVerticalDestruction);
        Invoke("ActiveRightZoneBis", 4.5f);
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
