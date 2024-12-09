using System.Collections;
using UnityEngine;
using System.Linq;
using GDS.Minimal;
using GDS.Core; // Ensure the namespace containing your Bag and Item classes is included

public class PlayerShooting : MonoBehaviour
{
    [SerializeField] GameObject pauseUI;
    [SerializeField] GameObject inventoryUI;

    public GameObject bulletPrefab;       // Reference to the bullet prefab
    public float bulletSpeed = 20f;       // Speed of the bullet
    public float bulletLifetime = 2f;     // How long the bullet lasts before disappearing
    public Transform shootingPoint;       // Point from which bullets are instantiated

    void Update()
    {
        // Check if left mouse button is pressed
        if (!pauseUI.active && !inventoryUI.active && Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        // Check if BattleCanvas is inactive
        if (!GameManager.BattleCanvas.activeSelf)
        {
            // Get the player's inventory (main inventory from Store)
            var mainInventory = Store.Instance.MainInventory;

            // Find the stone item in the inventory
            var stoneItem = mainInventory.Slots
                .Select(slot => slot.Item)
                .FirstOrDefault(item => item.ItemBase.Id == "Stone");

            if (stoneItem != null && mainInventory.RemoveItem(stoneItem))
            {
                Debug.Log("Stone item used for shooting!");

                // Instantiate bullet at the shooting point
                GameObject bullet = Instantiate(bulletPrefab, shootingPoint.position, Quaternion.identity);

                // Assign the bullet's speed for kinematic movement
                Bullet bulletScript = bullet.GetComponent<Bullet>();
                bulletScript.Initialize(Camera.main.transform.forward, bulletSpeed);

                // Destroy the bullet after a set time
                Destroy(bullet, bulletLifetime);
            }
            else
            {
                Debug.Log("No stone item in inventory or failed to remove it!");
            }
        }
    }
}
