using BepInEx.Configuration;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace MiscModpackUtils.Patches
{
    public class PickupLogbookOrder
    {
        public static ConfigEntry<string> OverridesRaw;
        public static List<string> Overrides = [];
        public static void Init()
        {
            OverridesRaw = Main.Config.Bind("Reordering", "Pickup Logbook Ordering", "", "achievement names, separated by commas. ones that does not appear on the list keeps its original ordering.");
            if (string.IsNullOrWhiteSpace(OverridesRaw.Value)) return;
            foreach (var entry in OverridesRaw.Value.Split(",").Where(x => !string.IsNullOrWhiteSpace(x))) Overrides.Add(entry.Trim().ToUpper());
            On.RoR2.UI.LogBook.LogBookController.BuildPickupEntries += (orig, self) =>
            {
                var ret = orig(self).ToList();
                var l = Overrides.ToArray();
                foreach (var entry in l.Reverse())
                {
                    Main.Log.LogInfo("adding:" + entry);
                    var item = ret.Find(x => x.nameToken.Trim().ToUpper() == entry);
                    ret.Remove(item); ret.Insert(0, item);
                }
                Main.Log.LogInfo(ret.Select(x => x.nameToken).Join());
                return ret.Where(x => x != null).ToArray();
            };
        }
    }
}