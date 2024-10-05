using BepInEx.Configuration;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

namespace MiscModpackUtils.Patches
{
    public class ItemLookSwap
    {
        public static ConfigEntry<string> OverridesRaw;
        public static Dictionary<string, string> Overrides = [];

        public static void Init()
        {
            OverridesRaw = Main.Config.Bind("Overrides", "Item Look Overrides", "", "FROM=TO, item name to another item name. swaps 1 item's pickup, icon and name with another.");
            if (string.IsNullOrWhiteSpace(OverridesRaw.Value)) return;
            foreach (var _entry in OverridesRaw.Value.Split(",").Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                var entry = _entry.Split("=");
                if (entry.Length != 2) { Main.Log.LogWarning("Entry is malformed, skipping: " + _entry); continue; }
                Overrides.Add(entry[0].Trim().ToUpper(), entry[1].Trim().ToUpper());
            }
        }

        public static bool patched = false;
        public static void OnBasicallyEverythingLoaded()
        {
            if (patched || string.IsNullOrWhiteSpace(OverridesRaw.Value)) return;
            Dictionary<string, string> nameTokens = [];
            Dictionary<string, string> loreTokens = [];
            Dictionary<string, Sprite> icons = [];
            Dictionary<string, Texture> textures = [];
            Dictionary<string, GameObject> modelPrefabs = [];
            Dictionary<string, GameObject?> dropletDisplayPrefabs = [];
            Dictionary<string, UnlockableDef> unlockables = [];
            var pickup = PickupCatalog.allPickups.ToList();
            foreach (var key in Overrides.Keys)
            {
                var idx = pickup.FindIndex(x => x.nameToken.Trim().ToUpper() == key);
                if (idx == -1) { Main.Log.LogWarning($"key {key} is not valid!"); continue; }
                var name = pickup[idx].nameToken.Trim().ToUpper();
                if (pickup[idx].itemIndex != ItemIndex.None)
                {
                    var item = ItemCatalog.GetItemDef(pickup[idx].itemIndex);
                    nameTokens.Add(name, item.nameToken);
                    loreTokens.Add(name, item.loreToken);
                    icons.Add(name, item.pickupIconSprite);
                    textures.Add(name, item.pickupIconTexture);
                    modelPrefabs.Add(name, item.pickupModelPrefab);
                    dropletDisplayPrefabs.Add(name, item._itemTierDef?.dropletDisplayPrefab);
                }
                else if (pickup[idx].equipmentIndex != EquipmentIndex.None)
                {
                    var equip = EquipmentCatalog.GetEquipmentDef(pickup[idx].equipmentIndex);
                    nameTokens.Add(name, equip.nameToken);
                    loreTokens.Add(name, equip.loreToken);
                    icons.Add(name, equip.pickupIconSprite);
                    textures.Add(name, equip.pickupIconTexture);
                    modelPrefabs.Add(name, equip.pickupModelPrefab);
                    dropletDisplayPrefabs.Add(name, EquipmentDef.dropletDisplayPrefab);
                }
                else { Main.Log.LogWarning($"key {key} is not valid!"); continue; }
                if (pickup[idx].unlockableDef) unlockables.Add(name, pickup[idx].unlockableDef);
            }
            foreach (var key in Overrides.Keys)
            {
                var idx = pickup.FindIndex(x => x.nameToken.Trim().ToUpper() == key);
                if (idx == -1) continue;
                var from = pickup[idx].nameToken.Trim().ToUpper();
                idx = pickup.FindIndex(x => x.nameToken.Trim().ToUpper() == Overrides[key]);
                if (idx == -1) { Main.Log.LogWarning($"key {key} is not valid!"); continue; }
                pickup[idx].nameToken = nameTokens[from];
                pickup[idx].iconSprite = icons[from];
                pickup[idx].iconTexture = textures[from];
                pickup[idx].displayPrefab = modelPrefabs[from];
                pickup[idx].dropletDisplayPrefab = dropletDisplayPrefabs[from];
                if (unlockables.TryGetValue(key, out var unlockable)) pickup[idx].unlockableDef = unlockable;
                else pickup[idx].unlockableDef = null;
                if (pickup[idx].itemIndex != ItemIndex.None)
                {
                    var item = ItemCatalog.GetItemDef(pickup[idx].itemIndex);
                    item.nameToken = nameTokens[from];
                    item.loreToken = loreTokens[from];
                    item.pickupIconSprite = icons[from];
                    item.pickupModelPrefab = modelPrefabs[from];
                    item.unlockableDef = pickup[idx].unlockableDef;
                }
                else if (pickup[idx].equipmentIndex != EquipmentIndex.None)
                {
                    var equip = EquipmentCatalog.GetEquipmentDef(pickup[idx].equipmentIndex);
                    equip.nameToken = nameTokens[from];
                    equip.loreToken = loreTokens[from];
                    equip.pickupIconSprite = icons[from];
                    equip.pickupModelPrefab = modelPrefabs[from];
                    equip.unlockableDef = pickup[idx].unlockableDef;
                }
                else { Main.Log.LogWarning($"key {key} is not valid!"); continue; }
            }
            foreach (var key in Overrides.Keys) if (!Overrides.Values.Contains(key))
            {
                var idx = pickup.FindIndex(x => x.nameToken.Trim().ToUpper() == key);
                if (idx == -1) continue;
                pickup[idx].unlockableDef = null;
                if (pickup[idx].itemIndex != ItemIndex.None) ItemCatalog.GetItemDef(pickup[idx].itemIndex).unlockableDef = null;
                if (pickup[idx].equipmentIndex != EquipmentIndex.None) EquipmentCatalog.GetEquipmentDef(pickup[idx].equipmentIndex).unlockableDef = null;
            }
            patched = true;
        }
    }
}