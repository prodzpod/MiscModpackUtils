using BepInEx.Configuration;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MiscModpackUtils.Patches
{
    public class SurvivorLogbookOrder
    {
        public static ConfigEntry<string> OverridesRaw;
        public static List<string> Overrides = [];
        public static void Init()
        {
            OverridesRaw = Main.Config.Bind("Reordering", "Survivor Logbook Ordering", "", "survivor names, separated by commas. ones that does not appear on the list keeps its original ordering.");
            if (string.IsNullOrWhiteSpace(OverridesRaw.Value)) return;
            foreach (var entry in OverridesRaw.Value.Split(",").Where(x => !string.IsNullOrWhiteSpace(x))) Overrides.Add(entry.Trim().ToUpper());
            On.RoR2.UI.LogBook.LogBookController.BuildSurvivorEntries += (orig, self) =>
            {
                var ret = orig(self).ToList();
                var l = Overrides.ToArray();
                foreach (var entry in l.Reverse())
                {
                    var item = ret.Find(x => SurvivorCatalog.GetSurvivorDef(SurvivorCatalog.GetSurvivorIndexFromBodyIndex(((CharacterBody)x.extraData).bodyIndex)).cachedName.Trim().ToUpper() == entry);
                    ret.Remove(item); ret.Insert(0, item);
                }
                return ret.Where(x => x != null).ToArray();
            };
        }
    }
}