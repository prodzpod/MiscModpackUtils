using BepInEx.Configuration;
using HarmonyLib;
using MonoMod.Cil;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using R2API;

namespace MiscModpackUtils.Patches
{
    public class EliteLookSwap
    {
        public static ConfigEntry<string> OverridesRaw;
        public static Dictionary<string, string> Overrides = [];
        public static void Init()
        {
            OverridesRaw = Main.Config.Bind("Overrides", "Elite Loop Swap", "", "FROM=TO, elite names, separated by commas. Elite ramp and crown from TO will be replaced with the one from FROM. see the log for list of valid input for your pack.");
            if (string.IsNullOrWhiteSpace(OverridesRaw.Value)) return;
            foreach (var _entry in OverridesRaw.Value.Split(",").Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                var entry = _entry.Split("=");
                if (entry.Length != 2) { Main.Log.LogWarning("Entry is malformed, skipping: " + _entry); continue; }
                Overrides.Add(entry[0].Trim().ToUpper(), entry[1].Trim().ToUpper());
            }
            Main.Harmony.PatchAll(typeof(PatchR2API));
            On.RoR2.CharacterModel.SetEquipmentDisplay += (orig, self, idx) =>
            {
                var def = EquipmentCatalog.GetEquipmentDef(idx);
                if (def != null && Overrides.ContainsKey(def.name.ToUpper())) orig(self, EquipmentCatalog.equipmentDefs.FirstOrDefault(x => x.name.ToUpper() == Overrides[def.name.ToUpper()])?.equipmentIndex ?? idx);
                else orig(self, idx);
            };
        }

        [HarmonyPatch]
        public class PatchR2API
        {
            public static void ILManipulator(ILContext il, MethodBase original, ILLabel retLabel)
            {
                ILCursor c = new(il);
                while (c.TryGotoNext(MoveType.After, x => x.MatchLdfld<CharacterModel>(nameof(CharacterModel.myEliteIndex))))
                    c.EmitDelegate<Func<EliteIndex, EliteIndex>>(idx =>
                    {
                        var def = EliteCatalog.GetEliteDef(idx);
                        if (def == null) return idx;
                        if (Overrides.ContainsKey(def.eliteEquipmentDef.name.ToUpper()))
                            return (EquipmentCatalog.equipmentDefs.FirstOrDefault(x => x.name.ToUpper() == Overrides[def.eliteEquipmentDef.name.ToUpper()]) ?? def.eliteEquipmentDef).passiveBuffDef.eliteDef.eliteIndex;
                        return idx;
                    });
            }

            public static MethodBase TargetMethod()
            {
                return AccessTools.GetDeclaredMethods(typeof(EliteRamp)).Find(x => x.Name.StartsWith($"<{nameof(EliteRamp.UpdateRampProperly)}>g"));
            }
        }
    }
}
