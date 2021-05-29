using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;
/// <summary>
/// This script is used to create anchor points within the scene as a reference for AR gameobjects
/// Code written by Antoine Kenneth Odi in 2021
/// </summary>

[RequireComponent(typeof(ARAnchorManager))]
[RequireComponent(typeof(ARRaycastManager))]
public class AnchorCreator : MonoBehaviour
{

    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();
    List<ARAnchor> m_Anchors = new List<ARAnchor>();
    ARRaycastManager m_raycastManager;
    ARAnchorManager m_AnchorManager;

    // Find all the managers necessary for the script
    void Awake()
    {
        m_raycastManager = GetComponent<ARRaycastManager>();
        m_AnchorManager = GetComponent<ARAnchorManager>();

    }

    // Creates an anchor using AR foundation's raycast
    ARAnchor CreateAnchor(in ARRaycastHit hit)
    {
        ARAnchor anchor = null;

        // If we hit a plane, try to "attach" the anchor to the plane
        var planeManager = GetComponent<ARPlaneManager>();
        ARPlane plane = planeManager.GetPlane(hit.trackableId);
        anchor = m_AnchorManager.AttachAnchor(plane, hit.pose);
        return anchor;
    }

    void Update()
    {
        if (Input.touchCount == 0)
            return;

        var touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began)
            return;


        // Perform the raycast
        if (m_raycastManager.Raycast(touch.position, s_Hits, TrackableType.Planes))
        {
            // Raycast hits are sorted by distance, so the first one will be the closest hit.
            var hit = s_Hits[0];

            // Create a new anchor
            var anchor = CreateAnchor(hit);
            if (anchor)
            {
                // Remember the anchor so we can remove it later.
                m_Anchors.Add(anchor);
            }
            else
            {
            }
        }
    }

    // Clears all existing anchors in the scene
    public void RemoveAllAnchors()
    {
        foreach (var anchor in m_Anchors)
        {
            Destroy(anchor.gameObject);
        }
        m_Anchors.Clear();
    }

}
