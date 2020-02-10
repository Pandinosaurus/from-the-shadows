﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuCamera : MonoBehaviour
{
    public GameObject startPosition;
    public List<GameObject> cameraPositions;
    public List<GameObject> cameraPositionsZoom;
    public Image cursor;
    public List<GameObject> cursorPositions;
    public float cameraSpeed;

    public Canvas chapterCanvas;

    private int chapterSelected;
    private bool isMoving = false;
    private bool zoom = false;
    private bool returnToMainMenu = false;
    private bool smoothTransition = true;

    // Update is called once per frame
    void Update()
    {
        if (isMoving)
        {
            // Move the camera
            Vector3 targetPosition;
            Transform targetRotation;

            if (returnToMainMenu)
            {
                targetPosition = startPosition.transform.position;
                targetRotation = startPosition.transform;
            }
            else if (!zoom)
            {
                targetPosition = cameraPositions[chapterSelected].transform.position;
                targetRotation = cameraPositions[chapterSelected].transform;
            }
            else
            {
                targetPosition = cameraPositionsZoom[chapterSelected].transform.position;
                targetRotation = cameraPositionsZoom[chapterSelected].transform;
            }

            if (smoothTransition)
            {
                Vector3 velocity = Vector3.zero;

                // Move the camera
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, 0.1f);

                // Rotate the camera
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation.rotation,
                    Time.deltaTime * cameraSpeed
                );

                isMoving = !(velocity.magnitude == 0); // Doesn't actualise the camera position while not moving
            }
            else
            {
                transform.position = targetPosition;
                transform.rotation = targetRotation.rotation;
                isMoving = false;
            }

            // Cursor on UI follow world points
            cursor.gameObject.SetActive(cursorPositions[chapterSelected].GetComponent<Renderer>().isVisible);
            Vector2 screenPoint = Camera.main.WorldToScreenPoint(cursorPositions[chapterSelected].transform.position);
            Vector2 canvasSizeDelta = chapterCanvas.GetComponent<RectTransform>().sizeDelta / 2f;
            float canvasScaleFactor = chapterCanvas.scaleFactor;
            cursor.GetComponent<RectTransform>().anchoredPosition = screenPoint / canvasScaleFactor - canvasSizeDelta;
        }
    }

    /// <summary>
    /// Set if the camera must use its zoom position or not.
    /// </summary>
    public void SetZoom(bool isZooming)
    {
        zoom = isZooming;
        isMoving = true;
    }

    /// <summary>
    /// Set if the camera must return to its start position.
    /// </summary>
    public void SetReturnToMainMenu(bool var)
    {
        returnToMainMenu = var;
        isMoving = true;
    }

    /// <summary>
    /// Set the current chapter selected.
    /// </summary>
    /// <param name="number">The chapter to select.</param>
    public void SetChapterSelected(int number)
    {
        chapterSelected = number;
        isMoving = true;
    }

    /// <summary>
    /// If the camera must move smoothly or not.
    /// </summary>
    public bool SmoothTransition
    {
        set { smoothTransition = value; }
    }
}