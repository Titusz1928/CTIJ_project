using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GDS.Minimal;
using GDS.Core;
using GDS.Sample;

public class ArmorInventory : MonoBehaviour
{
    // Slots to hold equipped armor pieces
    public Item HelmetSlot { get; private set; } = Item.NoItem;
    public Item BodyArmorSlot { get; private set; } = Item.NoItem;
    public Item BootsSlot { get; private set; } = Item.NoItem;

    [SerializeField] GameObject HelmetImage1;
    [SerializeField] GameObject HelmetImage2;
    [SerializeField] GameObject BodyArmorImage1;
    [SerializeField] GameObject BodyArmorImage2;
    [SerializeField] GameObject BootsImage1;
    [SerializeField] GameObject BootsImage2;


    // Equip an armor piece into the correct slot
    public void EquipArmor(Item armorItem)
    {
        if (armorItem == null || armorItem == Item.NoItem)
        {
            Debug.LogError("Cannot equip an empty or invalid armor item.");
            return;
        }

        switch (armorItem.ItemBase.ItemClass)
        {
            case GDS.Sample.ItemClass.Helmet:
                EquipHelmet(armorItem);
                break;

            case GDS.Sample.ItemClass.BodyArmor:
                EquiBodyArmor(armorItem);
                break;

            case GDS.Sample.ItemClass.Boots:
                EquipBoots(armorItem);
                break;

            default:
                Debug.LogError("This item cannot be equipped as armor.");
                break;
        }
    }

    public void EquipHelmet(Item item)
    {
        if (HelmetSlot != null && HelmetSlot != Item.NoItem)
        {
            DropHelmet();
            Debug.Log($"Replaced helmet: {HelmetSlot.ItemBase.Id} with new helmet: {item.ItemBase.Id}");
        }
        else
        {
            Debug.Log($"Equipped helmet: {item.ItemBase.Id}");
        }

        string iconPath = item.ItemBase.IconPath;
        Sprite helmetSprite = Resources.Load<Sprite>(iconPath);
        if (helmetSprite != null)
        {
            Debug.Log($"Helmet icon loaded successfully: {iconPath}");
            HelmetImage1.GetComponent<UnityEngine.UI.Image>().sprite = helmetSprite;
            HelmetImage2.GetComponent<UnityEngine.UI.Image>().sprite = helmetSprite;
        }
        else
        {
            Debug.LogError($"Failed to load helmet icon at path: {iconPath}");
        }

        HelmetSlot = item;
    }

    public void EquiBodyArmor(Item item)
    {
        if (BodyArmorSlot != null && BodyArmorSlot != Item.NoItem)
        {
            DropBodyArmor();
            Debug.Log($"Replaced body armor: {BodyArmorSlot.ItemBase.Id} with new body armor: {item.ItemBase.Id}");
        }
        else
        {
            Debug.Log($"Equipped body armor: {item.ItemBase.Id}");
        }
        string iconPath = item.ItemBase.IconPath;
        Sprite bodyArmorSprite = Resources.Load<Sprite>(iconPath);
        if (bodyArmorSprite != null)
        {
            Debug.Log($"Armor icon loaded successfully: {iconPath}");
            BodyArmorImage1.GetComponent<UnityEngine.UI.Image>().sprite = bodyArmorSprite;
            BodyArmorImage2.GetComponent<UnityEngine.UI.Image>().sprite = bodyArmorSprite;
        }
        else
        {
            Debug.LogError($"Failed to load armor icon at path: {iconPath}");
        }

        BodyArmorSlot = item;
    }

