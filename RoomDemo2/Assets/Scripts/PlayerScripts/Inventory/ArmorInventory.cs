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

    [SerializeField] GameObject particleEffectPrefab;


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
        Debug.Log("Attempting to drop helmet");

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

        // Instantiate the prefab at the desired position with random rotation
        GameObject droppedHelmet = Instantiate(prefab, dropPosition, randomRotation);

        // Set the tag and layer for the dropped helmet
        droppedHelmet.tag = "Pickable";
        droppedHelmet.layer = LayerMask.NameToLayer("PickableObjects");

        // Adjust the drop height
        droppedHelmet.transform.position = new Vector3(
            droppedHelmet.transform.position.x,
            1.5f, // Adjusted height
            droppedHelmet.transform.position.z
        );

        // Scale the dropped helmet based on prefab size
        droppedHelmet.transform.localScale = prefab.transform.localScale;

        // Add required Rigidbody component if needed
        if (!droppedHelmet.TryGetComponent<Rigidbody>(out _))
        {
            var rb = droppedHelmet.AddComponent<Rigidbody>();
            rb.mass = 1f;
        }

        // Add BoxCollider to the dropped helmet if needed
        var prefabCollider = prefab.GetComponent<BoxCollider>();
        if (prefabCollider != null)
        {
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
            var collider = droppedHelmet.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.3f, 0.3f, 0.3f);
        }

        // Add the GenericPickableAction component if it's not already present
        if (!droppedHelmet.TryGetComponent<GenericPickableAction>(out _))
        {
            droppedHelmet.AddComponent<GenericPickableAction>();
        }

        // Particle effect for dropped helmet
        Vector3 prefabSize = prefab.transform.localScale;
        Vector3 particleOffset = new Vector3(0, prefabSize.y * 0.5f, 0); // Adjust for the height

        GameObject particleSystemInstance = Instantiate(
            particleEffectPrefab,
            droppedHelmet.transform.position + particleOffset,
            Quaternion.identity
        );

        particleSystemInstance.transform.localScale = prefabSize * 0.2f; // Scale particles relative to the prefab size
        particleSystemInstance.transform.SetParent(droppedHelmet.transform); // Make it a child of the dropped helmet

        // Play particle system
        ParticleSystem ps = particleSystemInstance.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Play();
        }

        // Clear the equipped helmet slot
        Debug.Log($"Dropped helmet: {HelmetSlot.ItemBase.Name}");
        HelmetSlot = Item.NoItem;

        // Load the empty slot icon
        string emptySlotPath = "InventorySystem/GDS/Resources/Shared/Icons/Equipment/64/helmet.png";
        Sprite emptySlotSprite = Resources.Load<Sprite>(emptySlotPath);
        if (emptySlotSprite != null)
        {
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
        Debug.Log("Attempting to drop body armor");

        if (BodyArmorSlot == null || BodyArmorSlot == Item.NoItem)
        {
            Debug.Log("No body armor equipped to drop.");
            return;
        }

        var prefab = PrefabRegistry.Instance?.GetPrefab(BodyArmorSlot.ItemBase.Id);
        if (prefab == null)
        {
            Debug.LogError($"No prefab found for body armor: {BodyArmorSlot.ItemBase.Name}");
            return;
        }

        Vector3 dropPosition = transform.position + transform.forward;
        Quaternion randomRotation = Quaternion.Euler(
            UnityEngine.Random.Range(0f, 360f),
            UnityEngine.Random.Range(0f, 360f),
            UnityEngine.Random.Range(0f, 360f)
        );

        GameObject droppedBodyArmor = Instantiate(prefab, dropPosition, randomRotation);

        droppedBodyArmor.tag = "Pickable";
        droppedBodyArmor.layer = LayerMask.NameToLayer("PickableObjects");

        droppedBodyArmor.transform.position = new Vector3(
            droppedBodyArmor.transform.position.x,
            1.5f,
            droppedBodyArmor.transform.position.z
        );

        droppedBodyArmor.transform.localScale = prefab.transform.localScale;

        if (!droppedBodyArmor.TryGetComponent<Rigidbody>(out _))
        {
            var rb = droppedBodyArmor.AddComponent<Rigidbody>();
            rb.mass = 1f;
        }

        var prefabCollider = prefab.GetComponent<BoxCollider>();
        if (prefabCollider != null)
        {
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
            var collider = droppedBodyArmor.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.3f, 0.3f, 0.3f);
        }

        if (!droppedBodyArmor.TryGetComponent<GenericPickableAction>(out _))
        {
            droppedBodyArmor.AddComponent<GenericPickableAction>();
        }

        // Particle effect
        Vector3 prefabSize = prefab.transform.localScale;
        Vector3 particleOffset = new Vector3(0, prefabSize.y * 0.5f, 0);

        GameObject particleSystemInstance = Instantiate(
            particleEffectPrefab,
            droppedBodyArmor.transform.position + particleOffset,
            Quaternion.identity
        );

        particleSystemInstance.transform.localScale = prefabSize * 0.2f;
        particleSystemInstance.transform.SetParent(droppedBodyArmor.transform);

        ParticleSystem ps = particleSystemInstance.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Play();
        }

        Debug.Log($"Dropped body armor: {BodyArmorSlot.ItemBase.Name}");
        BodyArmorSlot = Item.NoItem;

        string emptySlotPath = "InventorySystem/GDS/Resources/Shared/Icons/Equipment/64/body-armor.png";
        Sprite emptySlotSprite = Resources.Load<Sprite>(emptySlotPath);
        if (emptySlotSprite != null)
        {
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
        Debug.Log("Attempting to drop boots");

        if (BootsSlot == null || BootsSlot == Item.NoItem)
        {
            Debug.Log("No boots equipped to drop.");
            return;
        }

        var prefab = PrefabRegistry.Instance?.GetPrefab(BootsSlot.ItemBase.Id);
        if (prefab == null)
        {
            Debug.LogError($"No prefab found for boots: {BootsSlot.ItemBase.Name}");
            return;
        }

        Vector3 dropPosition = transform.position + transform.forward;
        Quaternion randomRotation = Quaternion.Euler(
            UnityEngine.Random.Range(0f, 360f),
            UnityEngine.Random.Range(0f, 360f),
            UnityEngine.Random.Range(0f, 360f)
        );

        GameObject droppedBoots = Instantiate(prefab, dropPosition, randomRotation);

        droppedBoots.tag = "Pickable";
        droppedBoots.layer = LayerMask.NameToLayer("PickableObjects");

        droppedBoots.transform.position = new Vector3(
            droppedBoots.transform.position.x,
            1.5f,
            droppedBoots.transform.position.z
        );

        droppedBoots.transform.localScale = prefab.transform.localScale;

        if (!droppedBoots.TryGetComponent<Rigidbody>(out _))
        {
            var rb = droppedBoots.AddComponent<Rigidbody>();
            rb.mass = 1f;
        }

        var prefabCollider = prefab.GetComponent<BoxCollider>();
        if (prefabCollider != null)
        {
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
            var collider = droppedBoots.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.3f, 0.3f, 0.3f);
        }

        if (!droppedBoots.TryGetComponent<GenericPickableAction>(out _))
        {
            droppedBoots.AddComponent<GenericPickableAction>();
        }

        // Particle effect
        Vector3 prefabSize = prefab.transform.localScale;
        Vector3 particleOffset = new Vector3(0, prefabSize.y * 0.5f, 0);

        GameObject particleSystemInstance = Instantiate(
            particleEffectPrefab,
            droppedBoots.transform.position + particleOffset,
            Quaternion.identity
        );

        particleSystemInstance.transform.localScale = prefabSize * 0.2f;
        particleSystemInstance.transform.SetParent(droppedBoots.transform);

        ParticleSystem ps = particleSystemInstance.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Play();
        }

        Debug.Log($"Dropped boots: {BootsSlot.ItemBase.Name}");
        BootsSlot = Item.NoItem;

        string emptySlotPath = "InventorySystem/GDS/Resources/Shared/Icons/Equipment/64/boots.png";
        Sprite emptySlotSprite = Resources.Load<Sprite>(emptySlotPath);
        if (emptySlotSprite != null)
        {
            BootsImage1.GetComponent<UnityEngine.UI.Image>().sprite = emptySlotSprite;
            BootsImage2.GetComponent<UnityEngine.UI.Image>().sprite = emptySlotSprite;
            Debug.Log("Boots slot set to empty icon.");
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
