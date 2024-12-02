using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttractionManager : MonoBehaviour
{
    public float attractionRadius = 5f;  // Default attraction radius
    public float lifetime = 20f;         // Default lifetime of the attraction object

    private void Start()
    {
        // Set a timer to destroy the object after its lifetime
        Destroy(gameObject, lifetime);
    }

    private void OnDrawGizmos()
    {
        // Visualize the attraction radius in the Scene view
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attractionRadius);
    }
}

