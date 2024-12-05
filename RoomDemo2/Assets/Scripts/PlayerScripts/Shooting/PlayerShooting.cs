using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public GameObject bulletPrefab;       // Reference to the bullet prefab
    public float bulletSpeed = 20f;       // Speed of the bullet
    public float bulletLifetime = 2f;     // How long the bullet lasts before disappearing
    public Transform shootingPoint;       // Point from which bullets are instantiated

    void Update()
    {
        // Check if left mouse button is pressed
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        if (!GameManager.BattleCanvas.activeSelf)
        {
            // Instantiate bullet at the shooting point
            GameObject bullet = Instantiate(bulletPrefab, shootingPoint.position, Quaternion.identity);

            // Assign the bullet's speed for kinematic movement
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            bulletScript.Initialize(Camera.main.transform.forward, bulletSpeed);

            // Destroy the bullet after a set time
            Destroy(bullet, bulletLifetime);
        }
    }
}

