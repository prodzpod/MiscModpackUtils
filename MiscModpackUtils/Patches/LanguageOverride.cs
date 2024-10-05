using BepInEx;
using BepInEx.Configuration;
using R2API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MiscModpackUtils.Patches
{
    public class LanguageOverride
    {
        public static void Init()
        {
            string[] overlays = [..Directory.GetFiles(Paths.PluginPath, "*.overlaylanguage", SearchOption.AllDirectories),
            ..Directory.GetFiles(Paths.ConfigPath, "*.overlaylanguage", SearchOption.AllDirectories)];
            foreach (string overlay in overlays) LanguageAPI.AddOverlayPath(overlay);
            Main.Log.LogInfo($"Added {overlays.Length} .overlanguage files");
        }
    }
}