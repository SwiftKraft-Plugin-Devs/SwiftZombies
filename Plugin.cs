using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using PluginAPI.Helpers;
using SwiftAPI.API.CustomItems;
using SwiftShops.API;
using SwiftZombies.Core;
using System.IO;

namespace SwiftZombies
{
    public class Plugin
    {
        public static Plugin Instance;

        public static readonly string PluginFolder = Path.Combine(Paths.LocalPlugins.Plugins, Name);

        public const string Author = "SwiftKraft";

        public const string Name = "SwiftZombies";

        public const string Description = "CoD Zombies in SCP: SL. ";

        public const string Version = "Alpha v0.0.1";

        [PluginPriority(LoadPriority.Lowest)]
        [PluginEntryPoint(Name, Version, Description, Author)]
        public void Init()
        {
            Instance = this;

            EventManager.RegisterEvents<EventHandler>(this);

            Log.Info("SwiftZombies Loaded! Version: " + Version);

            ShopProfile shop = new() { ID = "SZOMBIES", DisplayName = "Swift Zombies Shop" };

            CustomItemManager.RegisterItem("GUNMAN1", new AllyCard() { BaseItem = ItemType.KeycardJanitor, Description = "Spawns a Tier 1 Gun Man", DisplayName = "Tier 1 Gun Man", Items = [ItemType.GunCOM18] });
            CustomItemManager.RegisterItem("GUNMAN2", new AllyCard() { BaseItem = ItemType.KeycardJanitor, Description = "Spawns a Tier 2 Gun Man", DisplayName = "Tier 2 Gun Man", Items = [ItemType.GunCrossvec, ItemType.Painkillers] });
            CustomItemManager.RegisterItem("GUNMAN3", new AllyCard() { BaseItem = ItemType.KeycardJanitor, Description = "Spawns a Tier 3 Gun Man", DisplayName = "Tier 3 Gun Man", Items = [ItemType.GunE11SR, ItemType.Medkit] });
            CustomItemManager.RegisterItem("GUNMAN4", new AllyCard() { BaseItem = ItemType.KeycardJanitor, Description = "Spawns a Tier 4 Gun Man", DisplayName = "Tier 4 Gun Man", Items = [ItemType.GunFRMG0, ItemType.Medkit, ItemType.Medkit] });
            CustomItemManager.RegisterItem("FLASHMAN", new AllyCard() { BaseItem = ItemType.KeycardJanitor, Description = "Spawns a Flashbang Man", DisplayName = "Flashbang Man", Items = [ItemType.GrenadeFlash, ItemType.Medkit] });

            shop.AddItem(new CustomShopItem() { Item = "GUNMAN1".GetCustomItemWithID(), ID = "ALLY1", Price = 500 });
            shop.AddItem(new CustomShopItem() { Item = "GUNMAN2".GetCustomItemWithID(), ID = "ALLY2", Price = 1000 });
            shop.AddItem(new CustomShopItem() { Item = "GUNMAN3".GetCustomItemWithID(), ID = "ALLY3", Price = 2000 });
            shop.AddItem(new CustomShopItem() { Item = "GUNMAN4".GetCustomItemWithID(), ID = "ALLY4", Price = 3000 });
            shop.AddItem(new CustomShopItem() { Item = "FLASHMAN".GetCustomItemWithID(), ID = "ALLYFLASH", Price = 1500 });
            shop.AddItem(new AmmoShopItem() { Item = ItemType.Ammo9x19, Amount = 50, ID = "9MM", Price = 10 });
            shop.AddItem(new AmmoShopItem() { Item = ItemType.Ammo762x39, Amount = 30, ID = "762", Price = 15 });
            shop.AddItem(new AmmoShopItem() { Item = ItemType.Ammo556x45, Amount = 30, ID = "556", Price = 15 });
            shop.AddItem(new AmmoShopItem() { Item = ItemType.Ammo12gauge, Amount = 20, ID = "12G", Price = 20 });
            shop.AddItem(new AmmoShopItem() { Item = ItemType.Ammo44cal, Amount = 20, ID = "44C", Price = 10 });
            shop.AddItem(new RegularShopItem() { Item = ItemType.GunCOM15, ID = "C15", Price = 100 });
            shop.AddItem(new RegularShopItem() { Item = ItemType.GunCOM18, ID = "C18", Price = 300 });
            shop.AddItem(new RegularShopItem() { Item = ItemType.GunCrossvec, ID = "CV", Price = 650 });
            shop.AddItem(new RegularShopItem() { Item = ItemType.GunE11SR, ID = "E11", Price = 1200 });
            shop.AddItem(new RegularShopItem() { Item = ItemType.GunFSP9, ID = "FSP", Price = 500 });
            shop.AddItem(new RegularShopItem() { Item = ItemType.GunAK, ID = "AK", Price = 1100 });
            shop.AddItem(new RegularShopItem() { Item = ItemType.GunFRMG0, ID = "FRMG", Price = 2500 });
            shop.AddItem(new RegularShopItem() { Item = ItemType.GunLogicer, ID = "LOGI", Price = 2300 });
            shop.AddItem(new RegularShopItem() { Item = ItemType.GunRevolver, ID = "REV", Price = 700 });
            shop.AddItem(new RegularShopItem() { Item = ItemType.GunShotgun, ID = "SG", Price = 1150 });
            shop.AddItem(new RegularShopItem() { Item = ItemType.GunCom45, ID = "C45", Price = 3000 });
            shop.AddItem(new RegularShopItem() { Item = ItemType.GunA7, ID = "A7", Price = 950 });
            shop.AddItem(new RegularShopItem() { Item = ItemType.GrenadeFlash, ID = "FLASH", Price = 500 });
            shop.AddItem(new RegularShopItem() { Item = ItemType.GrenadeHE, ID = "GRENADE", Price = 800 });
            shop.AddItem(new RegularShopItem() { Item = ItemType.SCP207, ID = "COLA", Price = 1200 });
            shop.AddItem(new RegularShopItem() { Item = ItemType.SCP1576, ID = "JUICE", Price = 1400 });
            shop.AddItem(new RegularShopItem() { Item = ItemType.Medkit, ID = "MEDKIT", Price = 200 });
            shop.AddItem(new RegularShopItem() { Item = ItemType.Adrenaline, ID = "ADR", Price = 300 });
            shop.AddItem(new RegularShopItem() { Item = ItemType.Painkillers, ID = "PILL", Price = 100 });
            shop.AddItem(new RegularShopItem() { Item = ItemType.SCP500, ID = "PANACEA", Price = 800 });
            shop.AddItem(new RegularShopItem() { Item = ItemType.SCP330, ID = "CANDY", Price = 300 });

            ShopManager.RegisterProfile(shop);
            ShopManager.SetProfileActive(shop, true);
        }
    }
}