    public void EquipBoots(Item item)
    {
        if (BootsSlot != null && BootsSlot != Item.NoItem)
        {
            DropBoots();
            Debug.Log($"Replaced boots: {BootsSlot.ItemBase.Id} with new boots: {item.ItemBase.Id}");
        }
        else
        {
            Debug.Log($"Equipped boots: {item.ItemBase.Id}");
        }
        string iconPath = item.ItemBase.IconPath;
        Sprite bootsSprite = Resources.Load<Sprite>(iconPath);
        if (bootsSprite != null)
        {
            Debug.Log($"Boot icon loaded successfully: {iconPath}");
            BootsImage1.GetComponent<UnityEngine.UI.Image>().sprite = bootsSprite;
            BootsImage2.GetComponent<UnityEngine.UI.Image>().sprite = bootsSprite;
        }
        else
        {
            Debug.LogError($"Failed to load boot icon at path: {iconPath}");
        }

        BootsSlot = item;
    }

    public void DropHelmet()
    {
        Debug.Log("attempting to drop helmet");
        if (HelmetSlot == null || HelmetSlot == Item.NoItem)
        {
            Debug.Log("No helmet equipped to drop.");
            return;
        }

        // Get the prefab associated with the helmet's ID
        var prefab = PrefabRegistry.Instance?.GetPrefab(HelmetSlot.ItemBase.Id);
        if (prefab == null)
        {
            Debug.LogError($"No prefab found for helmet: {HelmetSlot.ItemBase.Name}");
            return;
        }

        // Determine the drop position relative to the player
        Vector3 dropPosition = transform.position + transform.forward;

        // Generate a random rotation
        Quaternion randomRotation = Quaternion.Euler(
            UnityEngine.Random.Range(0f, 360f), // Random X rotation
            UnityEngine.Random.Range(0f, 360f), // Random Y rotation
            UnityEngine.Random.Range(0f, 360f)  // Random Z rotation
        );

        // Instantiate the helmet prefab at the desired position with random rotation
        GameObject droppedHelmet = Instantiate(prefab, dropPosition, randomRotation);

        // Set the tag and layer for the dropped helmet
        droppedHelmet.tag = "Pickable";
        droppedHelmet.layer = LayerMask.NameToLayer("PickableObjects");

        // Adjust the drop height
        droppedHelmet.transform.position = new Vector3(
            droppedHelmet.transform.position.x,
            1.5f,
            droppedHelmet.transform.position.z
        );

        // Scale the dropped helmet if necessary
        droppedHelmet.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        // Add required Rigidbody component
        if (!droppedHelmet.TryGetComponent<Rigidbody>(out _))
        {
            var rb = droppedHelmet.AddComponent<Rigidbody>();
            rb.mass = 1f;
        }

        // Add BoxCollider to the dropped helmet if needed
        var prefabCollider = prefab.GetComponent<BoxCollider>();
        if (prefabCollider != null)
        {
            Debug.Log("Collider found for helmet prefab.");
            BoxCollider droppedCollider = droppedHelmet.GetComponent<BoxCollider>();
            if (droppedCollider == null)
            {
                droppedCollider = droppedHelmet.AddComponent<BoxCollider>();
            }
            droppedCollider.center = prefabCollider.center;
            droppedCollider.size = prefabCollider.size;
        }
        else
        {
            Debug.Log("No collider found on helmet prefab. Adding default collider.");
            var collider = droppedHelmet.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.3f, 0.3f, 0.3f);
        }

        // Add the GenericPickableAction component if it's not already present
        if (!droppedHelmet.TryGetComponent<GenericPickableAction>(out _))
        {
            droppedHelmet.AddComponent<GenericPickableAction>();
        }

        // Clear the equipped helmet slot
        Debug.Log($"Dropped helmet: {HelmetSlot.ItemBase.Name}");
        HelmetSlot = Item.NoItem;

