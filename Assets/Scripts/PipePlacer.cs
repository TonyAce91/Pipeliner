using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// This is used to instantiate pipe using the real world reference via plane detection
/// Code written by Antoine Kenneth Odi in 2020
/// </summary>

public enum RaycastMode
{
    None,
    Stop,
    Warning,
    Text
}

[System.Serializable]
public struct ObjectReferences
{
    public GameObject stopReference;
    public GameObject warningReference;
    public GameObject textReference;
}

[RequireComponent(typeof(ARSessionOrigin))]
public class PipePlacer : MonoBehaviour
{
    [SerializeField] private GameObject m_contentPrefab = null;
    [SerializeField] ARSessionOrigin m_sessionOrigin = null;
    [SerializeField] ARRaycastManager m_raycastManager = null;
    private ARPlaneManager m_arPlaneManager = null;
    [SerializeField] private Text warningText = null;
    [SerializeField] private GameObject m_invisibleARPlane = null;
    [SerializeField] private GameObject m_defaultARPlane = null;

    private RaycastMode currentRaycastMode = RaycastMode.None;

    public GameObject spawnedObject = null;

    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();


    [SerializeField] private Quaternion baseRotation = new Quaternion(0, 0, 1, 0);

    [Header("Objects To Spawn")]
    [SerializeField] private GameObject m_stopObject = null;
    [SerializeField] private GameObject m_warningObject = null;
    [SerializeField] private GameObject m_textObject = null;

    [Header("Button References")]
    [SerializeField] private List<Image> m_spawnerButtons = new List<Image>();

    private List<GameObject> m_listOfReferences = new List<GameObject>();


    private Gyroscope gyro;
    private bool gyroActive;


    // Finds reference to components necessary for the code to run
    void Awake()
    {
        m_sessionOrigin = GetComponent<ARSessionOrigin>();
        m_raycastManager = GetComponent<ARRaycastManager>();
        m_arPlaneManager = GetComponent<ARPlaneManager>();
    }

    private void Start()
    {
        if (SystemInfo.supportsGyroscope)
        {
            gyro = Input.gyro;
            gyro.enabled = true;
            gyroActive = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        OnlyOnePlane();

        if (Input.touchCount <= 0 || m_contentPrefab == null)
            return;

        var touch = Input.GetTouch(0);

        if (m_raycastManager.Raycast(touch.position, s_Hits, TrackableType.PlaneWithinPolygon) && spawnedObject == null)
        {
            // Raycast hits are sorted by distance, so the first one
            // will be the closest hit.
            var hitPose = s_Hits[0].pose;

            // This does not move the content; instead, it moves and orients the ARSessionOrigin
            // such that the content appears to be at the raycast hit position.
            //m_sessionOrigin.MakeContentAppearAt(m_content, hitPose.position, m_content.rotation);

            if (m_contentPrefab)
            {
                spawnedObject = Instantiate(m_contentPrefab, hitPose.position + new Vector3(0, -(/*GPS.Instance.altitude **/ 0.05f), 0), Quaternion.identity);
                spawnedObject.transform.rotation = Quaternion.Euler(0, -Input.compass.magneticHeading, 0);
            }

            if (spawnedObject)
                spawnedObject.transform.position = hitPose.position + new Vector3(0, -(/*GPS.Instance.altitude **/ 0.05f), 0);
            else if (warningText)
                warningText.text = "Content not detected";
        }

        else if (m_raycastManager.Raycast(touch.position, s_Hits, TrackableType.Planes) && (currentRaycastMode == RaycastMode.Stop || currentRaycastMode == RaycastMode.Warning))
        {
            var hitPose = s_Hits[0].pose;
            if (currentRaycastMode == RaycastMode.Stop && m_stopObject)
            {
                m_listOfReferences.Add(Instantiate(m_stopObject, hitPose.position, Quaternion.identity));
            }
            else if (currentRaycastMode == RaycastMode.Warning && m_warningObject)
            {
                m_listOfReferences.Add(Instantiate(m_warningObject, hitPose.position, Quaternion.identity));
            }
        }

        else if (m_raycastManager.Raycast(touch.position, s_Hits, TrackableType.Planes) && (currentRaycastMode == RaycastMode.Text))
        {
            var hitPose = s_Hits[0].pose;
            m_listOfReferences.Add(Instantiate(m_textObject, hitPose.position, Quaternion.identity));
        }
        //else if (currentRaycastMode == RaycastMode.Text)
        //{
        //    Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
        //    int layerMask = 1 << 8;
        //    RaycastHit hitInfo;
        //    if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, layerMask) && m_textObject)
        //    {
        //    }
        //}
    }

    TrackableId currentPlaneID = TrackableId.invalidId;

    private void OnlyOnePlane()
    {
        if (m_arPlaneManager && m_arPlaneManager.trackables.count > 1 && m_invisibleARPlane && m_arPlaneManager.planePrefab != m_invisibleARPlane)
        {
            int i = 0;
            m_defaultARPlane = m_arPlaneManager.planePrefab;
            m_arPlaneManager.planePrefab = m_invisibleARPlane;
            // Need a checck for both default and invisible game objects to have a plane component
            foreach (ARPlane plane in m_arPlaneManager.trackables)
            {
                if (i > 0)
                {
                    //plane.gameObject = m_invisibleARPlane.gameObject;
                    plane.gameObject.SetActive(false);
                    i++;
                }
                else
                {
                    plane.gameObject.SetActive(true);
                    currentPlaneID = plane.trackableId;
                }
            }
        }
    }

    public void ResetPlanes()
    {
        m_arPlaneManager.enabled = true;
        m_arPlaneManager.planePrefab = m_defaultARPlane;
        ARSession m_arSession = FindObjectOfType<ARSession>();
        m_arSession.Reset();
        currentRaycastMode = RaycastMode.None;
        AnchorCreator anchorCreator = GetComponent<AnchorCreator>();
        if (anchorCreator)
            anchorCreator.RemoveAllAnchors();
        SetRaycastMode(0);
        SceneManager.LoadScene(0);
    }

    public void ClearData()
    {
        Destroy(spawnedObject);
        spawnedObject = null;
        foreach (GameObject reference in m_listOfReferences)
            Destroy(reference);
    }

    public void LoadData()
    {
       
    }

    public void SetRaycastMode(int mode = 0)
    {
        currentRaycastMode = (RaycastMode)mode;
        for (int i = 0; i < m_spawnerButtons.Count; i++)
        {
            Color buttonColour = m_spawnerButtons[i].color;
            if (mode == i + 1)
            {
                buttonColour.a = 1f;
                m_spawnerButtons[i].color = buttonColour;
            }
            else
            {
                buttonColour.a = 0.5f;
                m_spawnerButtons[i].color = buttonColour;
            }
        }
    }
}
