using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MiscModpackUtils.Patches
{
    public class DifficultyOrder
    {
        public static ConfigEntry<string> OverridesRaw;
        public static List<string> Overrides = [];
        public static void Init()
        {
            OverridesRaw = Main.Config.Bind("Reordering", "Difficulty Ordering", "", "difficulty names, separated by commas. ones that does not appear on the list keeps its original ordering.");
            if (string.IsNullOrWhiteSpace(OverridesRaw.Value)) return;
            foreach (var entry in OverridesRaw.Value.Split(",").Where(x => !string.IsNullOrWhiteSpace(x))) Overrides.Add(entry.Trim().ToUpper());
            On.RoR2.RuleDef.FromDifficulty += (orig) =>
            {
                RuleDef ruleDef = orig();
                var l = Overrides.ToList(); l.Reverse();
                foreach (var entry in l)
                {
                    var idx = ruleDef.choices.FindIndex(x => DifficultyAPI.difficultyDefinitions[x.difficultyIndex]?.nameToken.Trim().ToUpper() == entry);
                    if (idx == -1) { Main.Log.LogWarning($"Entry {entry} does not exist!"); continue; }
                    var r = ruleDef.choices[idx]; ruleDef.choices.RemoveAt(idx); ruleDef.choices.Insert(0, r);
                }
                return ruleDef;
            };
        }
    }
}