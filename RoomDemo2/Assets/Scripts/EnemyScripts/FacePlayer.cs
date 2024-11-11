using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    private Transform mainCamera; // Reference to the main camera's transform

    void Start()
    {
        // Find the main camera in the scene
        mainCamera = Camera.main.transform;
    }

    void Update()
    {
        if (mainCamera != null)
        {
            // Calculate the direction from the object to the camera
            Vector3 direction = mainCamera.position - transform.position;

            // Only use the X and Z components to calculate the angle for Y-axis rotation
            direction.y = 0; // Keep the rotation only around the Y-axis

            // Calculate the angle on the Y-axis using Atan2, based on the X and Z components
            float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

            // Rotate the object around the Y-axis
            transform.rotation = Quaternion.Euler(new Vector3(0, angle, 0));
        }
    }
}
