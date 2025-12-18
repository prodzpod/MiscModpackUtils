using BepInEx.Configuration;
using HarmonyLib;
using RoR2;
using RoR2.ExpansionManagement;
using RoR2.Skills;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MiscModpackUtils.Patches
{
    public class SkillOrder
    {
        public static ConfigEntry<string> OverridesRaw;
        public static List<string> Overrides = [];
        public static void Init()
        {
            OverridesRaw = Main.Config.Bind("Reordering", "Skill Ordering", "", "skill names, separated by commas. ones that does not appear on the list keeps its original ordering.");
            if (string.IsNullOrWhiteSpace(OverridesRaw.Value)) return;
            foreach (var entry in OverridesRaw.Value.Split(",").Where(x => !string.IsNullOrWhiteSpace(x))) Overrides.Add(entry.Trim().ToUpper());
            On.RoR2.UI.LoadoutPanelController.Row.FromSkillSlot += (orig, owner, bodyIndex, skillSlotIndex, skillSlot) =>
            {
                var ret = orig(owner, bodyIndex, skillSlotIndex, skillSlot);
                var names = skillSlot.skillFamily.variants.Select(x => x.skillDef.skillNameToken).ToList();
                return TransformRow(ret, Overrides.ToArray().Reverse(), names);
            };
            On.RoR2.UI.LoadoutPanelController.Row.FromSkin += (orig, owner, bodyIndex) =>
            {
                var ret = orig(owner, bodyIndex);
                var names = SkinCatalog.GetBodySkinDefs(bodyIndex).Select(x => x.nameToken).ToList();
                return TransformRow(ret, Overrides.ToArray().Reverse(), names);
            };
        }
        public static LoadoutPanelController.Row TransformRow(LoadoutPanelController.Row ret, IEnumerable<string> overrides, List<string> names)
        {
            foreach (var entry in overrides)
            {
                var idx = names.IndexOf(entry);
                if (idx == -1) continue;
                var item = ret.rowData.Find(x => x.defIndex == idx);
                ret.rowData.Remove(item); ret.rowData.Insert(0, item);
                item.button.transform.SetAsFirstSibling();
            }
            return ret;
        }
    }
}