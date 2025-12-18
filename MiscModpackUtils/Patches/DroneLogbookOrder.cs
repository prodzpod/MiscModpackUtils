using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MiscModpackUtils.Patches
{
    public class DroneLogbookOrder
    {
        public static ConfigEntry<string> OverridesRaw;
        public static List<string> Overrides = [];
        public static void Init()
        {
            OverridesRaw = Main.Config.Bind("Reordering", "Drone Logbook Ordering", "", "drone names, separated by commas. ones that does not appear on the list keeps its original ordering.");
            if (string.IsNullOrWhiteSpace(OverridesRaw.Value)) return;
            foreach (var entry in OverridesRaw.Value.Split(",").Where(x => !string.IsNullOrWhiteSpace(x))) Overrides.Add(entry.Trim().ToUpper());
            On.RoR2.UI.LogBook.LogBookController.BuildDroneEntries += (orig, self) =>
            {
                var ret = orig(self).ToList();
                var l = Overrides.ToArray();
                foreach (var entry in l.Reverse())
                {
                    var item = ret.Find(x => x.nameToken.Trim().ToUpper() == entry);
                    ret.Remove(item); ret.Insert(0, item);
                }
                return ret.Where(x => x != null).ToArray();
            };
        }
    }
}