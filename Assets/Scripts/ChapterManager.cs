﻿using System.Collections.Generic;
using UnityEngine;

public class ChapterManager : MonoBehaviour
{
    public int chapterIndex;
    public List<LevelManager> levels;
    private int currentLevel = 0; //indice du niveau actuel
    private float timeSinceBegin = 0;

    // Update is called once per frame
    void Start()
    {
        //Initialisation of the first level
        GameManager.Instance.LoadSaveFile(0);
        UpdateEnabledLevels();
        Camera.main.GetComponent<LevelCamera>().MoveTo(levels[currentLevel].cameraPoint.position);
    }

    private void Update()
    {
        timeSinceBegin += Time.deltaTime; //Compter de temps pour la collecte de metadonnées
    }

    /// <summary>
    /// decrease the current level index and teleports the cameara to the position avec the previous level
    /// </summary>
    public void PreviousLevel()
    {
        currentLevel--;

        //Activation du niveau courant et désactivation des autres
        UpdateEnabledLevels();

        if (currentLevel < 0) //il faut faire revenir le joueur au choix des level.
        {
            Debug.Log("Déjà au premier tableau");
        }
        else //on transfert le joueur dans le tableau suivant
        {
            //appel de la fonction pour faire bouger la cam
            Debug.Log("On passe au level précédent : " +currentLevel);
            Camera.main.GetComponent<LevelCamera>().MoveTo(levels[currentLevel].cameraPoint.position);
        }
    }

    /// <summary>
    /// increase the current level index and teleports the cameara to the position avec the next level
    /// </summary>
    public void NextLevel()
    {
        //Mise àjour des infos concernant le niveau courant
        GameManager.Instance.SetLevelCompleted(chapterIndex, currentLevel);
        GameManager.Instance.WriteSaveFile();
        
        currentLevel++;

        //Activation du niveau courant et désactivation des autres
        UpdateEnabledLevels();


        if (currentLevel == levels.Count) //Le chapitre est terminé
        {
            Debug.Log("Dernier level terminé, direction le menu de selection de niveau");
            CollectMetaData();
            GameManager.Instance.LoadScene("ChapterMenu");
        }
        else //on transfert le joueur dans le tableau suivant
        {
            //appel de la fonction pour faire bouger la cam
            Debug.Log("On passe au level suivant : " +currentLevel);
            Camera.main.GetComponent<LevelCamera>().MoveTo(levels[currentLevel].cameraPoint.position);
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
        //Debug.Log(GameManager.Instance.GetMetaFloat("totalTimePlayed"));
        GameManager.Instance.WriteSaveFile();
        timeSinceBegin = 0;
    }
}