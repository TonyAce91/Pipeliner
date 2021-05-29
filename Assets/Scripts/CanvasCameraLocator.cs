using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is used to find camera within the scene
/// Code written by Antoine Kenneth Odi in 2020
/// </summary>
public class CanvasCameraLocator : MonoBehaviour
{
    Camera mainCamera = null;
    Canvas objectCanvas = null;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = FindObjectOfType<Camera>();
        objectCanvas = GetComponent<Canvas>();
        objectCanvas.worldCamera = mainCamera;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
