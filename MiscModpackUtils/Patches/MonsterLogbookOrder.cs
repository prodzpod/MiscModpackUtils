using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MiscModpackUtils.Patches
{
    public class MonsterLogbookOrder
    {
        public static ConfigEntry<string> OverridesRaw;
        public static List<string> Overrides = [];
        public static void Init()
        {
            OverridesRaw = Main.Config.Bind("Reordering", "Monster Logbook Ordering", "", "monster names, separated by commas. ones that does not appear on the list keeps its original ordering.");
            if (string.IsNullOrWhiteSpace(OverridesRaw.Value)) return;
            foreach (var entry in OverridesRaw.Value.Split(",").Where(x => !string.IsNullOrWhiteSpace(x))) Overrides.Add(entry.Trim().ToUpper());
            On.RoR2.UI.LogBook.LogBookController.BuildMonsterEntries += (orig, self) =>
            {
                var ret = orig(self).ToList();
                var l = Overrides.ToArray(); l.Reverse();
                foreach (var entry in l)
                {
                    var item = ret.Find(x => x.nameToken.Trim().ToUpper() == entry);
                    ret.Remove(item); ret.Insert(0, item);
                }
                return ret.ToArray();
            };
        }
    }
}