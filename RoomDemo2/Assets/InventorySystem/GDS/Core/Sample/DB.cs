using System;
using System.Collections.Generic;
using System.Linq;
using GDS.Core;
using static GDS.Core.Size;
using static GDS.Sample.BaseId;
namespace GDS.Sample {
    /// <summary>
    /// A Database/Repository of items
    /// Contains all the items that can be created (see ItemFactory in InventoryExt.cs)
    /// Contains enums for aspects of an item or slot (type, class rarity)
    /// Contains an array with possible equipment slots
    /// </summary>
    public static class DB {
        public static List<SampleItemBase> AllBases = new() {
            new (WarriorHelmet, "Warrior Helmet", "Shared/Icons/helmet", false,  Size2x2, ItemClass.Helmet),
            new (SteelBoots, "SteelBoots", "Shared/Icons/boots", false, Size2x2, ItemClass.Boots),
            new (LeatherArmor, "LeatherArmor", "Shared/Icons/armor", false, Size2x3, ItemClass.BodyArmor),
            new (SteelArmor, "SteelArmor", "Shared/Icons/steelarmor", false, Size2x3, ItemClass.BodyArmor),
            new (ShortSword, "ShortSword", "Shared/Icons/sword", false, Size1x2, ItemClass.Weapon1H),
            new (Apple, "Apple", "Shared/Icons/apple", true, Size1x1, ItemClass.Consumable),
            new (Club,"Club","Shared/Icons/club",false,Size1x1,ItemClass.Weapon1H),
            new (Wood, "Wood", "Shared/Icons/wood", true, Size1x1, ItemClass.Material),
            new (Stone, "Stone","Shared/Icons/stone",true,Size1x1,ItemClass.Material),
            new (CookedPorkchop,"CookedPorkchop","Shared/Icons/cookedporkchop",true,Size1x1,ItemClass.Consumable),
            new (Goldcoin,"Goldcoin","Shared/Icons/goldcoin",true,Size1x1,ItemClass.Valuable),
            new (RawDogMeat,"RawDogMeat","Shared/Icons/rawdogmeat",true,Size1x1,ItemClass.Consumable),
        };

        public static Dictionary<BaseId, SampleItemBase> AllBasesDict = AllBases.ToDictionary(x => x.BaseId);

        public static SampleSetSlot[] EquipmentSlots = Array.ConvertAll<SlotType, SampleSetSlot>(
            EnumUtil.GetAllEnumValues<SlotType>(),
            value => new SampleSetSlot(value) { Accepts = Accepts.Equipment(value) }
        );

    }

    /// <summary>
    /// Item Base Id 
    /// Used when creating an item
    /// </summary>
    public enum BaseId {
        WarriorHelmet,
        LeatherArmor,
        SteelArmor,
        SteelBoots,
        ShortSword,
        Apple,
        Wood,
        Stone,
        Club,
        CookedPorkchop,
        Goldcoin,
        RawDogMeat
    }

    /// <summary>
    /// Slot Type
    /// Used when creating an equipment restricted slot (see SetSlot)
    /// </summary>
    public enum SlotType {
        Helmet,
        Gloves,
        BodyArmor,
        Boots,
        Weapon,
    }

    /// <summary>
    /// Item Class
    /// Used to check if item can be placed in a particular slot (see Accepts)
    /// </summary>
    public enum ItemClass {
        NoItemClass,
        Helmet,
        Gloves,
        BodyArmor,
        Boots,
        Weapon1H,
        Consumable,
        Material,
        Valuable
    }

    /// <summary>
    /// Rarity
    /// Used in views (mostly) to render item or tooltip background using a color associated with the rarity
    /// </summary>
    public enum Rarity {
        NoRarity,
        Common,
        Magic,
        Rare,
        Unique,
        Legendary,
        Set,
        Epic
    }
}