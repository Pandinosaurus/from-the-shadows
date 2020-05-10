﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(Animator))]
public class Carousel : MonoBehaviour
{
    public MenuManager menuManager;
    public GridLayoutGroup buttonsGroup;
    public LevelButton levelButtonPrefab;
    public GameObject collectibleLight, collectibleShadow, collectibleMissing; // Prefabs
    public GameObject levelScreenshotsParent;
    public TextMeshProUGUI levelName;
    public Image leftArrow;
    public Image rightArrow;
    public LevelScreenshot levelScreenshotPrefab;

    private List<LevelScreenshot> screenshots = new List<LevelScreenshot>();
    [Header("Carousel")]
    public float distanceBetweenScreenshots;
    [Range(0.0f, 3.0f)]
    [Tooltip("The minimum size when a level is not selected")]
    public float minSize;
    [Range(0.0f, 3.0f)]
    [Tooltip("The size multiplier of the selected level")]
    public float maxSize;
    [Range(0f, 1f)]
    [Tooltip("Foreground alpha value when screenshot is unselected")]
    public float foregroundMaxAlpha;
    [Tooltip("The speed of the carousel")]
    public float speed;
    [Tooltip("time between every possible change (prevents to go too fast if you hold)")]
    public float repeatDelay;
    [Range(0.0f, 1.0f)]
    public float stickDeadZone;

    private int currentLevelIndex = 0;

    private float timeCpt = 0;
    private bool pressed = false;

    [Header("Level Buttons Infos")]
    public LevelButtonInfosArray[] levelButtonInfosMatrix;

    [HideInInspector] public Animator animator;

    private void Awake()
    {
        pressed = false;
        animator = GetComponent<Animator>();
    }

    public void SetMenuLevels(int chapterNumber)
    {
        ResetScreenshots();

        int nbCompleted = 0;
        int totalLevels = 0;

        List<Level> levels = GameManager.Instance.Saves[GameManager.Instance.CurrentSave].Chapters[chapterNumber].GetLevels();
        foreach (Level l in levels)
        {

            if (l.Completed) nbCompleted++;
            totalLevels++;
        }

        //SetMenuLevelInfo(0, screenshots[0]);
        LevelButtonInfosArray levelButtonInfosArray = null;
        if (chapterNumber < levelButtonInfosMatrix.Length)
        {
            levelButtonInfosArray = levelButtonInfosMatrix[chapterNumber];
        }

        int nbLevelSpawned = 0;
        for (int i = 0; i < totalLevels; i++) // Create the levels buttons
        {
            if (i == 0 || (levels[i].IsCheckpoint && levels[i].Completed) || (GameManager.Instance.CurrentChapter > 0 && i == levels.Count - 1 && levels[i - 1].Completed))
            {
                int localLevelIndex = i;
                LevelScreenshot spawnedScreenshot = Instantiate(levelScreenshotPrefab, levelScreenshotsParent.transform).GetComponent<LevelScreenshot>();
                SetMenuLevelInfo(i, spawnedScreenshot);
                spawnedScreenshot.GetComponent<Button>().onClick.AddListener(delegate
                {
                    LevelButtonClicked(new LoadingChapterInfo(localLevelIndex), spawnedScreenshot);
                });
                spawnedScreenshot.LevelId = localLevelIndex;
                spawnedScreenshot.levelIndex = nbLevelSpawned;
                spawnedScreenshot.GetComponent<RectTransform>().localPosition = new Vector3(nbLevelSpawned * distanceBetweenScreenshots, 0, 0);

                if (levelButtonInfosArray != null && nbLevelSpawned < levelButtonInfosArray.infos.Length)
                {
                    LevelButtonInfos levelButtonInfos = levelButtonInfosArray.infos[nbLevelSpawned];
                    spawnedScreenshot.screenshot.sprite = levelButtonInfos.image;
                }

                spawnedScreenshot.Init(this);
                screenshots.Add(spawnedScreenshot);
                nbLevelSpawned++;
            }
        }

        for (int i = 0; i < screenshots.Count; i++)
        {
            Navigation explicitNav = new Navigation();
            explicitNav.mode = Navigation.Mode.Explicit;
            if (i > 0)
            {
                explicitNav.selectOnLeft = screenshots[i - 1].GetComponent<Button>();
            }
            if (i < screenshots.Count - 1)
            {
                explicitNav.selectOnRight = screenshots[i + 1].GetComponent<Button>();
            }
            screenshots[i].GetComponent<Button>().navigation = explicitNav;
        }

        EventSystem.current.SetSelectedGameObject(screenshots[0].gameObject);
    }

