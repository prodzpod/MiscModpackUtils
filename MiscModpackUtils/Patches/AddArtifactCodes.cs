using BepInEx.Configuration;
using R2API.ScriptableObjects;
using R2API;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RoR2;
using System.Globalization;
using RoR2.Achievements;
using BepInEx;

namespace MiscModpackUtils.Patches
{
    public class AddArtifactCodes
    {
        public static ConfigEntry<string> OverridesRaw;
        public static Dictionary<string, string> Overrides = [];
        public static Sprite artifactBG;
        public static void Init()
        {
            OverridesRaw = Main.Config.Bind("General", "Add Artifact Codes", "", "FROM=TO, artifact names to code. X = Empty, C = Circle, R = Rectangle, T = Triangle, D = Diamond, S = Star(SS2), G = Gene(GeneticsArtifact)");
            if (string.IsNullOrWhiteSpace(OverridesRaw.Value)) return;
            foreach (var _entry in OverridesRaw.Value.Split(",").Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                var entry = _entry.Split("=");
                if (entry.Length != 2) { Main.Log.LogWarning("Entry is malformed, skipping: " + _entry); continue; }
                Overrides.Add(entry[0].Trim().ToUpper(), entry[1].Trim().ToUpper());
            }
            var bundle = AssetBundle.LoadFromFile(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Main.pluginInfo.Location), "miscmodpackutils"));
            artifactBG = bundle.LoadAsset<Sprite>("assets/artifactbg.png");
        }
        public static bool patched = false;
        public static void OnBasicallyEverythingLoaded()
        {
            if (patched || string.IsNullOrWhiteSpace(OverridesRaw.Value)) return;
            var artifacts = ArtifactCatalog.artifactDefs.ToList();
            foreach (var key in Overrides.Keys)
            {
                var idx = artifacts.FindIndex(x => x.nameToken.Trim().ToUpper() == key);
                if (idx == -1) { Main.Log.LogWarning($"key {key} is not valid!"); continue; }
                UnlockableDef unlockableDef = ScriptableObject.CreateInstance<UnlockableDef>();
                unlockableDef.cachedName = "Artifacts." + artifacts[idx].cachedName;
                artifacts[idx].unlockableDef = unlockableDef;
                ContentAddition.AddUnlockableDef(unlockableDef);
                AddCode(artifacts[idx], Overrides[key].Trim().ToUpper());
                AchievementDef achievement = new()
                {
                    identifier = "ObtainArtifact" + artifacts[idx].cachedName,
                    unlockableRewardIdentifier = unlockableDef.cachedName,
                    prerequisiteAchievementIdentifier = null,
                    nameToken = "ACHIEVEMENT_" + ("ObtainArtifact" + artifacts[idx].cachedName).ToUpper(CultureInfo.InvariantCulture) + "_NAME",
                    descriptionToken = "ACHIEVEMENT_" + ("ObtainArtifact" + artifacts[idx].cachedName).ToUpper(CultureInfo.InvariantCulture) + "_DESCRIPTION",
                    type = typeof(BaseAchievement),
                    serverTrackerType = null,
                    lunarCoinReward = 3
                };
                unlockableDef.achievementIcon = Utils.GetComposite(artifactBG.texture, artifacts[idx].smallIconSelectedSprite.texture);
                achievement.SetAchievedIcon(unlockableDef.achievementIcon);
                AchievementManager.achievementDefs = [.. AchievementManager.achievementDefs, achievement];
                AchievementManager.achievementIdentifiers.Add(achievement.identifier);
                AchievementManager.achievementNamesToDefs.Add(achievement.identifier, achievement);
                unlockableDef.getHowToUnlockString = () => Language.GetStringFormatted("UNLOCK_VIA_ACHIEVEMENT_FORMAT", Language.GetString(achievement.nameToken), Language.GetString(achievement.descriptionToken));
                unlockableDef.getUnlockedString = () => Language.GetStringFormatted("UNLOCKED_FORMAT", Language.GetString(achievement.nameToken), Language.GetString(achievement.descriptionToken));
            }
            Main.Log.LogInfo($"Added {Overrides.Keys.Count} artifact codes!");
            patched = true;
        }
        public static Dictionary<char, int> Codes = new() {
            { 'X', 11 },
            { 'C', 1 },
            { 'R', 7 },
            { 'T', 3 },
            { 'D', 5 },
            { 'S', 4 },
            { 'G', 15 },
        };
        public static void AddCode(ArtifactDef def, string code)
        {
            if (code == null || code.Length != 9) { Main.Log.LogWarning("Artifact code is malformed: " + code + ", skipping"); return; }
            var artifactCode = ScriptableObject.CreateInstance<ArtifactCode>();
            artifactCode.topRow = new Vector3Int(Codes[code[0]], Codes[code[1]], Codes[code[2]]);
            artifactCode.middleRow = new Vector3Int(Codes[code[3]], Codes[code[4]], Codes[code[5]]);
            artifactCode.bottomRow = new Vector3Int(Codes[code[6]], Codes[code[7]], Codes[code[8]]);
            ArtifactCodeAPI.AddCode(def, artifactCode);
        }
    }
}
