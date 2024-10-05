using BepInEx.Configuration;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MiscModpackUtils.Patches
{
    public class SkillAchievementSwap
    {
        public static ConfigEntry<string> OverridesRaw;
        public static Dictionary<string, string> Overrides = [];
        public static void Init()
        {
            OverridesRaw = Main.Config.Bind("Overrides", "Skill Achievement Override", "", "FROM=TO, skill or skin to achievement name. also swaps icons.");
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
            List<SkillFamily> _skillfamily = SkillCatalog.allSkillFamilies.ToList();
            var _skin = SkinCatalog.allSkinDefs.ToList();
            foreach (var key in Overrides.Keys)
            {
                var idx = _unlock.FindIndex(x => x.nameToken.Trim().ToUpper() == Overrides[key]);
                var unlock = idx == -1 ? null : _unlock[idx];
                SkillFamily skillfamily = null; SkinDef skin = null;
                idx = _skillfamily.FindIndex(x => x.variants.Any(y => y.skillDef.skillNameToken.Trim().ToUpper() == key));
                if (idx != -1) skillfamily = _skillfamily[idx];
                idx = _skin.FindIndex(x => x.nameToken.Trim().ToUpper() == key);
                if (idx != -1) skin = _skin[idx];
                if (!skillfamily && !skin) { Main.Log.LogWarning($"key {key} is not valid!"); continue; }
                Sprite icon = null;
                if (skillfamily)
                {
                    idx = skillfamily.variants.ToList().FindIndex(x => x.skillDef.skillNameToken.Trim().ToUpper() == key);
                    skillfamily.variants[idx].unlockableDef = unlock;
                    skillfamily.variants[idx].unlockableName = unlock?.cachedName ?? "";
                    icon = skillfamily.variants[idx].skillDef.icon;
                }
                if (skin)
                {
                    skin.unlockableDef = unlock;
                    skin.unlockableName = unlock?.cachedName ?? "";
                    icon = skin.icon;
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