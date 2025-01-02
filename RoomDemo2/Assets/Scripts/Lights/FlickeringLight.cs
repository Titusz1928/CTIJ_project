using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlickeringLight : MonoBehaviour
{
    private Light pointLight;
    public float minIntensity = 1f;
    public float maxIntensity = 2f;

    void Start()
    {
        pointLight = GetComponent<Light>();
    }

    void Update()
    {
        if (pointLight != null)
        {
            pointLight.intensity = Random.Range(minIntensity, maxIntensity);
        }
    }
}

