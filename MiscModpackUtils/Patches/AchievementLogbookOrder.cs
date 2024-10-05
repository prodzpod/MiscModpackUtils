using BepInEx.Configuration;
using RoR2;
using System.Collections.Generic;
using System.Linq;

namespace MiscModpackUtils.Patches
{
    public class AchievementLogbookOrder
    {
        public static ConfigEntry<string> OverridesRaw;
        public static List<string> Overrides = [];
        public static void Init()
        {
            OverridesRaw = Main.Config.Bind("Reordering", "Achievement Logbook Ordering", "", "achievement names, separated by commas. ones that does not appear on the list keeps its original ordering.");
            if (string.IsNullOrWhiteSpace(OverridesRaw.Value)) return;
            foreach (var entry in OverridesRaw.Value.Split(",").Where(x => !string.IsNullOrWhiteSpace(x))) Overrides.Add(entry.Trim().ToUpper());
            On.RoR2.UnlockableCatalog.SetUnlockableDefs += (orig, unlockableDefs) =>
            {
                orig(unlockableDefs);
                for (var i = 0; i < UnlockableCatalog.unlockableCount; i++)
                {
                    UnlockableDef def = UnlockableCatalog.GetUnlockableDef((UnlockableIndex)i);
                    int idx = Overrides.IndexOf(def.nameToken.Trim().ToUpper());
                    if (idx != -1) def.sortScore = idx; // vertical align
                    // txt += "Unlockable " + def.cachedName + ": " + def.sortScore + "\n";
                }
            };
        }
    }
}