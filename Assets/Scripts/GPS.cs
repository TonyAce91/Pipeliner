using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;
using System;

/// <summary>
/// This is used to display GPS coordinates extractedd fro mphone information
/// Code written by Antoine Kenneth Odi in 2020
/// </summary>
public class GPS : MonoBehaviour
{
    public static GPS Instance { set; get; }

    public Text gpsText = null;

    public float latitude;
    public float longitude;
    public float altitude;

    private void Start()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        StartCoroutine(StartLocationService());
    }

    private void Update()
    {
        gpsText.text = "Long: " + longitude + "\nLat: " + latitude + "\nAlt: " + altitude;
    }

    private IEnumerator StartLocationService()
    {
        if (!Input.location.isEnabledByUser)
        {
            Debug.Log("User has not enabled GPS");
            yield break;
        }

        Input.location.Start();

        // Timeout
        int maxWait = 20;

        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait <= 0)
        {
            Debug.Log("Timed out");
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.Log("Unable to determine device location");
            yield break;
        }

        latitude = Input.location.lastData.latitude;
        longitude = Input.location.lastData.longitude;
        altitude = Input.location.lastData.altitude;

        yield break;
    }
}
