using BepInEx.Configuration;
using RoR2;
using RoR2.UI;
using RoR2.UI.SkinControllers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MiscModpackUtils.Patches
{
    public class PauseScreen
    {
        public static ConfigEntry<string> OverridesRaw;
        public static List<string> Overrides = [];
        public static void Init()
        {
            OverridesRaw = Main.Config.Bind("Overrides", "Pause Menu Overrides", "", "the words written in pause menu, separated by commas. pause menu buttons are rearranged in that order and everything else is deleted.");
            if (string.IsNullOrWhiteSpace(OverridesRaw.Value)) return;
            foreach (var entry in OverridesRaw.Value.Split(",").Where(x => !string.IsNullOrWhiteSpace(x))) Overrides.Add(entry.Trim().ToUpper());
            On.RoR2.UI.PauseScreenController.Awake += (orig, self) =>
            {
                orig(self);
                Dictionary<string, Transform> transforms = [];
                Transform controller = self.GetComponentInChildren<ButtonSkinController>().gameObject.transform.parent;
                for (var i = 0; i < controller.childCount; i++)
                {
                    Transform child = controller.GetChild(i);
                    if (!child || !child.gameObject.activeSelf) continue;
                    var text = child.GetComponentInChildren<HGTextMeshProUGUI>().text.Trim().ToUpper();
                    if (Overrides.Contains(text)) transforms.Add(text, child);
                    else child.gameObject.SetActive(false);
                }
                var l = Overrides.ToArray();
                for (var i = 0; i < l.Length; i++) transforms[l[i]].SetSiblingIndex(i); 
            };
        }
    }
}