using BepInEx.Configuration;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MiscModpackUtils.Patches
{
    public class EquipmentCooldown
    {
        public static ConfigEntry<string> OverridesRaw;
        public static Dictionary<string, float> Overrides = [];
        public static void Init()
        {
            OverridesRaw = Main.Config.Bind("Overrides", "Equipment Cooldown Overrides", "", "FROM=TO, FROM equipment's cooldown will be set to TO in seconds.");
        }

        public static bool patched = false;
        [SystemInitializer(typeof(EquipmentCatalog))]
        public static void SI()
        {
            if (patched || string.IsNullOrWhiteSpace(OverridesRaw.Value)) return;
            foreach (var _entry in OverridesRaw.Value.Split(",").Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                var entry = _entry.Split("=");
                if (entry.Length != 2 || !float.TryParse(entry[1], out var f)) { Main.Log.LogWarning("Entry is malformed, skipping: " + _entry); continue; }
                Overrides.Add(entry[0].Trim().ToUpper(), f);
            }
            foreach (var key in Overrides.Keys)
            {
                var def = EquipmentCatalog.equipmentDefs.FirstOrDefault(x => x.nameToken.Trim().ToUpper() == key);
                if (def == default) { Main.Log.LogWarning("Equipment " + key + " does not exist, skipping"); continue; }
                def.cooldown = Overrides[key];
            }
            Main.Log.LogInfo($"Patched {Overrides.Keys.Count} equipment cooldowns!");
            patched = true;
        }
    }
}