    public void SelectCheckpoint(int index)
    {
        Animator animator = GetComponent<Animator>();
        GetComponentInParent<Canvas>().GetComponent<AudioSource>().PlayOneShot(menuManager.uiSelect);

        foreach (LevelScreenshot go in screenshots)
        {
            go.destination = new Vector3((go.levelIndex - index) * distanceBetweenScreenshots, 0, 0);
        }
        levelName.text = levelButtonInfosMatrix[GameManager.Instance.CurrentChapter].infos[index].name;

        if (animator != null)
        {
            if (index < currentLevelIndex) animator.SetTrigger("LeftArrowGiggle");
            else if (index > currentLevelIndex) animator.SetTrigger("RightArrowGiggle");
        }

        if (index == 0) leftArrow.color = Color.gray;
        else leftArrow.color = Color.white;

        if (index == screenshots.Count - 1) rightArrow.color = Color.gray;
        else rightArrow.color = Color.white;

        currentLevelIndex = index;
    }

    public void SetMenuLevelInfo(int level, LevelScreenshot screenshot)
    {
        List<KeyValuePair<string, bool>> collectiblesTaken = GetCollectibleToNextCheckPoint(level);

        foreach (KeyValuePair<string, bool> kv in collectiblesTaken)
        {
            if (kv.Key == "light" && kv.Value)
            {
                Instantiate(collectibleLight, screenshot.collectiblesHolder.transform);
            }
            else if (kv.Key == "shadow" && kv.Value)
            {
                Instantiate(collectibleShadow, screenshot.collectiblesHolder.transform);
            }
            else
            {
                Instantiate(collectibleMissing, screenshot.collectiblesHolder.transform);
            }
        }
    }

    /// <summary>
    /// Returns a List<KeyValuePair<string, bool>> containing all the collectibles between the current checkpoint and the next check point
    /// </summary>
    /// <returns></returns>
    public List<KeyValuePair<string, bool>> GetCollectibleToNextCheckPoint(int level)
    {
        List<KeyValuePair<string, bool>> collectibles = new List<KeyValuePair<string, bool>>();
        List<Level> ChapterLevels = GameManager.Instance.GetChapters()[GameManager.Instance.CurrentChapter].GetLevels();

        //pour chaque tableau jusqu'au prochain checkpoint
        do
        {
            //tout les colletibles de lumière du tableau
            bool[] levelLightCollectibles = ChapterLevels[level].LightCollectibles;
            foreach (bool b in levelLightCollectibles)
            {
                collectibles.Add(new KeyValuePair<string, bool>("light", b));
            }

            //tout les colletibles d'ombre du tableau
            bool[] levelShadowtCollectibles = ChapterLevels[level].ShadowCollectibles;
            foreach (bool b in levelShadowtCollectibles)
            {
                collectibles.Add(new KeyValuePair<string, bool>("shadow", b));
            }
            level++;
        } while (level < ChapterLevels.Count && !ChapterLevels[level].IsCheckpoint);

        return collectibles;
    }

    public void ResetScreenshots()
    {
        foreach (LevelScreenshot child in screenshots)
        {
            GameObject.Destroy(child.gameObject);
        }
        screenshots = new List<LevelScreenshot>();
        currentLevelIndex = 0;
    }

    private void LevelButtonClicked(LoadingChapterInfo loadingChapterInfo, LevelScreenshot screenshot)
    {
        if(!pressed)
        {
            GetComponentInParent<Canvas>().GetComponent<AudioSource>().PlayOneShot(menuManager.uiPress);

            int currentSave = GameManager.Instance.CurrentSave;

            if (GameManager.Instance.Saves[currentSave].NbPlayer == 1)
                GameManager.Instance.LoadChapter("ChapterSolo_0" + GameManager.Instance.CurrentChapter, loadingChapterInfo);
            else
                GameManager.Instance.LoadChapter("ChapterDuo_0" + GameManager.Instance.CurrentChapter, loadingChapterInfo);
            //animation
            StartCoroutine(screenshot.PressedAnimation());

            GameObject.Find("MusicManager").GetComponent<MusicManager>().StopTheme();
            //disable les controles pour ne pas pouvoir continuer alors qu'un bouton a déjà été pressed
            pressed = true;
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
