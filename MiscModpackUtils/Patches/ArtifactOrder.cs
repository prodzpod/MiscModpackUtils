using BepInEx.Configuration;
using MonoMod.Cil;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MiscModpackUtils.Patches
{
    public class ArtifactOrder
    {
        public static ConfigEntry<string> OverridesRaw;
        public static List<string> Overrides = [];
        public static void Init()
        {
            OverridesRaw = Main.Config.Bind("Reordering", "Artifact Ordering", "", "artifact names, separated by commas. ones that does not appear on the list keeps its original ordering.");
            if (string.IsNullOrWhiteSpace(OverridesRaw.Value)) return;
            foreach (var entry in OverridesRaw.Value.Split(",").Where(x => !string.IsNullOrWhiteSpace(x))) Overrides.Add(entry.Trim().ToUpper());
            IL.RoR2.RuleCatalog.Init += il =>
            {
                List<ArtifactIndex> idxs = [];
                ILCursor c = new(il);
                c.EmitDelegate(() =>
                {
                    foreach (var entry in Overrides)
                    {
                        var idx = ArtifactCatalog.artifactDefs.ToList().FindIndex(x => x.nameToken.Trim().ToUpper() == entry);
                        if (idx == -1) { Main.Log.LogWarning($"Entry {entry} does not exist!"); continue; }
                        idxs.Add(ArtifactCatalog.artifactDefs[idx].artifactIndex);
                    }
                    foreach (var entry in ArtifactCatalog.artifactDefs) if (!idxs.Contains(entry.artifactIndex)) idxs.Add(entry.artifactIndex);
                });
                c.GotoNext(x => x.MatchCallOrCallvirt(typeof(RuleDef), nameof(RuleDef.FromArtifact)));
                c.EmitDelegate<Func<ArtifactIndex, ArtifactIndex>>(i => idxs[(int)i]);
            };
        }
    }
}