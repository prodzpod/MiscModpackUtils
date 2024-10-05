using BepInEx.Configuration;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MiscModpackUtils.Patches
{
    public class ItemAchievementSwap
    {
        public static ConfigEntry<string> OverridesRaw;
        public static Dictionary<string, string> Overrides = [];
        public static void Init()
        {
            OverridesRaw = Main.Config.Bind("Overrides", "Item Achievement Override", "", "FROM=TO, item names to achievement name. also swaps icons.");
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
            List<UnlockableDef> remapped = [];
            var _unlock = UnlockableCatalog.indexToDefTable.ToList();
            var _pickup = PickupCatalog.allPickups.ToList();
            foreach (var key in Overrides.Keys)
            {
                var idx = _pickup.FindIndex(x => x.nameToken.Trim().ToUpper() == key);
                if (idx == -1) { Main.Log.LogWarning($"key {key} is not valid!"); continue; }
                var pickup = _pickup[idx]; var name = pickup.nameToken.Trim().ToUpper();
                idx = _unlock.FindIndex(x => x.nameToken.Trim().ToUpper() == Overrides[key]);
                var unlock = idx == -1 ? null : _unlock[idx];
                pickup.unlockableDef = unlock;
                Sprite icon = null;
                if (pickup.itemIndex != ItemIndex.None)
                {
                    var item = ItemCatalog.GetItemDef(pickup.itemIndex);
                    item.unlockableDef = unlock;
                    icon = Utils.GetComposite(item._itemTierDef.bgIconTexture as Texture2D, item.pickupIconSprite.texture);
                }
                else if (pickup.equipmentIndex != EquipmentIndex.None)
                {
                    var equip = EquipmentCatalog.GetEquipmentDef(pickup.equipmentIndex);
                    equip.unlockableDef = unlock;
                    icon = Utils.GetComposite(equip.bgIconTexture as Texture2D, equip.pickupIconSprite.texture);
                }
                else { Main.Log.LogWarning($"key {key} is not valid!"); continue; }
                if (unlock && !remapped.Contains(unlock) && AchievementManager.GetAchievementDef(unlock.cachedName) != null)
                {
                    remapped.Add(unlock);
                    var achievement = AchievementManager.GetAchievementDef(unlock.cachedName);
                    achievement.SetAchievedIcon(icon);
                }
            }
            patched = true;
        }
    }
}