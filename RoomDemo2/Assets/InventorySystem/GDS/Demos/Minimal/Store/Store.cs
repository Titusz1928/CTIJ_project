using UnityEngine;
using GDS.Sample;
using GDS.Core;
using GDS.Core.Events;
using static GDS.Core.LogUtil;
using static GDS.Core.InventoryFactory;
using static GDS.Core.InventoryExtensions;
using static GDS.Sample.ItemFactory;
using System.Text;
using System.Collections.Generic;

namespace GDS.Minimal {

    /// <summary>
    /// `Store` is a singleton that contains all the system state, listens to UI events and updates the state
    /// Only the Store can modify it's internal state
    /// </summary>
    public class Store {
        public Store() {
            // Listen (subscribe) to reset event - it is triggered when entering play mode
            EventBus.GlobalBus.Subscribe<ResetEvent>(e => Reset());
            // Listen to pick and place UI events
            Bus.Subscribe<PickItemEvent>(e => OnPickItem(e as PickItemEvent));
            Bus.Subscribe<PlaceItemEvent>(e => OnPlaceItem(e as PlaceItemEvent));
            // Initilize by calling Reset
            Reset();
        }

        // Store singleton instance
        public static readonly Store Instance = new();
        // Event bus - the channel used to pass all the events
        public readonly EventBus Bus = new();
        // Main inventory state
        public readonly ListBag MainInventory = CreateListBag("MainInventory", 40);
        // DraggedItem contains info about the item being dragged (not a reference)
        // Views and behaviors subscribe to this and react by re-rendering or triggering other flows

        //TITUSZ
        public readonly SetBag Equipment = CreateSetBag("Equipment", DB.EquipmentSlots);




        public readonly Observable<Item> DraggedItem = new(Item.NoItem);
        

        /// <summary>
        /// Resets the Store state
        /// Sets the inventory state by creating items and adding them to the list
        /// </summary>
        void Reset() {
            Debug.Log($"Reseting ".Yellow() + "[Basic Store]".Gray());
            MainInventory.SetState(
                  Create(BaseId.Apple, Rarity.Common),
                  Create(BaseId.Apple, Rarity.Common),
                  Create(BaseId.Apple, Rarity.Common),
                  Create(BaseId.LeatherArmor,Rarity.Unique),
                  Create(BaseId.WarriorHelmet, Rarity.Rare),
                  Create(BaseId.SteelBoots, Rarity.Legendary)
            );
        }

        /// <summary>
        /// PickItem event handler
        /// If picking is successful, sets the new state of Dragged Item and notifies the 
        /// inventory from which the item was picked
        /// </summary>
        /// <param name="e"></param>
        void OnPickItem(PickItemEvent e) {
            LogEvent(e);
            var (success, replacedItem) = e.Bag.PickItem(e.Item, e.Slot);
            if (!success) return;
            DraggedItem.SetValue(replacedItem);
            e.Bag.Notify();
        }

        /// <summary>
        /// PlaceItem event handler
        /// If placing is successful, sets the new state of the Dragged Item and notifies the
        /// inventory in which the item was placed
        /// </summary>
        /// <param name="e"></param>
        void OnPlaceItem(PlaceItemEvent e) {
            LogEvent(e);
            var (success, replacedItem) = e.Bag.PlaceItem(e.Item, e.Slot);
            if (!success) return;
            DraggedItem.SetValue(replacedItem);
            e.Bag.Notify();
        }

        public string LogMainInventoryItems()
        {
            StringBuilder logBuilder = new StringBuilder();
            logBuilder.AppendLine("MainInventory Items:");

            foreach (var slot in MainInventory.Slots)
            {
                if (slot.Item is not NoItem)
                {
                    logBuilder.AppendLine($"{slot.Item.Name()}");
                }
/*                else
                {
                    logBuilder.AppendLine($"Slot {slot.Index}: Empty");
                }*/
            }

            return logBuilder.ToString();
        }

        public string[] getMainInventoryWeapons()
        {
            List<string> weaponsList = new List<string>(); // Create a list to hold the item names

            foreach (var slot in MainInventory.Slots)
            {
                // Assuming "Weapon" is the class or type that represents weapons
                if (slot.Item is not NoItem && slot.Item.Class() == ItemClass.Weapon1H)
                {
                    weaponsList.Add(slot.Item.Name()); // Add the item name to the list
                }
            }

            return weaponsList.ToArray(); // Convert the list to an array and return it
        }

        public string[] getMainInventoryConsumables()
        {
            List<string> consumablesList = new List<string>(); // Create a list to hold the item names

            foreach (var slot in MainInventory.Slots)
            {
                // Assuming "Consumable" is the class or type that represents consumables
                if (slot.Item is not NoItem && slot.Item.Class() == ItemClass.Consumable)
                {
                    consumablesList.Add(slot.Item.Name()); // Add the item name to the list
                }
            }

            return consumablesList.ToArray(); // Convert the list to an array and return it
        }

        public string[] getMainInventoryMaterials()
        {
            List<string> materialsList = new List<string>(); // Create a list to hold the item names

            foreach (var slot in MainInventory.Slots)
            {
                // Assuming "Material" is the class or type that represents materials
                if (slot.Item is not NoItem && slot.Item.Class() == ItemClass.Material)
                {
                    materialsList.Add(slot.Item.Name()); // Add the item name to the list
                }
            }

            return materialsList.ToArray(); // Convert the list to an array and return it
        }

    }
}