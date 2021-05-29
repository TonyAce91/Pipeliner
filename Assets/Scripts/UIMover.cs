using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is used to hide and unhide menus on the side
/// Code written by Antoine Kenneth Odi in 2021
/// </summary>
public class UIMover : MonoBehaviour
{
    public Vector2 closedPos;
    public Vector2 openPos;
    private Vector2 startPos;
    private Vector2 targetPos;


    private bool lerping = false;

    //[SerializeField] private float lerpTime = 1f;
    [SerializeField] private float lerpSpeed = 1f;
    private float startTime = 0;
    RectTransform rect;
    private bool isClosed = true;
    private float journeyLength;

    private void Start()
    {
        rect = (RectTransform)transform;
        journeyLength = Vector2.Distance(openPos, closedPos);
    }

    private void Update()
    {
        if (lerping)
        {
            float distCovered = (Time.time - startTime) * lerpSpeed;

            float fractionOfJourney = distCovered / journeyLength;

            rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, fractionOfJourney);

            Debug.Log("suppose to be lerping");
        }

        if ((rect.anchoredPosition - targetPos).SqrMagnitude() < 0.1f)
            lerping = false;
    }

    public void ToggleMenu()
    {
        if (lerping)
            return;
        targetPos = isClosed ? openPos : closedPos;
        isClosed = !isClosed;
        lerping = true;
        startTime = Time.time;
        startPos = rect.anchoredPosition;
    }
}
