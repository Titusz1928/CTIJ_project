using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    
    private Vector3 direction;
    private float speed;
    [SerializeField]
    public float damageAmount = 30f;

    public void Initialize(Vector3 direction, float speed)
    {
        this.direction = direction.normalized;
        this.speed = speed;
    }

    void FixedUpdate()
    {
        // Move the bullet in the specified direction
        transform.position += direction * speed * Time.fixedDeltaTime;
    }

    void OnCollisionEnter(Collision collision)
    {
        HealthManager healthManager = collision.gameObject.GetComponent<HealthManager>();
        if (healthManager != null)
        {
            // If the object has HealthManager, decrease its health
            healthManager.DecreaseHealth(damageAmount);
            //Debug.Log($"{collision.gameObject.name} hit by bullet! Health decreased.");
        }

        // Destroy the bullet upon collision
        Destroy(gameObject);
    }
}