        // Load the empty slot icon
        string emptySlotPath = "InventorySystem/GDS/Resources/Shared/Icons/Equipment/64/helmet.png";
        Sprite emptySlotSprite = Resources.Load<Sprite>(emptySlotPath);
        if (emptySlotSprite != null)
        {
            // Set the UI images to the empty slot icon
            HelmetImage1.GetComponent<UnityEngine.UI.Image>().sprite = emptySlotSprite;
            HelmetImage2.GetComponent<UnityEngine.UI.Image>().sprite = emptySlotSprite;
            Debug.Log("Helmet slot set to empty icon.");
        }
        else
        {
            Debug.LogError($"Failed to load empty slot icon at path: {emptySlotPath}");
        }
    }

    public void DropBodyArmor()
    {
        Debug.Log("attempting to drop body armor");
        if (BodyArmorSlot == null || BodyArmorSlot == Item.NoItem)
        {
            Debug.Log("No helmet equipped to drop.");
            return;
        }

        // Get the prefab associated with the helmet's ID
        var prefab = PrefabRegistry.Instance?.GetPrefab(BodyArmorSlot.ItemBase.Id);
        if (prefab == null)
        {
            Debug.LogError($"No prefab found for body armor: {BodyArmorSlot.ItemBase.Name}");
            return;
        }

        // Determine the drop position relative to the player
        Vector3 dropPosition = transform.position + transform.forward;

        // Generate a random rotation
        Quaternion randomRotation = Quaternion.Euler(
            UnityEngine.Random.Range(0f, 360f), // Random X rotation
            UnityEngine.Random.Range(0f, 360f), // Random Y rotation
            UnityEngine.Random.Range(0f, 360f)  // Random Z rotation
        );

        // Instantiate the helmet prefab at the desired position with random rotation
        GameObject droppedBodyArmor = Instantiate(prefab, dropPosition, randomRotation);

        // Set the tag and layer for the dropped helmet
        droppedBodyArmor.tag = "Pickable";
        droppedBodyArmor.layer = LayerMask.NameToLayer("PickableObjects");

        // Adjust the drop height
        droppedBodyArmor.transform.position = new Vector3(
            droppedBodyArmor.transform.position.x,
            1.5f,
            droppedBodyArmor.transform.position.z
        );

        // Scale the dropped helmet if necessary
        droppedBodyArmor.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        // Add required Rigidbody component
        if (!droppedBodyArmor.TryGetComponent<Rigidbody>(out _))
        {
            var rb = droppedBodyArmor.AddComponent<Rigidbody>();
            rb.mass = 1f;
        }

        // Add BoxCollider to the dropped helmet if needed
        var prefabCollider = prefab.GetComponent<BoxCollider>();
        if (prefabCollider != null)
        {
            Debug.Log("Collider found for body armor prefab.");
            BoxCollider droppedCollider = droppedBodyArmor.GetComponent<BoxCollider>();
            if (droppedCollider == null)
            {
                droppedCollider = droppedBodyArmor.AddComponent<BoxCollider>();
            }
            droppedCollider.center = prefabCollider.center;
            droppedCollider.size = prefabCollider.size;
        }
        else
        {
            Debug.Log("No collider found on body armor prefab. Adding default collider.");
            var collider = droppedBodyArmor.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.3f, 0.3f, 0.3f);
        }

        // Add the GenericPickableAction component if it's not already present
        if (!droppedBodyArmor.TryGetComponent<GenericPickableAction>(out _))
        {
            droppedBodyArmor.AddComponent<GenericPickableAction>();
        }

        // Clear the equipped helmet slot
        Debug.Log($"Dropped body armor: {BodyArmorSlot.ItemBase.Name}");
        BodyArmorSlot = Item.NoItem;

        // Load the empty slot icon
        string emptySlotPath = "InventorySystem/GDS/Resources/Shared/Icons/Equipment/64/body-armor.png";
        Sprite emptySlotSprite = Resources.Load<Sprite>(emptySlotPath);
        if (emptySlotSprite != null)
        {
            // Set the UI images to the empty slot icon
            BodyArmorImage1.GetComponent<UnityEngine.UI.Image>().sprite = emptySlotSprite;
            BodyArmorImage2.GetComponent<UnityEngine.UI.Image>().sprite = emptySlotSprite;
            Debug.Log("Body armor slot set to empty icon.");
        }
        else
        {
            Debug.LogError($"Failed to load empty slot icon at path: {emptySlotPath}");
        }
    }

    public void DropBoots()
    {
        Debug.Log("attempting to drop boots");
        if (BootsSlot == null || BootsSlot == Item.NoItem)
        {
            Debug.Log("No boots equipped to drop.");
            return;
        }

        // Get the prefab associated with the helmet's ID
        var prefab = PrefabRegistry.Instance?.GetPrefab(BootsSlot.ItemBase.Id);
        if (prefab == null)
        {
            Debug.LogError($"No prefab found for boots: {BootsSlot.ItemBase.Name}");
            return;
        }

        // Determine the drop position relative to the player
        Vector3 dropPosition = transform.position + transform.forward;

        // Generate a random rotation
        Quaternion randomRotation = Quaternion.Euler(
            UnityEngine.Random.Range(0f, 360f), // Random X rotation
            UnityEngine.Random.Range(0f, 360f), // Random Y rotation
            UnityEngine.Random.Range(0f, 360f)  // Random Z rotation
        );

        // Instantiate the helmet prefab at the desired position with random rotation
        GameObject droppedBoots = Instantiate(prefab, dropPosition, randomRotation);

        // Set the tag and layer for the dropped helmet
        droppedBoots.tag = "Pickable";
        droppedBoots.layer = LayerMask.NameToLayer("PickableObjects");

        // Adjust the drop height
        droppedBoots.transform.position = new Vector3(
            droppedBoots.transform.position.x,
            1.5f,
            droppedBoots.transform.position.z
        );

        // Scale the dropped helmet if necessary
        droppedBoots.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        // Add required Rigidbody component
        if (!droppedBoots.TryGetComponent<Rigidbody>(out _))
        {
            var rb = droppedBoots.AddComponent<Rigidbody>();
            rb.mass = 1f;
        }

        // Add BoxCollider to the dropped helmet if needed
        var prefabCollider = prefab.GetComponent<BoxCollider>();
        if (prefabCollider != null)
        {
            Debug.Log("Collider found for body armor prefab.");
            BoxCollider droppedCollider = droppedBoots.GetComponent<BoxCollider>();
            if (droppedCollider == null)
            {
                droppedCollider = droppedBoots.AddComponent<BoxCollider>();
            }
            droppedCollider.center = prefabCollider.center;
            droppedCollider.size = prefabCollider.size;
        }
        else
        {
            Debug.Log("No collider found on body armor prefab. Adding default collider.");
            var collider = droppedBoots.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.3f, 0.3f, 0.3f);
        }

        // Add the GenericPickableAction component if it's not already present
        if (!droppedBoots.TryGetComponent<GenericPickableAction>(out _))
        {
            droppedBoots.AddComponent<GenericPickableAction>();
        }

        // Clear the equipped helmet slot
        Debug.Log($"Dropped boot: {BootsSlot.ItemBase.Name}");
        BootsSlot = Item.NoItem;

        // Load the empty slot icon
        string emptySlotPath = "InventorySystem/GDS/Resources/Shared/Icons/Equipment/64/boots.png";
        Sprite emptySlotSprite = Resources.Load<Sprite>(emptySlotPath);
        if (emptySlotSprite != null)
        {
            // Set the UI images to the empty slot icon
            BootsImage1.GetComponent<UnityEngine.UI.Image>().sprite = emptySlotSprite;
            BootsImage1.GetComponent<UnityEngine.UI.Image>().sprite = emptySlotSprite;
            Debug.Log("Boot slot set to empty icon.");
        }
        else
        {
            Debug.LogError($"Failed to load empty slot icon at path: {emptySlotPath}");
        }
    }

    // Debugging: Display current equipped armor
    public void DisplayEquippedArmor()
    {
        Debug.Log($"Helmet: {(HelmetSlot == Item.NoItem ? "None" : HelmetSlot.ItemBase.Id)}");
        Debug.Log($"Body Armor: {(BodyArmorSlot == Item.NoItem ? "None" : BodyArmorSlot.ItemBase.Id)}");
        Debug.Log($"Boots: {(BootsSlot == Item.NoItem ? "None" : BootsSlot.ItemBase.Id)}");
    }
}
