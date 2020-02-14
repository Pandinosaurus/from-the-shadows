﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChapterManager : MonoBehaviour
{
    public List<LevelManager> levels;
    public PauseMenu pauseMenu;
    private int currentLevel = 0; //indice du niveau actuel
    private float timeSinceBegin = 0;
    private LevelCamera levelCamera;

    // Update is called once per frame
    void Start()
    {
        if (GameManager.Instance.LoadingChapterInfo != null)
        {
            currentLevel = GameManager.Instance.LoadingChapterInfo.StartLevelIndex;
        }

        levelCamera = Camera.main.GetComponent<LevelCamera>();
        CreateEmptyCameraPoints();
        levelCamera.SetLimit(levels[currentLevel].cameraLimitLB, levels[currentLevel].cameraLimitRT);
        levelCamera.MoveTo((levels[currentLevel].cameraLimitRT.position + levels[currentLevel].cameraLimitLB.position) / 2, false);

        SpawnPlayer(levels[currentLevel].playerSpawn.position);

        UpdateEnabledLevels();
    }

    private void Update()
    {
        timeSinceBegin += Time.deltaTime; //Compter de temps pour la collecte de metadonnées

        // Position moyenne des deux joueurs

        if (levelCamera.StayInLimits)
        {
            Vector3 meanPosition = new Vector3();
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            meanPosition = (players[0].transform.position + players[1].transform.position) / 2;
            Camera.main.GetComponent<LevelCamera>().MoveTo(meanPosition);
        }

        if (Input.GetButtonDown("Start_G"))
        {
            pauseMenu.gameObject.SetActive(true);
            pauseMenu.OpenPauseMenu();
        }
        if (Input.GetKey(KeyCode.RightAlt) && Input.GetKeyDown(KeyCode.N))
        {
            NextLevel();
        }
    }

    public void SpawnPlayer(Vector3 pos)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            player.transform.position = pos;
        }
    }

    /// <summary>
    /// decrease the current level index and teleports the cameara to the position avec the previous level
    /// </summary>
    public void PreviousLevel()
    {
        currentLevel--;

        //Activation du niveau courant et désactivation des autres
        UpdateEnabledLevels();

        if (currentLevel >= 0) //on bouge la cam dans le tableau précédent
        {
            CreateEmptyCameraPoints();
            levelCamera.SetLimit(levels[currentLevel].cameraLimitLB, levels[currentLevel].cameraLimitRT);
        }
    }

    /// <summary>
    /// increase the current level index and teleports the cameara to the position avec the next level
    /// </summary>
    public void NextLevel()
    {
        //Mise àjour des infos concernant le niveau courant
        GameManager.Instance.SetLevelCompleted(GameManager.Instance.CurrentChapter, currentLevel);
        currentLevel++;

        //Activation du niveau courant et désactivation des autres
        UpdateEnabledLevels();


        if (currentLevel == levels.Count) //Le chapitre est terminé
        {
            //Save the metaData
            CollectMetaData();
            SaveManager.Instance.WriteSaveFile();
            GameManager.Instance.LoadMenu("MainMenu", new LoadingMenuInfo(2));
        }
        else //on transfert le joueur dans le tableau suivant
        {
            //appel de la fonction pour faire bouger la cam
            CreateEmptyCameraPoints();
            levelCamera.SetLimit(levels[currentLevel].cameraLimitLB, levels[currentLevel].cameraLimitRT);
        }

        SaveManager.Instance.WriteSaveFile();
    }

    public void CreateEmptyCameraPoints()
    {
        if (levels[currentLevel].cameraLimitLB == null)
        {
            GameObject emptyPoint = new GameObject();
            emptyPoint.transform.position = new Vector3(-5f, 5f, -10f);
            levels[currentLevel].cameraLimitLB = emptyPoint.transform;
        }
        if (levels[currentLevel].cameraLimitRT == null)
        {
            GameObject emptyPoint = new GameObject();
            emptyPoint.transform.position = new Vector3(-5f, 5f, -10f);
            levels[currentLevel].cameraLimitRT = emptyPoint.transform;
        }
    }

    public void UpdateEnabledLevels()
    {
        for (int i = 0; i < levels.Count; i++)
        {
            if (i == currentLevel)
            {
                levels[i].EnableLevel();
            }
            else
            {
                levels[i].DisableLevel();
            }
        }
    }

    private void CollectMetaData()
    {
        GameManager.Instance.AddMetaFloat("totalTimePlayed", timeSinceBegin); //collecte du temps de jeu
        timeSinceBegin = 0;
    }

    /// <summary>
    /// The player died : displays all deaths animations (player, screen, etc...) and reset all Resetable Objects
    /// </summary>
    /// <param name="playerId"></param>
    public void ResetLevel(int playerId)
    {
        StartCoroutine(ResetLevelAsync(playerId));
    }

    /// <summary>
    /// The player died : displays all deaths animations (player, screen, etc...) and reset all Resetable Objects
    /// </summary>
    /// <param name="playerId"></param>
    IEnumerator ResetLevelAsync(int playerId)
    {
        GameObject transitionScreen = (GameObject)Resources.Load("SwipeTransition"); //load le prefab
        transitionScreen = Instantiate(transitionScreen, gameObject.transform); //l'affiche

        //tant que l'ecran n'a pas fini de fade au noir
        while (!transitionScreen.GetComponent<TransitionScreen>().finishedFadingIn)
        {
            yield return null;
        }
        //On fait ci dessous tout ce qui intervient pendant que l'écran est noir

        //Teleporte les joueurs au début du jeu
        SpawnPlayer(levels[currentLevel].playerSpawn.position);
        //téléporte la camera à sa position de départ
        //Incrémente la meta donnée du joueur mort
        GameManager.Instance.AddMetaInt("playerDeath" + playerId, 1);
        //Reset tous les objets Resetables
        levels[currentLevel].ResetAllResetables();
    }
}
